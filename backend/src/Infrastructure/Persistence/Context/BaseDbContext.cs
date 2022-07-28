using System.Data;
using System.Diagnostics;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Infrastructure.Auditing;
using FSH.WebApi.Infrastructure.Identity;
using FSH.WebApi.Infrastructure.Multitenancy;
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
  private readonly ISubscriptionInfo? _currentSubscriptionType;
  private readonly TenantDbContext _tenantDb;
  private readonly ICurrentUser _currentUser;
  private readonly ISerializerService _serializer;
  private readonly ITenantConnectionStringBuilder _csBuilder;
  private readonly DatabaseSettings _dbSettings;

  protected BaseDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser,
    ISerializerService serializer, ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings,
    ISubscriptionInfo currentSubscriptionType, TenantDbContext tenantDb)
    : base(currentTenant, options)
  {
    _currentTenant = currentTenant;
    _currentUser = currentUser;
    _serializer = serializer;
    _csBuilder = csBuilder;
    _dbSettings = dbSettings.Value;
    _currentSubscriptionType = currentSubscriptionType;
    _tenantDb = tenantDb;
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

    string connectionString = string.Empty;
    if (_currentTenant != null && _currentSubscriptionType != null)
    {
      var subscriptionType = _currentSubscriptionType.SubscriptionType;
      var tenantId = _currentTenant.Id;
      var tenant = _tenantDb.TenantInfo.Find(tenantId);
      connectionString = subscriptionType.Name switch
      {
        nameof(SubscriptionType.Standard) => tenant.ConnectionString,
        nameof(SubscriptionType.Demo) => tenant.DemoConnectionString,
        nameof(SubscriptionType.Train) => tenant.TrainConnectionString,
        _ => throw new ArgumentOutOfRangeException(subscriptionType.Name)
      };
    }

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
      optionsBuilder.UseDatabase(_dbSettings.DBProvider, TenantInfo?.ConnectionString!);
    }
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
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