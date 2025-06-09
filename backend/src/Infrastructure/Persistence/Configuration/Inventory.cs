using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class InventoryItemConfig : BaseAuditableTenantEntityConfiguration<InventoryItem>
{
    public override void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        base.Configure(builder);

        builder.Property(i => i.Name)
            .HasMaxLength(256);
    }
}
