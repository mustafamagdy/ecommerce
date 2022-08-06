using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class CashRegisterConfig : IEntityTypeConfiguration<CashRegister>
{
  public virtual void Configure(EntityTypeBuilder<CashRegister> builder)
  {
    builder.IsMultiTenant();

    builder.HasOne(a => a.Branch).WithMany(a => a.CashRegisters).HasForeignKey(a => a.BranchId);

    var activeOperations = builder.Navigation(nameof(CashRegister.ActiveOperations));
    activeOperations.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
    // builder.HasMany(a => a.ActiveOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    builder.HasMany(a => a.ArchivedOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);

    builder
      .Property(b => b.Balance)
      .HasPrecision(7, 3);
  }
}

public abstract class BasePaymentOperationConfig<T> : IEntityTypeConfiguration<T>
  where T : PaymentOperation
{
  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    builder.IsMultiTenant();
    builder
      .Property(b => b.Amount)
      .HasPrecision(7, 3);

    builder
      .Property(b => b.Type)
      .HasConversion(
        p => p.Value,
        p => PaymentOperationType.FromValue(p)
      );
  }
}

public class ActivePaymentOperationConfig : BasePaymentOperationConfig<ActivePaymentOperation>
{
  public override void Configure(EntityTypeBuilder<ActivePaymentOperation> builder)
  {
    base.Configure(builder);
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ActiveOperations).HasForeignKey(a => a.CashRegisterId);
  }
}

public class ArchivedPaymentOperationConfig : BasePaymentOperationConfig<ArchivedPaymentOperation>
{
  public override void Configure(EntityTypeBuilder<ArchivedPaymentOperation> builder)
  {
    base.Configure(builder);
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ArchivedOperations).HasForeignKey(a => a.CashRegisterId);
  }
}