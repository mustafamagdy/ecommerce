using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class BranchConfig : IEntityTypeConfiguration<Branch>
{
  public void Configure(EntityTypeBuilder<Branch> builder)
  {
    builder.IsMultiTenant();

    builder.HasMany(a => a.CashRegisters).WithOne(a => a.Branch).HasForeignKey(a => a.BranchId);
  }
}