using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class CreditMemoByIdWithDetailsSpec : Specification<CreditMemo, CreditMemoDto>, ISingleResultSpecification
{
    public CreditMemoByIdWithDetailsSpec(Guid creditMemoId)
    {
        Query
            .Where(cm => cm.Id == creditMemoId)
            // .Include(cm => cm.Customer) // For CustomerName, to be fetched in handler
            .Include(cm => cm.Applications)
                .ThenInclude(app => app.CustomerInvoice); // For InvoiceNumber in applications
            // .Include(cm => cm.OriginalCustomerInvoice); // For OriginalCustomerInvoiceNumber, if direct nav prop exists
                                                       // Otherwise, OriginalCustomerInvoiceId is used to fetch separately.
    }
}
