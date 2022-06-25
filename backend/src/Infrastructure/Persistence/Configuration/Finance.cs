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

public class ActivePaymentOperationConfig : IEntityTypeConfiguration<ActivePaymentOperation>
{
  public void Configure(EntityTypeBuilder<ActivePaymentOperation> builder)
  {
    builder.IsMultiTenant();

    builder.HasOne(a => a.CashRegister).WithMany(a => a.ActiveOperations).HasForeignKey(a => a.CashRegisterId);
  }
}

public class ArchivedPaymentOperationConfig : IEntityTypeConfiguration<ArchivedPaymentOperation>
{
  public void Configure(EntityTypeBuilder<ArchivedPaymentOperation> builder)
  {
    builder.IsMultiTenant();

    builder.HasOne(a => a.CashRegister).WithMany(a => a.ArchivedOperations).HasForeignKey(a => a.CashRegisterId);
  }
}