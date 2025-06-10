using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class ApplyCreditMemoToInvoiceRequest : IRequest<Guid> // Returns CreditMemoApplication.Id or CreditMemo.Id
{
    [Required]
    public Guid CreditMemoId { get; set; }

    [Required]
    public Guid CustomerInvoiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount to Apply must be greater than zero.")]
    public decimal AmountToApply { get; set; }
}
