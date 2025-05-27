using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Finbuckle.MultiTenant;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization.Seeders;

public class AccountingSeeder : ICustomSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantInfo _currentTenant;
    private readonly ILogger<AccountingSeeder> _logger;

    public string Order => "Accounting"; // Defines the order of execution if multiple seeders exist

    public AccountingSeeder(ApplicationDbContext db, ITenantInfo currentTenant, ILogger<AccountingSeeder> logger)
    {
        _db = db;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Chart of Accounts seeding for TenantId: {TenantId}", _currentTenant.Id);

        // Check if accounts already exist for this tenant
        if (await _db.Accounts.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Chart of Accounts already seeded for TenantId: {TenantId}. Skipping.", _currentTenant.Id);
            return;
        }

        var chartOfAccounts = new List<Account>
        {
            // Assets
            new Account("Cash", "1000", AccountType.Asset, 0, "Primary cash account", true),
            new Account("Accounts Receivable", "1010", AccountType.Asset, 0, "Money owed by customers", true),
            new Account("Inventory", "1020", AccountType.Asset, 0, "Goods available for sale", true),
            new Account("Prepaid Expenses", "1030", AccountType.Asset, 0, "Expenses paid in advance", true),
            new Account("Equipment", "1500", AccountType.Asset, 0, "Long-term equipment assets", true),

            // Liabilities
            new Account("Accounts Payable", "2000", AccountType.Liability, 0, "Money owed to suppliers", true),
            new Account("Salaries Payable", "2100", AccountType.Liability, 0, "Accrued salaries and wages", true),
            new Account("Unearned Revenue", "2200", AccountType.Liability, 0, "Revenue received but not yet earned", true),
            new Account("Loans Payable", "2300", AccountType.Liability, 0, "Borrowed funds", true),

            // Equity
            new Account("Common Stock", "3000", AccountType.Equity, 0, "Owner's investment in the company", true),
            new Account("Retained Earnings", "3100", AccountType.Equity, 0, "Accumulated profits/losses", true),

            // Revenue
            new Account("Sales Revenue", "4000", AccountType.Revenue, 0, "Revenue from sales of goods", true),
            new Account("Service Revenue", "4100", AccountType.Revenue, 0, "Revenue from services provided", true),

            // Expenses
            new Account("Cost of Goods Sold", "5000", AccountType.Expense, 0, "Direct costs of producing goods", true),
            new Account("Rent Expense", "5100", AccountType.Expense, 0, "Cost of occupying premises", true),
            new Account("Utilities Expense", "5200", AccountType.Expense, 0, "Cost of utilities (electricity, water, etc.)", true),
            new Account("Salaries Expense", "5300", AccountType.Expense, 0, "Cost of employee salaries", true),
            new Account("Marketing Expense", "5400", AccountType.Expense, 0, "Cost of marketing and advertising", true),
            new Account("Depreciation Expense", "5500", AccountType.Expense, 0, "Allocation of asset cost over time", true)
        };

        // Set TenantId for all accounts - this is handled by AuditableEntity and context now.
        // No need to manually set _currentTenant.Id as BaseDbContext should handle it.

        _db.Accounts.AddRange(chartOfAccounts);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chart of Accounts seeded successfully for TenantId: {TenantId}", _currentTenant.Id);
    }
}