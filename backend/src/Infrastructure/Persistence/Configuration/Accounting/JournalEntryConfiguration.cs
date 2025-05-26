using FSH.WebApi.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration.Accounting;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries", "Accounting");

        builder.HasKey(je => je.Id);

        builder.Property(je => je.EntryDate)
            .IsRequired();

        builder.Property(je => je.Description)
            .HasMaxLength(500);

        builder.Property(je => je.Status)
            .IsRequired();

        // Relationships:
        // A JournalEntry has many Transactions.
        builder.HasMany(je => je.Transactions)
            .WithOne(t => t.JournalEntry) // Navigation property in Transaction pointing back to JournalEntry
            .HasForeignKey(t => t.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade); // If a JournalEntry is deleted, its Transactions are also deleted.
    }
}
