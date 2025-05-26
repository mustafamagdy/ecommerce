using FSH.WebApi.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration.Accounting;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", "Accounting");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .IsRequired();

        // Relationships:
        // Many Transactions belong to one Account.
        builder.HasOne(t => t.Account)
            .WithMany() // Assuming Account does not have a direct navigation collection of Transactions. If it does, specify it here.
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting an Account if it has Transactions. User must reassign or handle them.

        // Many Transactions belong to one JournalEntry.
        // This relationship is already configured in JournalEntryConfiguration (HasMany Transactions).
        // We just need to ensure the foreign key is set.
        builder.HasOne(t => t.JournalEntry)
            .WithMany(je => je.Transactions) // This matches the configuration in JournalEntryConfiguration
            .HasForeignKey(t => t.JournalEntryId)
            .IsRequired() // A Transaction must belong to a JournalEntry
            .OnDelete(DeleteBehavior.Cascade); // Overrides the default if different, but Cascade is good here.
                                            // This was already set in JournalEntryConfiguration.
    }
}
