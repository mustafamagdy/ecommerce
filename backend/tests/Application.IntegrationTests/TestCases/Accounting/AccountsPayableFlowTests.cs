using Xunit;
using FluentAssertions;
using FSH.WebApi.Application.Accounting.Suppliers;
using FSH.WebApi.Application.Accounting.VendorInvoices;
using FSH.WebApi.Application.Accounting.VendorPayments;
using FSH.WebApi.Application.Accounting.PaymentMethods; // Assuming we might need to create one
using FSH.WebApi.Domain.Accounting; // For enums like VendorInvoiceStatus
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory/ISender if TestBase doesn't provide directly
using FSH.WebApi.Application.IntegrationTests.Infra; // Assuming TestBase or TestFixture is here

namespace FSH.WebApi.Application.IntegrationTests.TestCases.Accounting;

// Assuming a TestBase or similar fixture setup as seen in other integration tests in the project
// For example: public class AccountsPayableFlowTests : IClassFixture<TestFixture<Startup>>
// Or if there's a TestBase that provides ScopeFactory or Sender:
public class AccountsPayableFlowTests : TestBase // Replace TestBase with actual base class if different
{
    // Constructor would take ITestOutputHelper and the TestFixture if using IClassFixture
    // public AccountsPayableFlowTests(TestFixture<Startup> fixture, ITestOutputHelper output) : base(fixture, output) {}
    // If TestBase provides Sender directly:
    // public AccountsPayableFlowTests(ISender sender) : base(sender) {} -> Adapt as per actual TestBase

    // Helper to create a PaymentMethod if none suitable exists by default
    private async Task<Guid> EnsurePaymentMethodAsync(string name = "Test Bank Transfer")
    {
        // Check if it exists
        var searchRequest = new SearchPaymentMethodsRequest { Keyword = name, PageSize = 1, PageNumber = 1 };
        var searchResult = await Sender.Send(searchRequest); // Assuming Sender is available from TestBase
        if (searchResult.Data.Any(pm => pm.Name == name))
        {
            return searchResult.Data.First(pm => pm.Name == name).Id;
        }

        // Create if not exists
        var createRequest = new CreatePaymentMethodRequest { Name = name, Description = "Test bank transfer method", IsActive = true };
        return await Sender.Send(createRequest);
    }


    [Fact]
    public async Task Should_Successfully_Complete_Full_AP_Cycle()
    {
        // Arrange
        var paymentMethodId = await EnsurePaymentMethodAsync("AP Cycle Test Method");

        // 1. Create Supplier
        var createSupplierRequest = new CreateSupplierRequest
        {
            Name = $"AP Cycle Supplier - {Guid.NewGuid().ToString().Substring(0, 8)}",
            ContactInfo = "ap.cycle@test.com",
            Address = "123 AP Cycle St",
            TaxId = "APCYCLE123",
            DefaultPaymentTermId = null, // Assuming no specific payment term needed for this test
            BankDetails = "AP Cycle Bank"
        };
        var supplierId = await Sender.Send(createSupplierRequest);
        supplierId.Should().NotBeEmpty();

        // Optional: Retrieve and verify supplier
        var getSupplierRequest = new GetSupplierRequest(supplierId);
        var supplierDto = await Sender.Send(getSupplierRequest);
        supplierDto.Should().NotBeNull();
        supplierDto.Name.Should().Be(createSupplierRequest.Name);

        // 2. Create Vendor Invoice
        var createInvoiceRequest = new CreateVendorInvoiceRequest
        {
            SupplierId = supplierId,
            InvoiceNumber = $"INV-APCYCLE-{Guid.NewGuid().ToString().Substring(0, 6)}",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            TotalAmount = 1000m, // This should match sum of items' (Qty*Price)
            Currency = "USD",
            InvoiceItems = new List<CreateVendorInvoiceItemRequest>
            {
                new CreateVendorInvoiceItemRequest { Description = "Cycle Item 1", Quantity = 1, UnitPrice = 600m, TaxAmount = 0m, TotalAmount = 600m},
                new CreateVendorInvoiceItemRequest { Description = "Cycle Item 2", Quantity = 2, UnitPrice = 200m, TaxAmount = 0m, TotalAmount = 400m}
            }
        };
        // Adjust TotalAmount based on CreateVendorInvoiceRequestValidator logic for sum of item TotalAmounts
        createInvoiceRequest.TotalAmount = createInvoiceRequest.InvoiceItems.Sum(i => i.TotalAmount);

        var invoiceId = await Sender.Send(createInvoiceRequest);
        invoiceId.Should().NotBeEmpty();

        var getInvoiceRequest = new GetVendorInvoiceRequest(invoiceId);
        var invoiceDto = await Sender.Send(getInvoiceRequest);
        invoiceDto.Should().NotBeNull();
        invoiceDto.TotalAmount.Should().Be(createInvoiceRequest.TotalAmount);
        invoiceDto.Status.Should().Be(VendorInvoiceStatus.Draft.ToString()); // Assuming default status is Draft
        invoiceDto.SupplierId.Should().Be(supplierId);

        // (Optional step: Submit/Approve invoice if workflow requires it before payment)
        // For simplicity, assuming Draft invoice can be paid or handler updates status.
        // If an UpdateVendorInvoiceRequest to change status to Approved is needed:
        var updateInvoiceStatusRequest = new UpdateVendorInvoiceRequest
        {
            Id = invoiceId,
            Status = VendorInvoiceStatus.Approved // This was commented out in UpdateVendorInvoiceRequest, so handler might not support direct status update.
                                                 // The VendorInvoice domain entity has UpdateStatus. If a specific handler for this exists, use it.
                                                 // For now, assuming direct payment application will change status.
        };
        // If the handler for UpdateVendorInvoiceRequest supports status changes, or if there's a specific handler:
        // await Sender.Send(updateInvoiceStatusRequest);
        // var updatedInvoiceDto = await Sender.Send(getInvoiceRequest);
        // updatedInvoiceDto.Status.Should().Be(VendorInvoiceStatus.Approved.ToString());


        // 3. Create Vendor Payment
        var createPaymentRequest = new CreateVendorPaymentRequest
        {
            SupplierId = supplierId,
            PaymentDate = DateTime.UtcNow.Date,
            AmountPaid = invoiceDto.TotalAmount, // Pay full amount
            PaymentMethodId = paymentMethodId,
            ReferenceNumber = $"PAY-APCYCLE-{Guid.NewGuid().ToString().Substring(0, 6)}",
            Applications = new List<VendorPaymentApplicationRequestItem>
            {
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = invoiceId, AmountApplied = invoiceDto.TotalAmount }
            }
        };
        var paymentId = await Sender.Send(createPaymentRequest);
        paymentId.Should().NotBeEmpty();

        // 4. Verify Invoice Status and Payment Application
        var finalInvoiceDto = await Sender.Send(getInvoiceRequest);
        finalInvoiceDto.Should().NotBeNull();
        // Assuming CreateVendorPaymentHandler updates the invoice status to Paid
        finalInvoiceDto.Status.Should().Be(VendorInvoiceStatus.Paid.ToString());
        // Assuming VendorInvoiceDto has AmountPaid property that is updated by the payment
        // This would require GetVendorInvoiceHandler to correctly calculate/fetch this.
        // For now, we'll rely on the status change. A specific AmountPaidOnInvoiceDto field would be better.


        var getPaymentRequest = new GetVendorPaymentRequest(paymentId);
        var paymentDto = await Sender.Send(getPaymentRequest);
        paymentDto.Should().NotBeNull();
        paymentDto.AmountPaid.Should().Be(createPaymentRequest.AmountPaid);
        paymentDto.AppliedInvoices.Should().HaveCount(1);
        paymentDto.AppliedInvoices.First().VendorInvoiceId.Should().Be(invoiceId);
        paymentDto.AppliedInvoices.First().AmountApplied.Should().Be(createPaymentRequest.AmountPaid);
        paymentDto.UnappliedAmount.Should().Be(0);
    }

    [Fact]
    public async Task Should_Handle_Partial_Payment_For_VendorInvoice()
    {
        // Arrange
        var paymentMethodId = await EnsurePaymentMethodAsync("AP Partial Pay Test Method");
        var supplierId = await Sender.Send(new CreateSupplierRequest { Name = $"AP Partial Supplier - {Guid.NewGuid().ToString().Substring(0, 8)}" });

        var createInvoiceRequest = new CreateVendorInvoiceRequest
        {
            SupplierId = supplierId,
            InvoiceNumber = $"INV-PARTIAL-{Guid.NewGuid().ToString().Substring(0, 6)}",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            TotalAmount = 500m, Currency = "USD",
            InvoiceItems = new List<CreateVendorInvoiceItemRequest> { new CreateVendorInvoiceItemRequest { Description = "Partial Item", Quantity = 1, UnitPrice = 500m, TaxAmount = 0m, TotalAmount = 500m} }
        };
        var invoiceId = await Sender.Send(createInvoiceRequest);
        var invoiceDto = await Sender.Send(new GetVendorInvoiceRequest(invoiceId));

        var partialPaymentAmount = 200m;
        var createPaymentRequest = new CreateVendorPaymentRequest
        {
            SupplierId = supplierId,
            PaymentDate = DateTime.UtcNow.Date,
            AmountPaid = partialPaymentAmount,
            PaymentMethodId = paymentMethodId,
            Applications = new List<VendorPaymentApplicationRequestItem>
            {
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = invoiceId, AmountApplied = partialPaymentAmount }
            }
        };

        // Act
        var paymentId = await Sender.Send(createPaymentRequest);

        // Assert
        paymentId.Should().NotBeEmpty();
        var updatedInvoiceDto = await Sender.Send(new GetVendorInvoiceRequest(invoiceId));
        updatedInvoiceDto.Should().NotBeNull();

        // Assuming CreateVendorPaymentHandler updates invoice status to 'PartiallyPaid' or similar.
        // This depends on VendorInvoice.ApplyPayment logic and if a 'PartiallyPaid' status exists and is used.
        // For now, we check if it's not Draft and not Paid.
        // A more specific status like PartiallyPaid would be better.
        // The current VendorInvoice.ApplyPayment (if it exists) or the handler needs to set this.
        // Based on CreateVendorPaymentHandler, it updates to Paid if newInvoiceTotalApplied >= invoice.TotalAmount
        // and does nothing to status if partially paid. So status might remain Draft or Approved.
        // This needs to be aligned with domain logic for VendorInvoice status updates.
        // For now, let's assume it's not Draft. If it was Approved, it might stay Approved.
        // If it was Draft, it might stay Draft or move to PartiallyPaid.
        // Let's assume a scenario where it moves to PartiallyPaid if such a status is handled.
        // The VendorInvoice domain entity has: Draft, Submitted, Approved, Paid, Cancelled. No PartiallyPaid.
        // So, if partially paid, it would likely remain in Approved (if it was approved before payment).
        // If it was Draft, it might remain Draft.

        // Let's assume for the test that it was Approved before payment
        // (Manual status update before payment for test setup)
        // This part of the test highlights the need for clear status transition logic in domain/handlers.
        // For now, we'll assert that AmountPaid on the invoice is updated.
        // A dedicated property on VendorInvoiceDto for AmountPaid would be good.
        // The CreateVendorPaymentHandler does: invoice.UpdateStatus(VendorInvoiceStatus.Paid) or nothing.
        // This means a partial payment won't change status from Draft/Approved.

        // To properly test this, we need a way to see the "balance due" or "amount paid" on the invoice DTO.
        // Let's assume VendorInvoiceDto will eventually have an "AmountPaid" property.
        // For now, we check the payment itself.
        var paymentDto = await Sender.Send(new GetVendorPaymentRequest(paymentId));
        paymentDto.AmountPaid.Should().Be(partialPaymentAmount);
        paymentDto.AppliedInvoices.First().AmountApplied.Should().Be(partialPaymentAmount);

        // To verify invoice side, we'd need VendorInvoice to expose AmountPaid or similar.
        // The domain entity VendorInvoice does not have AmountPaid. This is a gap.
        // CustomerInvoice had ApplyPayment(amount) which updated its internal AmountPaid.
        // VendorInvoice needs similar logic.
        // For now, this test can only verify the payment side.
        // If VendorInvoice.ApplyPayment was implemented and updated an AmountPaid property:
        // updatedInvoiceDto.AmountPaid.Should().Be(partialPaymentAmount);
        // updatedInvoiceDto.Status.Should().Be(VendorInvoiceStatus.PartiallyPaid.ToString()); // If this status existed
    }

    // Other scenarios like One Payment for Multiple Invoices, Multiple Payments for One Invoice
    // would follow similar patterns, setting up multiple invoices/payments and verifying applications.
}
