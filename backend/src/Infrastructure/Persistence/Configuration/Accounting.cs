using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

// Account Configuration
public class AccountConfig : BaseAuditableTenantEntityConfiguration<Account>
{
    public override void Configure(EntityTypeBuilder<Account> builder)
    {
        base.Configure(builder);

        builder.ToTable("Accounts", SchemaNames.Accounting);

        builder.Property(a => a.AccountName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.AccountNumber)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.AccountType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Balance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1024);

        builder.Property(a => a.IsActive)
            .IsRequired();
    }
}

// JournalEntry Configuration
public class JournalEntryConfig : BaseAuditableTenantEntityConfiguration<JournalEntry>
{
    public override void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("JournalEntries", SchemaNames.Accounting);

        builder.Property(je => je.EntryDate)
            .IsRequired();

        builder.Property(je => je.Description)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(je => je.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(je => je.IsPosted)
            .IsRequired();

        // One-to-many with Transaction
        builder.HasMany(je => je.Transactions)
            .WithOne(t => t.JournalEntry)
            .HasForeignKey(t => t.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade); // If a Journal Entry is deleted, its transactions are deleted.
    }
}

// Transaction Configuration
public class TransactionConfig : BaseAuditableTenantEntityConfiguration<Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("Transactions", SchemaNames.Accounting);

        builder.Property(t => t.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(512);

        // Many-to-one with Account
        builder.HasOne(t => t.Account)
            .WithMany() // An Account can have many Transactions, but not explicitly defined as a collection on Account entity for this side of relation.
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting an Account if it has Transactions.

        // Many-to-one with JournalEntry is configured by JournalEntryConfig's HasMany.
    }
}

// FinancialStatement Configuration
public class FinancialStatementConfig : BaseAuditableTenantEntityConfiguration<FinancialStatement>
{
    public override void Configure(EntityTypeBuilder<FinancialStatement> builder)
    {
        base.Configure(builder);

        builder.ToTable("FinancialStatements", SchemaNames.Accounting);

        builder.Property(fs => fs.StatementType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(fs => fs.PeriodStartDate)
            .IsRequired();

        builder.Property(fs => fs.PeriodEndDate)
            .IsRequired();

        builder.Property(fs => fs.GeneratedDate)
            .IsRequired();

        builder.Property(fs => fs.Content)
            .HasColumnType("nvarchar(max)"); // For JSON content, or use specific JSON type if DB supports e.g. "jsonb" for PostgreSQL
                                             // MaxLength(8000) might be too small for complex statements.
    }
}

// Budget Configuration
public class BudgetConfig : BaseAuditableTenantEntityConfiguration<Budget>
{
    public override void Configure(EntityTypeBuilder<Budget> builder)
    {
        base.Configure(builder);

        builder.ToTable("Budgets", SchemaNames.Accounting);

        builder.Property(b => b.BudgetName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(b => b.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.PeriodStartDate)
            .IsRequired();

        builder.Property(b => b.PeriodEndDate)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasMaxLength(1024);

        // Many-to-one with Account
        builder.HasOne(b => b.Account)
            .WithMany() // An Account can have many Budgets, not explicitly defined as a collection on Account entity for this side.
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade); // If an Account is deleted, its associated Budgets are also deleted. Consider Restrict if preferred.
    }
}

// SchemaNames helper class if not already present globally (it is present in the project structure)
// public static class SchemaNames
// {
//     public static string Accounting = nameof(Accounting).ToLowerInvariant();
//     // Add other schema names here if needed
// }
