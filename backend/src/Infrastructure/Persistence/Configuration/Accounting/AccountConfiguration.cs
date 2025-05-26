using FSH.WebApi.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration.Accounting;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", "Accounting"); // Specify schema "Accounting"

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AccountType)
            .IsRequired();

        builder.Property(a => a.Balance)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.IsActive)
            .IsRequired();

        // AuditableEntity properties (Id, CreatedAt, UpdatedAt, DeletedAt, DeletedBy, CreatedBy, UpdatedBy)
        // are typically configured in a base configuration or by convention.
        // Assuming Id is already configured as PK by AuditableEntity or base class.
        // If not, ensure Id is configured: builder.HasKey(a => a.Id);

        // Relationships:
        // An Account can have many Transactions. This is the inverse of Transaction.Account.
        // This is not explicitly defined here but through TransactionConfiguration.
    }
}
