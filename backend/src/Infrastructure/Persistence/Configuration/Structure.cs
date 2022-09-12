using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Structure;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class BranchConfig : BaseAuditableEntityConfiguration<Branch>
{
  public override void Configure(EntityTypeBuilder<Branch> builder)
  {
    base.Configure(builder);

    builder.Property(a => a.Active).HasDefaultValue(true);

    // builder.HasMany(a => a.CashRegisters).WithOne(a => a.Branch).HasForeignKey(a => a.BranchId);
    var cashRegisterNavigation = builder.Navigation(nameof(Branch.CashRegisters));
    cashRegisterNavigation.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}