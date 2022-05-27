using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class BrandConfig : IEntityTypeConfiguration<Brand>
{
  public void Configure(EntityTypeBuilder<Brand> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.Name)
      .HasMaxLength(256);
  }
}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.Name)
      .HasMaxLength(1024);

    builder
      .Property(p => p.ImagePath)
      .HasMaxLength(2048);
  }
}


public class ServiceCatalogConfig : IEntityTypeConfiguration<ServiceCatalog>
{
  public void Configure(EntityTypeBuilder<ServiceCatalog> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.Price)
      .IsRequired()
      .HasPrecision(4);

    builder
      .Property(a => a.Priority)
      .HasConversion<string>()
      .HasDefaultValue(ServicePriority.Normal);
  }
}

public class ServiceConfig : IEntityTypeConfiguration<Service>
{
  public void Configure(EntityTypeBuilder<Service> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.Name)
      .HasMaxLength(1024);

    builder
      .Property(p => p.ImagePath)
      .HasMaxLength(2048);
  }
}