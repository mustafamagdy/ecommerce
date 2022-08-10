using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class BranchConfig : BaseAuditableTenantEntityConfiguration<Branch>
{
  public override void Configure(EntityTypeBuilder<Branch> builder)
  {
    base.Configure(builder);

    builder.HasMany(a => a.CashRegisters).WithOne(a => a.Branch).HasForeignKey(a => a.BranchId);
    var cashRegisterNavigation = builder.Navigation(nameof(Branch.CashRegisters));
    cashRegisterNavigation.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}