using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class CashRegisterConfig : IEntityTypeConfiguration<CashRegister>
{
  public void Configure(EntityTypeBuilder<CashRegister> builder)
  {
    builder.IsMultiTenant();

    builder.HasOne(a => a.Branch).WithMany(a => a.CashRegisters).HasForeignKey(a => a.BranchId);
    builder.HasMany(a => a.ActiveOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
    builder.HasMany(a => a.ArchivedOperations).WithOne(a => a.CashRegister).HasForeignKey(a => a.CashRegisterId);
  }
}

public abstract class BasePaymentOperationConfig<T> : IEntityTypeConfiguration<T>
  where T : PaymentOperation
{
  public void Configure(EntityTypeBuilder<T> builder)
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
  public void Configure(EntityTypeBuilder<ActivePaymentOperation> builder)
  {
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ActiveOperations).HasForeignKey(a => a.CashRegisterId);
  }
}

public class ArchivedPaymentOperationConfig : BasePaymentOperationConfig<ArchivedPaymentOperation>
{
  public void Configure(EntityTypeBuilder<ArchivedPaymentOperation> builder)
  {
    builder.HasOne(a => a.CashRegister).WithMany(a => a.ArchivedOperations).HasForeignKey(a => a.CashRegisterId);
  }
}