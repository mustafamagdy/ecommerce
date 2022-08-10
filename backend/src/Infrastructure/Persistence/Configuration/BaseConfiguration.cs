using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public abstract class BaseEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
  where TEntity : BaseEntity<TKey>
{
  public virtual void Configure(EntityTypeBuilder<TEntity> builder)
  {
    builder.HasKey(a => a.Id);
    builder.Property(a => a.Id).ValueGeneratedNever();
  }
}

public abstract class BaseAuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
  where TEntity : AuditableEntity
{
  public virtual void Configure(EntityTypeBuilder<TEntity> builder)
  {
    builder.HasKey(a => a.Id);
    builder.Property(a => a.Id).ValueGeneratedNever();
  }
}

public abstract class BaseAuditableTenantEntityConfiguration<TEntity> : BaseAuditableEntityConfiguration<TEntity>
  where TEntity : AuditableEntity
{
  public override void Configure(EntityTypeBuilder<TEntity> builder)
  {
    base.Configure(builder);
    builder.IsMultiTenant();
  }
}

public abstract class BaseTenantEntityConfiguration<TEntity, TKey> : BaseEntityConfiguration<TEntity, TKey>
  where TEntity : BaseEntity<TKey>
{
  public override void Configure(EntityTypeBuilder<TEntity> builder)
  {
    base.Configure(builder);
    builder.IsMultiTenant();
  }
}