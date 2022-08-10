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

    builder.HasOne(a => a.Branch).WithMany(a => a.CashRegisters).HasForeignKey(a => a.BranchId);

    builder.HasMany(a => a.ActiveOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    var activeOperations = builder.Navigation(nameof(CashRegister.ActiveOperations));
    activeOperations.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(a => a.ArchivedOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    var archivedOperations = builder.Navigation(nameof(CashRegister.ArchivedOperations));
    archivedOperations.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .Property(b => b.Balance)
      .HasPrecision(7, 3);
  }
}

public abstract class BaseAuditablePaymentOperationConfig<T> : BaseAuditableTenantEntityConfiguration<T>
  where T : PaymentOperation
{
  public override void Configure(EntityTypeBuilder<T> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Amount)
      .HasPrecision(7, 3);

    builder
      .Property(b => b.Type)
      .HasConversion(
        p => p.Value,
        p => PaymentOperationType.FromValue(p));
  }
}

public class ActiveAuditablePaymentOperationConfig : BaseAuditablePaymentOperationConfig<ActivePaymentOperation>
{
  public override void Configure(EntityTypeBuilder<ActivePaymentOperation> builder)
  {
    base.Configure(builder);
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ActiveOperations).HasForeignKey(a => a.CashRegisterId);
  }
}

public class ArchivedAuditablePaymentOperationConfig : BaseAuditablePaymentOperationConfig<ArchivedPaymentOperation>
{
  public override void Configure(EntityTypeBuilder<ArchivedPaymentOperation> builder)
  {
    base.Configure(builder);
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ArchivedOperations).HasForeignKey(a => a.CashRegisterId);
  }
}