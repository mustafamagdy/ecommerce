using System.Data;
using System.Diagnostics;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.Auditing;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Infrastructure.Auditing;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Extensions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public abstract class BaseDbContext
  : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>
{
  private readonly ITenantInfo? _currentTenant;
  private readonly ISubscriptionAccessor _currentSubscriptionType;
  private readonly ICurrentUser _currentUser;
  private readonly ISerializerService _serializer;
  private readonly ITenantConnectionStringBuilder _csBuilder;
  private readonly DatabaseSettings _dbSettings;
  private readonly ITenantConnectionStringResolver _tenantConnectionStringResolver;

  protected BaseDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser,
    ISerializerService serializer, ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings,
    ISubscriptionAccessor currentSubscriptionType, ITenantConnectionStringResolver tenantConnectionStringResolver)
    : base(currentTenant, options)
  {
    _currentTenant = currentTenant;
    _currentUser = currentUser;
    _serializer = serializer;
    _csBuilder = csBuilder;
    _dbSettings = dbSettings.Value;
    _currentSubscriptionType = currentSubscriptionType;
    _tenantConnectionStringResolver = tenantConnectionStringResolver;
  }

  // Used by Dapper
  public IDbConnection Connection => Database.GetDbConnection();

  public DbSet<Trail> AuditTrails => Set<Trail>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // QueryFilters need to be applied before base.OnModelCreating
    modelBuilder.AppendGlobalQueryFilter<ISoftDelete>(s => s.DeletedOn == null);

    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (_dbSettings.LogSensitiveInfo)
    {
      optionsBuilder.EnableDetailedErrors();
      optionsBuilder.EnableSensitiveDataLogging();
      optionsBuilder.LogTo(m => Debug.WriteLine(m), LogLevel.Information);
      optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
      optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    }

    TenantMismatchMode = TenantMismatchMode.Overwrite;
    TenantNotSetMode = TenantNotSetMode.Overwrite;

    var subscriptionType = _currentSubscriptionType.SubscriptionType;
    string connectionString = _currentTenant.Id != MultitenancyConstants.RootTenant.Id
      ? _tenantConnectionStringResolver.Resolve(_currentTenant.Id, subscriptionType)
      : _currentTenant.ConnectionString.IfNullOrEmpty(_dbSettings.ConnectionString);

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException($"Unable to create db context for tenant {_currentTenant.Id} for subscription {subscriptionType}");
    }

    optionsBuilder.UseDatabase(_dbSettings.DBProvider, connectionString);
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
  {
    var auditEntries = HandleAuditingBeforeSaveChanges(_currentUser.GetUserId());

    int result = await base.SaveChangesAsync(cancellationToken);

    await HandleAuditingAfterSaveChangesAsync(auditEntries, cancellationToken);

    return result;
  }

  private List<AuditTrail> HandleAuditingBeforeSaveChanges(Guid userId)
  {
    foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToList())
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.CreatedBy = userId;
          entry.Entity.LastModifiedBy = userId;
          break;

        case EntityState.Modified:
          entry.Entity.LastModifiedOn = DateTime.UtcNow;
          entry.Entity.LastModifiedBy = userId;
          break;

        case EntityState.Deleted:
          if (entry.Entity is ISoftDelete softDelete)
          {
            softDelete.DeletedBy = userId;
            softDelete.DeletedOn = DateTime.UtcNow;
            entry.State = EntityState.Modified;
          }

          break;
      }
    }

    ChangeTracker.DetectChanges();

    var trailEntries = new List<AuditTrail>();
    foreach (var entry in ChangeTracker.Entries<IAuditableEntity>()
               .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
               .ToList())
    {
      var trailEntry = new AuditTrail(entry, _serializer)
      {
        TableName = entry.Entity.GetType().Name,
        UserId = userId
      };
      trailEntries.Add(trailEntry);
      foreach (var property in entry.Properties)
      {
        if (property.IsTemporary)
        {
          trailEntry.TemporaryProperties.Add(property);
          continue;
        }

        string propertyName = property.Metadata.Name;
        if (property.Metadata.IsPrimaryKey())
        {
          trailEntry.KeyValues[propertyName] = property.CurrentValue;
          continue;
        }

        switch (entry.State)
        {
          case EntityState.Added:
            trailEntry.TrailType = TrailType.Create;
            trailEntry.NewValues[propertyName] = property.CurrentValue;
            break;

          case EntityState.Deleted:
            trailEntry.TrailType = TrailType.Delete;
            trailEntry.OldValues[propertyName] = property.OriginalValue;
            break;

          case EntityState.Modified:
            if (property.IsModified && entry.Entity is ISoftDelete && property.OriginalValue == null &&
                property.CurrentValue != null)
            {
              trailEntry.ChangedColumns.Add(propertyName);
              trailEntry.TrailType = TrailType.Delete;
              trailEntry.OldValues[propertyName] = property.OriginalValue;
              trailEntry.NewValues[propertyName] = property.CurrentValue;
            }
            else if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
            {
              trailEntry.ChangedColumns.Add(propertyName);
              trailEntry.TrailType = TrailType.Update;
              trailEntry.OldValues[propertyName] = property.OriginalValue;
              trailEntry.NewValues[propertyName] = property.CurrentValue;
            }

            break;
        }
      }
    }

    foreach (var auditEntry in trailEntries.Where(e => !e.HasTemporaryProperties))
    {
      AuditTrails.Add(auditEntry.ToAuditTrail());
    }

    return trailEntries.Where(e => e.HasTemporaryProperties).ToList();
  }

  private Task HandleAuditingAfterSaveChangesAsync(List<AuditTrail> trailEntries, CancellationToken cancellationToken = new())
  {
    if (trailEntries == null || trailEntries.Count == 0)
    {
      return Task.CompletedTask;
    }

    foreach (var entry in trailEntries)
    {
      foreach (var prop in entry.TemporaryProperties)
      {
        if (prop.Metadata.IsPrimaryKey())
        {
          entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
        }
        else
        {
          entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
        }
      }

      AuditTrails.Add(entry.ToAuditTrail());
    }

    return SaveChangesAsync(cancellationToken);
  }
}