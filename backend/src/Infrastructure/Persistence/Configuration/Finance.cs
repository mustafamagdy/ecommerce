using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class CashRegisterConfig : BaseAuditableTenantEntityConfiguration<CashRegister>
{
  public override void Configure(EntityTypeBuilder<CashRegister> builder)
  {
    base.Configure(builder);

    // builder.HasMany(a => a.ActiveOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    var activeOperations = builder.Navigation(nameof(CashRegister.ActiveOperations));
    activeOperations.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    // builder.HasMany(a => a.ArchivedOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    var archivedOperations = builder.Navigation(nameof(CashRegister.ArchivedOperations));
    archivedOperations.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .Property(b => b.Balance)
      .HasPrecision(7, 3);
  }
}

public class PaymentOperationConfig : BaseAuditableTenantEntityConfiguration<PaymentOperation>
{
  public override void Configure(EntityTypeBuilder<PaymentOperation> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Amount)
      .HasPrecision(7, 3);

    builder
      .HasDiscriminator<string>("payment_type")
      .HasValue<ActivePaymentOperation>("active")
      .HasValue<ArchivedPaymentOperation>("archived");
  }
}