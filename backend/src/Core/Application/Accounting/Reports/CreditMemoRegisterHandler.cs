using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.CreditMemos.Specifications;
using FSH.WebApi.Application.Accounting.CustomerInvoices.Specifications; // For CustomerInvoicesByIdsSpec
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Domain.Operation.Customers; // For Customer entity

namespace FSH.WebApi.Application.Accounting.Reports;

public class CreditMemoRegisterHandler : IRequestHandler<CreditMemoRegisterRequest, CreditMemoRegisterDto>
{
    private readonly IReadRepository<CreditMemo> _creditMemoRepo;
    private readonly IReadRepository<Customer> _customerRepo;
    private readonly IReadRepository<CustomerInvoice> _invoiceRepo; // To get OriginalInvoiceNumber
    // private readonly IStringLocalizer<CreditMemoRegisterHandler> _localizer;

    public CreditMemoRegisterHandler(
        IReadRepository<CreditMemo> creditMemoRepo,
        IReadRepository<Customer> customerRepo,
        IReadRepository<CustomerInvoice> invoiceRepo
        /* IStringLocalizer<CreditMemoRegisterHandler> localizer */)
    {
        _creditMemoRepo = creditMemoRepo;
        _customerRepo = customerRepo;
        _invoiceRepo = invoiceRepo;
        // _localizer = localizer;
    }

    public async Task<CreditMemoRegisterDto> Handle(CreditMemoRegisterRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new CreditMemoRegisterDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CustomerId = request.CustomerId,
            StatusFilter = request.StatusFilter.ToString(),
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            CreditMemos = new List<CreditMemoRegisterLineDto>()
        };

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"Customer with ID {request.CustomerId.Value} not found.");
            reportDto.CustomerName = customer.Name;
        }

        // 1. Fetch Credit Memos
        var creditMemosSpec = new CreditMemosForRegisterSpec(request.StartDate, request.EndDate, request.CustomerId, request.StatusFilter);
        var creditMemos = await _creditMemoRepo.ListAsync(creditMemosSpec, cancellationToken);

        if (!creditMemos.Any())
        {
            return reportDto; // Return empty report
        }

        // 2. Prepare Original Invoice Details (Optimization)
        var originalInvoiceIds = creditMemos
            .Where(cm => cm.OriginalCustomerInvoiceId.HasValue)
            .Select(cm => cm.OriginalCustomerInvoiceId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, CustomerInvoice> originalInvoicesMap = new Dictionary<Guid, CustomerInvoice>();
        if (originalInvoiceIds.Any())
        {
            var invoicesSpec = new CustomerInvoicesByIdsSpec(originalInvoiceIds); // Assumes this spec exists
            var relatedInvoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);
            originalInvoicesMap = relatedInvoices.ToDictionary(inv => inv.Id);
        }

        // 3. Populate Report Lines
        reportDto.GrandTotalCreditMemoAmount = 0m;
        reportDto.GrandTotalAppliedAmount = 0m;
        reportDto.GrandTotalAvailableBalance = 0m;

        foreach (var creditMemo in creditMemos)
        {
            decimal appliedAmount = creditMemo.GetAppliedAmount(); // Domain method sums applications
            decimal availableBalance = creditMemo.GetAvailableBalance(); // Domain method: TotalAmount - AppliedAmount

            // Apply status filter logic more precisely if needed (beyond what spec does)
            // The spec filters by exact domain status. The report filter might be more nuanced.
            bool includeInReport = true;
            if (request.StatusFilter != CreditMemoRegisterStatusFilter.All)
            {
                var domainFilterStatus = Enum.TryParse<CreditMemoStatus>(request.StatusFilter.ToString(), true, out var parsedStatus) ? (CreditMemoStatus?)parsedStatus : null;

                if (domainFilterStatus.HasValue)
                {
                    // For "Approved" filter, we might mean Approved OR PartiallyApplied if it has balance.
                    // Current spec filters by exact status. This logic can refine.
                    if (request.StatusFilter == CreditMemoRegisterStatusFilter.Approved)
                    {
                        if (!(creditMemo.Status == CreditMemoStatus.Approved ||
                              (creditMemo.Status == CreditMemoStatus.PartiallyApplied && availableBalance > 0.005m))) // Small tolerance
                        {
                            includeInReport = false;
                        }
                    }
                    // else, spec already filtered by exact status like Draft, Applied, Void, PartiallyApplied.
                }
                else { /* Invalid filter string, should not happen if using enum */ }
            }


            if (!includeInReport) continue;


            var line = new CreditMemoRegisterLineDto
            {
                CreditMemoId = creditMemo.Id,
                CreditMemoNumber = creditMemo.CreditMemoNumber,
                Date = creditMemo.Date,
                CustomerId = creditMemo.CustomerId,
                CustomerName = creditMemo.Customer?.Name ?? "N/A", // Customer should be included by spec
                TotalAmount = creditMemo.TotalAmount,
                AppliedAmount = appliedAmount,
                AvailableBalance = availableBalance,
                Status = creditMemo.Status.ToString(),
                Reason = creditMemo.Reason,
                Notes = creditMemo.Notes,
                OriginalCustomerInvoiceId = creditMemo.OriginalCustomerInvoiceId
            };

            if (creditMemo.OriginalCustomerInvoiceId.HasValue &&
                originalInvoicesMap.TryGetValue(creditMemo.OriginalCustomerInvoiceId.Value, out var originalInvoice))
            {
                line.OriginalInvoiceNumber = originalInvoice.InvoiceNumber;
            }

            reportDto.CreditMemos.Add(line);
            reportDto.GrandTotalCreditMemoAmount += creditMemo.TotalAmount;
            reportDto.GrandTotalAppliedAmount += appliedAmount;
            reportDto.GrandTotalAvailableBalance += availableBalance;
        }

        // Sorting is handled by the spec (Date DESC, CustomerName ASC)
        // If a different final sort is needed on the processed DTO lines:
        // reportDto.CreditMemos = reportDto.CreditMemos.OrderByDescending(cm => cm.Date).ThenBy(cm => cm.CustomerName).ToList();

        return reportDto;
    }
}
