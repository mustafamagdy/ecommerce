using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.Name)
      .HasMaxLength(1024);
  }
}

public class OrderConfig : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.IsMultiTenant();
  }
}

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    builder.IsMultiTenant();

    builder
      .Property(b => b.ServiceName)
      .IsRequired()
      .HasMaxLength(256);

    builder
      .Property(b => b.ProductName)
      .IsRequired()
      .HasMaxLength(256);

    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);

    builder
      .Property(b => b.VatPercent)
      .HasPrecision(7, 3);
  }
}