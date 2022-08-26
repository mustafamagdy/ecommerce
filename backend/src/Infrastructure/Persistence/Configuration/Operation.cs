using FSH.WebApi.Domain.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class CustomerConfig : BaseTenantEntityConfiguration<Customer, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<Customer> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Name)
      .HasMaxLength(1024);

    builder
      .Property(b => b.PhoneNumber)
      .HasMaxLength(64);

    builder.HasIndex(a => a.PhoneNumber).IsUnique();

    var orders = builder.Metadata.FindNavigation(nameof(Customer.Orders));
    orders?.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class OrderConfig : BaseAuditableTenantEntityConfiguration<Order>
{
  public override void Configure(EntityTypeBuilder<Order> builder)
  {
    base.Configure(builder);

    builder
      .Property(a => a.OrderNumber)
      .HasMaxLength(64);

    builder.HasIndex(a => a.OrderNumber).IsUnique();

    var orderItems = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
    orderItems?.SetPropertyAccessMode(PropertyAccessMode.Field);

    var orderPayments = builder.Metadata.FindNavigation(nameof(Order.OrderPayments));
    orderPayments?.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class OrderItemConfig : BaseTenantEntityConfiguration<OrderItem, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    base.Configure(builder);


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

public class OrderPaymentConfig : BaseAuditableTenantEntityConfiguration<OrderPayment>
{
  public override void Configure(EntityTypeBuilder<OrderPayment> builder)
  {
    base.Configure(builder);

    builder
      .Property(a => a.Amount)
      .HasPrecision(7, 3);
  }
}