using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // Assuming Customer entity path
using FSH.WebApi.Domain.Operation.Orders;   // Assuming Order entity path
using FSH.WebApi.Domain.Catalog;           // Assuming Product entity path
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class CreateCustomerInvoiceHandler : IRequestHandler<CreateCustomerInvoiceRequest, Guid>
{
    private readonly IRepository<CustomerInvoice> _invoiceRepository;
    private readonly IReadRepository<Customer> _customerRepository; // Using IReadRepository
    private readonly IReadRepository<Order>? _orderRepository;      // Optional
    private readonly IReadRepository<Product>? _productRepository;  // Optional
    private readonly IStringLocalizer<CreateCustomerInvoiceHandler> _localizer;
    private readonly ILogger<CreateCustomerInvoiceHandler> _logger;

    public CreateCustomerInvoiceHandler(
        IRepository<CustomerInvoice> invoiceRepository,
        IReadRepository<Customer> customerRepository,
        IStringLocalizer<CreateCustomerInvoiceHandler> localizer,
        ILogger<CreateCustomerInvoiceHandler> logger,
        IReadRepository<Order>? orderRepository = null,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _localizer = localizer;
        _logger = logger;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateCustomerInvoiceRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate Customer
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
            throw new NotFoundException(_localizer["Customer with ID {0} not found.", request.CustomerId]);

        // 2. Validate Order (if OrderId is provided)
        if (request.OrderId.HasValue && _orderRepository != null)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId.Value, cancellationToken);
            if (order == null)
                throw new NotFoundException(_localizer["Order with ID {0} not found.", request.OrderId.Value]);
            if (order.CustomerId != request.CustomerId)
                throw new ValidationException(_localizer["Order {0} does not belong to Customer {1}.", order.OrderNumber, customer.Name]);
        }

        // 3. Generate InvoiceNumber (example: INV-YYYYMMDD-XXXX)
        // This needs a more robust generation strategy in a production system (e.g. database sequence or dedicated service)
        string invoiceNumber = await GenerateNextInvoiceNumberAsync(cancellationToken);


        var invoice = new CustomerInvoice(
            customerId: request.CustomerId,
            invoiceNumber: invoiceNumber,
            invoiceDate: request.InvoiceDate,
            dueDate: request.DueDate,
            currency: request.Currency,
            notes: request.Notes,
            orderId: request.OrderId,
            status: CustomerInvoiceStatus.Draft // Initial status
        );

        // 4. Process and add CustomerInvoiceItems
        foreach (var itemRequest in request.InvoiceItems)
        {
            if (itemRequest.ProductId.HasValue && _productRepository != null)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId.Value, cancellationToken);
                if (product == null)
                    throw new NotFoundException(_localizer["Product with ID {0} not found.", itemRequest.ProductId.Value]);
            }
            invoice.AddInvoiceItem(
                description: itemRequest.Description,
                quantity: itemRequest.Quantity,
                unitPrice: itemRequest.UnitPrice,
                taxAmount: itemRequest.TaxAmount,
                productId: itemRequest.ProductId
            );
        }
        // The AddInvoiceItem method calls RecalculateTotalAmount internally.

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        _logger.LogInformation(_localizer["Customer Invoice {0} created for Customer {1}."], invoice.InvoiceNumber, customer.Name);
        return invoice.Id;
    }

    private async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        // Basic example: INV-YYYYMMDD-CountForDay
        // Warning: This is NOT concurrency-safe and might lead to duplicates under load.
        // A proper implementation would use a database sequence, a distributed lock, or a dedicated number sequence service.
        var today = DateTime.UtcNow;
        string prefix = $"INV-{today:yyyyMMdd}-";

        // Find the last invoice number for today to determine the next sequence
        var lastInvoiceTodaySpec = new CustomerInvoiceByNumberPrefixSpec(prefix);
        var lastInvoice = (await _invoiceRepository.ListAsync(lastInvoiceTodaySpec, cancellationToken))
                            .OrderByDescending(inv => inv.InvoiceNumber)
                            .FirstOrDefault();

        int nextSequence = 1;
        if (lastInvoice != null)
        {
            string lastSeqStr = lastInvoice.InvoiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastSeqStr, out int lastSeq))
            {
                nextSequence = lastSeq + 1;
            }
        }
        return $"{prefix}{nextSequence:D4}"; // Formats to 4 digits, e.g., 0001
    }
}

// Specification to find invoices by prefix for number generation (example)
public class CustomerInvoiceByNumberPrefixSpec : Specification<CustomerInvoice>
{
    public CustomerInvoiceByNumberPrefixSpec(string prefix)
    {
        Query.Where(ci => ci.InvoiceNumber.StartsWith(prefix));
    }
}
