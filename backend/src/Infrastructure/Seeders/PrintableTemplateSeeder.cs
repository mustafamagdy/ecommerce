using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FSH.WebApi.Infrastructure.Seeders;

public class PrintableDocumentsSeeder : ICustomSeeder
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

    var hasSimpleSeeded = await _db.SimpleReceiptInvoiceTemplates.AnyAsync(cancellationToken: cancellationToken);
    if (!hasSimpleSeeded && simpleReceipts.Length > 0)
    {
      await SeedSimpleReceipts(simpleReceipts, cancellationToken);
    }

    var hasWideSeeded = await _db.SimpleReceiptInvoiceTemplates.AnyAsync(cancellationToken: cancellationToken);
    if (!hasWideSeeded && wideInvoices.Length > 0)
    {
      await SeedWideInvoices(wideInvoices, cancellationToken);
    }

    await _db.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Seeded Subscription");
  }

  private async Task SeedWideInvoices(WideReceiptInvoice[] wideInvoices, CancellationToken cancellationToken)
  {
    wideInvoices = wideInvoices.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(wideInvoices, cancellationToken);
  }

  private async Task SeedSimpleReceipts(SimpleReceiptInvoice[] simpleReceipts, CancellationToken cancellationToken)
  {
    simpleReceipts = simpleReceipts.Select(a =>
    {
      a.Id = Guid.NewGuid();
      return a;
    }).ToArray();
    await _db.AddRangeAsync(simpleReceipts, cancellationToken);
  }
}