using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FSH.WebApi.Infrastructure.Seeders;

public sealed class PrintableDocumentsSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<PrintableDocumentsSeeder> _logger;

  public PrintableDocumentsSeeder(ISerializerService serializerService, ILogger<PrintableDocumentsSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.02";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    _logger.LogInformation("Started to Seed Printables");

    string jsonData = await File.ReadAllTextAsync(path + "/Seeders/printables.json", cancellationToken);
    // var items = JsonConvert.DeserializeObject<List<PrintableDocument>>(jsonData, new PrintableDocumentJsonConverter());
    var items = JsonConvert.DeserializeObject<List<PrintableDocument>>(jsonData);

    var simpleReceipts = items.OfType<SimpleReceiptInvoice>().ToArray();
    var wideInvoices = items.OfType<WideReceiptInvoice>().ToArray();
    var ordersSummaryReport = items.OfType<OrdersSummaryReport>().ToArray();
    var pnlReports = items.OfType<ProfitAndLossReport>().ToArray();
    var balanceReports = items.OfType<BalanceSheetReport>().ToArray();

    var hasSimpleSeeded = await _db.Set<SimpleReceiptInvoice>().AnyAsync(cancellationToken).ConfigureAwait(false);
    if (!hasSimpleSeeded && simpleReceipts.Length > 0)
    {
      await SeedTemplates(simpleReceipts, cancellationToken);
    }

    var hasWideSeeded = await _db.Set<WideReceiptInvoice>().AnyAsync(cancellationToken).ConfigureAwait(false);
    if (!hasWideSeeded && wideInvoices.Length > 0)
    {
      await SeedTemplates(wideInvoices, cancellationToken);
    }

    var hasOrderSummaryReport = await _db.Set<OrdersSummaryReport>().AnyAsync(cancellationToken).ConfigureAwait(false);
    if (!hasOrderSummaryReport && ordersSummaryReport.Length > 0)
    {
      await SeedTemplates(ordersSummaryReport, cancellationToken);
    }

    var hasPnlReport = await _db.Set<ProfitAndLossReport>().AnyAsync(cancellationToken).ConfigureAwait(false);
    if (!hasPnlReport && pnlReports.Length > 0)
    {
      await SeedTemplates(pnlReports, cancellationToken);
    }

    var hasBalanceReport = await _db.Set<BalanceSheetReport>().AnyAsync(cancellationToken).ConfigureAwait(false);
    if (!hasBalanceReport && balanceReports.Length > 0)
    {
      await SeedTemplates(balanceReports, cancellationToken);
    }

    await _db.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Seeded Subscription");
  }

  private async Task SeedTemplates(WideReceiptInvoice[] templates, CancellationToken cancellationToken)
  {
    templates = templates.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(templates, cancellationToken);
  }

  private async Task SeedTemplates(SimpleReceiptInvoice[] templates, CancellationToken cancellationToken)
  {
    templates = templates.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(templates, cancellationToken);
  }

  private async Task SeedTemplates(OrdersSummaryReport[] tempates, CancellationToken cancellationToken)
  {
    tempates = tempates.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(tempates, cancellationToken);
  }

  private async Task SeedTemplates(ProfitAndLossReport[] templates, CancellationToken cancellationToken)
  {
    templates = templates.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(templates, cancellationToken);
  }

  private async Task SeedTemplates(BalanceSheetReport[] templates, CancellationToken cancellationToken)
  {
    templates = templates.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(templates, cancellationToken);
  }
}
