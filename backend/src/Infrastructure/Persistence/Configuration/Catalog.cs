using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Domain.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class BrandConfig : BaseAuditableTenantEntityConfiguration<Brand>
{
  public override void Configure(EntityTypeBuilder<Brand> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Name)
      .HasMaxLength(256);
  }
}

public sealed class ProductConfig : BaseAuditableTenantEntityConfiguration<Product>
{
  public override void Configure(EntityTypeBuilder<Product> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Name)
      .HasMaxLength(256);

    builder
      .Property(b => b.Rate)
      .HasPrecision(7, 3);

    builder
      .Property(p => p.ImagePath)
      .HasMaxLength(2048);
  }
}

public sealed class ServiceCatalogConfig : BaseAuditableTenantEntityConfiguration<ServiceCatalog>
{
  public override void Configure(EntityTypeBuilder<ServiceCatalog> builder)
  {
    base.Configure(builder);

    builder.Property(a => a.Priority)
      .HasConversion(
        p => p.Name,
        p => ServicePriority.FromValue(p));

    builder
      .Property(b => b.Price)
      .IsRequired()
      .HasPrecision(7, 3);

    builder
      .Property(a => a.Priority)
      .HasConversion<string>()
      .HasDefaultValue(ServicePriority.Normal);
  }
}

public sealed class ServiceConfig : BaseAuditableTenantEntityConfiguration<Service>
{
  public override void Configure(EntityTypeBuilder<Service> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Name)
      .HasMaxLength(1024);

    builder
      .Property(p => p.ImagePath)
      .HasMaxLength(2048);
  }
}