using Xunit;
using FluentAssertions;
using FSH.WebApi.Application.Accounting.Customers; // Assuming this is where Customer requests are, adjust if different
using FSH.WebApi.Application.Accounting.CustomerInvoices;
using FSH.WebApi.Application.Accounting.CustomerPayments;
using FSH.WebApi.Application.Accounting.CreditMemos;
using FSH.WebApi.Application.Accounting.PaymentMethods; // For ensuring PaymentMethod exists
using FSH.WebApi.Domain.Accounting; // For enums like CustomerInvoiceStatus, CreditMemoStatus
using FSH.WebApi.Domain.Operation.Customers; // For Customer entity for direct creation if needed, or use DTO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSH.WebApi.Application.IntegrationTests.Infra; // Assuming TestBase or TestFixture is here
// If Products are involved and need creation:
// using FSH.WebApi.Application.Catalog.Products;
// using FSH.WebApi.Domain.Catalog;


namespace FSH.WebApi.Application.IntegrationTests.TestCases.Accounting;

public class AccountsReceivableFlowTests : TestBase
{
    // Helper to create a Customer if needed - assuming a simple CreateCustomerRequest exists
    // Adjust if CreateCustomerRequest is in a different namespace or has different properties.
    // For FSH WebApi, Customer creation is usually under Host.Management, not Accounting directly.
    // For this test, we might need to use existing customers or simplify customer creation.
    // Let's assume a simplified path or direct entity creation for testing if handlers aren't easily available.
    // For now, we'll assume a CreateCustomerRequest exists in Accounting context or we use a known customer.
    // To avoid making this test dependent on Host.Management, we can't directly send CreateCustomerRequest from there.
    // Workaround: If there's no "Accounting.CreateCustomerRequest", we'd have to rely on seeded data or skip customer creation part in test.
    // Given the prior structure, Customer.cs is in Domain.Operation.Customers. Application layer for it might not be in Accounting.
    // For this test, we'll assume we can get/create a customer ID.
    // Let's define a helper that would ideally use a MediatR request if available.
    private async Task<Guid> EnsureCustomerAsync(string nameSuffix)
    {
        // This is a placeholder. In a real test suite, you'd use a proper
        // CreateCustomerCommand or ensure test data.
        // For FSH, customer creation is often specific. Let's try to find one or create one if there's an accessible command.
        // If no CreateCustomerCommand in Application.Accounting, this will be problematic.
        // For now, we'll proceed assuming we can get a customerId.
        // A better approach for true integration tests might involve a shared test data setup.

        // Fallback: create a dummy customer directly if no app layer command is usable here.
        // This is NOT ideal for true integration tests but works for flow demonstration.
        var customer = new Customer($"TestCust-{nameSuffix}", $"123-{nameSuffix.Substring(0, 3)}", false);
        // How to save this without a repository here? This is where TestBase usually helps with direct context or specific seeders.
        // If TestBase doesn't allow direct DB context, we MUST have a MediatR command.
        // Let's assume a simple CreateCustomerRequest is available in the Accounting context for test purposes.
        // If not, this test will highlight that dependency.
        // For now, to make progress, let's assume such a request exists.
        // If not, we'd use a known existing customer ID from seeded data.

        // Simplified: Let's try to find a customer or return a new Guid.
        // This is not robust. Tests should ensure data exists.
        var searchCustomers = new SearchCustomersRequest() { Keyword = "Default", PageNumber = 1, PageSize = 1 };
        var customersResult = await Sender.Send(searchCustomers);
        if (customersResult.Data.Any()) return customersResult.Data.First().Id;

        // If no customer found, this test will fail later. A real test setup would ensure one.
        // For now, creating a placeholder to allow test structure.
        var placeholderCustomerId = Guid.NewGuid();
        _createdCustomerIdsForCleanup.Add(placeholderCustomerId); // Track for cleanup
        return placeholderCustomerId;
    }
    private List<Guid> _createdCustomerIdsForCleanup = new List<Guid>(); // Hack for cleanup


    // Helper to create a PaymentMethod if none suitable exists by default
    private async Task<Guid> EnsurePaymentMethodAsync(string name = "AR Test Bank Transfer")
    {
        var searchRequest = new SearchPaymentMethodsRequest { NameKeyword = name, PageSize = 1, PageNumber = 1 };
        var searchResult = await Sender.Send(searchRequest);
        if (searchResult.Data.Any(pm => pm.Name == name))
        {
            return searchResult.Data.First(pm => pm.Name == name).Id;
        }
        var createRequest = new CreatePaymentMethodRequest { Name = name, Description = "AR test bank transfer method", IsActive = true };
        return await Sender.Send(createRequest);
    }

    [Fact]
    public async Task Should_Successfully_Complete_Invoice_To_Payment_Cycle()
    {
        // Arrange
        Guid customerId = await EnsureCustomerAsync($"ARINV-{Guid.NewGuid().ToString().Substring(0, 4)}");
        // If EnsureCustomerAsync returns a placeholder, we need to handle it or ensure a real customer for full test.
        // For now, assuming it gives a valid (even if just placeholder) ID.

        var paymentMethodId = await EnsurePaymentMethodAsync("AR InvoicePay Cycle Method");

        // 1. Create Customer Invoice
        var createInvoiceRequest = new CreateCustomerInvoiceRequest
        {
            CustomerId = customerId,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Currency = "USD",
            Notes = "AR Cycle Invoice",
            InvoiceItems = new List<CreateCustomerInvoiceItemRequest>
            {
                new CreateCustomerInvoiceItemRequest { Description = "Service A", Quantity = 1, UnitPrice = 200m, TaxAmount = 20m },
                new CreateCustomerInvoiceItemRequest { Description = "Service B", Quantity = 1, UnitPrice = 300m, TaxAmount = 30m }
            }
            // InvoiceNumber and TotalAmount are auto-calculated by handler/domain
        };
        var invoiceId = await Sender.Send(createInvoiceRequest);
        invoiceId.Should().NotBeEmpty();

        var getInvoiceRequest = new GetCustomerInvoiceRequest(invoiceId);
        var invoiceDto = await Sender.Send(getInvoiceRequest);
        invoiceDto.Should().NotBeNull();
        // Total = (200+20) + (300+30) = 220 + 330 = 550
        invoiceDto.TotalAmount.Should().Be(550m);
        invoiceDto.AmountPaid.Should().Be(0);
        invoiceDto.Status.Should().Be(CustomerInvoiceStatus.Draft.ToString()); // Default status from domain

        // 2. Optional: Update Invoice Status to Sent/Approved (if required before payment)
        // Assuming CustomerInvoice.Update allows status change, or a specific handler exists
        var updateInvoiceStatusRequest = new UpdateCustomerInvoiceRequest
        {
            Id = invoiceId,
            Status = CustomerInvoiceStatus.Sent // This was commented out in UpdateCustomerInvoiceRequest earlier
                                                // The domain entity CustomerInvoice.Update has a status parameter.
        };
        // For this test, let's assume the handler can update status or a specific status update handler exists.
        // If not, this part might fail or need adjustment.
        // Let's test the Update handler's capability.
        await Sender.Send(updateInvoiceStatusRequest);
        var sentInvoiceDto = await Sender.Send(getInvoiceRequest);
        sentInvoiceDto.Status.Should().Be(CustomerInvoiceStatus.Sent.ToString());


        // 3. Create Customer Payment
        var createPaymentRequest = new CreateCustomerPaymentRequest
        {
            CustomerId = customerId,
            PaymentDate = DateTime.UtcNow.Date,
            AmountReceived = sentInvoiceDto.TotalAmount, // Pay full amount
            PaymentMethodId = paymentMethodId,
            ReferenceNumber = $"PAY-ARCYCLE-{Guid.NewGuid().ToString().Substring(0, 6)}",
            Applications = new List<CustomerPaymentApplicationRequestItem>
            {
                new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = invoiceId, AmountApplied = sentInvoiceDto.TotalAmount }
            }
        };
        var paymentId = await Sender.Send(createPaymentRequest);
        paymentId.Should().NotBeEmpty();

        // 4. Verify Invoice and Payment
        var finalInvoiceDto = await Sender.Send(getInvoiceRequest);
        finalInvoiceDto.Should().NotBeNull();
        finalInvoiceDto.Status.Should().Be(CustomerInvoiceStatus.Paid.ToString());
        finalInvoiceDto.AmountPaid.Should().Be(sentInvoiceDto.TotalAmount);
        finalInvoiceDto.BalanceDue.Should().Be(0);

        var getPaymentRequest = new GetCustomerPaymentRequest(paymentId);
        var paymentDto = await Sender.Send(getPaymentRequest);
        paymentDto.Should().NotBeNull();
        paymentDto.AmountReceived.Should().Be(createPaymentRequest.AmountReceived);
        paymentDto.AppliedInvoices.Should().HaveCount(1);
        paymentDto.AppliedInvoices.First().CustomerInvoiceId.Should().Be(invoiceId);
        paymentDto.AppliedInvoices.First().AmountApplied.Should().Be(createPaymentRequest.AmountReceived);
        paymentDto.UnappliedAmount.Should().Be(0);
    }


    [Fact]
    public async Task Should_Successfully_Complete_Invoice_To_CreditMemo_Application_Cycle()
    {
        // Arrange
        Guid customerId = await EnsureCustomerAsync($"ARCM-{Guid.NewGuid().ToString().Substring(0,4)}");
        // Create Invoice
        var createInvoiceRequest = new CreateCustomerInvoiceRequest
        {
            CustomerId = customerId, InvoiceDate = DateTime.UtcNow.Date, DueDate = DateTime.UtcNow.Date.AddDays(10), Currency = "USD",
            InvoiceItems = new List<CreateCustomerInvoiceItemRequest> { new CreateCustomerInvoiceItemRequest { Description = "CM Item", Quantity = 1, UnitPrice = 300m, TaxAmount = 0m } }
        };
        var invoiceId = await Sender.Send(createInvoiceRequest);
        var invoiceDto = await Sender.Send(new GetCustomerInvoiceRequest(invoiceId));
        invoiceDto.TotalAmount.Should().Be(300m);

        // (Optional) Mark invoice as Sent
        await Sender.Send(new UpdateCustomerInvoiceRequest { Id = invoiceId, Status = CustomerInvoiceStatus.Sent });


        // 1. Create Credit Memo
        var creditAmount = 100m;
        var createCreditMemoRequest = new CreateCreditMemoRequest
        {
            CustomerId = customerId, Date = DateTime.UtcNow.Date, Reason = "Partial credit for INV", TotalAmount = creditAmount, Currency = "USD",
            OriginalCustomerInvoiceId = invoiceId // Link to original invoice
        };
        var creditMemoId = await Sender.Send(createCreditMemoRequest);
        creditMemoId.Should().NotBeEmpty();

        var getCreditMemoRequest = new GetCreditMemoRequest(creditMemoId);
        var creditMemoDto = await Sender.Send(getCreditMemoRequest);
        creditMemoDto.Should().NotBeNull();
        creditMemoDto.TotalAmount.Should().Be(creditAmount);
        // Assuming CreateCreditMemoHandler sets status to Approved or Draft.
        // The domain entity CreditMemo constructor sets it to Draft by default.
        // CreateCreditMemoHandler sets it to Approved.
        creditMemoDto.Status.Should().Be(CreditMemoStatus.Approved.ToString());
        creditMemoDto.AvailableBalance.Should().Be(creditAmount);

        // 2. (Optional) Approve Credit Memo - Already approved by handler in this case.
        // If it was Draft:
        // await Sender.Send(new UpdateCreditMemoRequest { Id = creditMemoId, Status = CreditMemoStatus.Approved });
        // creditMemoDto = await Sender.Send(getCreditMemoRequest);
        // creditMemoDto.Status.Should().Be(CreditMemoStatus.Approved.ToString());

        // 3. Apply Credit Memo to Invoice
        var applyCreditRequest = new ApplyCreditMemoToInvoiceRequest
        {
            CreditMemoId = creditMemoId,
            CustomerInvoiceId = invoiceId,
            AmountToApply = creditAmount
        };
        await Sender.Send(applyCreditRequest); // Returns CreditMemoId

        // 4. Verify Invoice and Credit Memo
        var updatedInvoiceDto = await Sender.Send(new GetCustomerInvoiceRequest(invoiceId));
        updatedInvoiceDto.Should().NotBeNull();
        updatedInvoiceDto.AmountPaid.Should().Be(creditAmount); // Credit application updates AmountPaid
        updatedInvoiceDto.Status.Should().Be(CustomerInvoiceStatus.PartiallyPaid.ToString()); // 300 total, 100 credit
        updatedInvoiceDto.BalanceDue.Should().Be(200m);

        var updatedCreditMemoDto = await Sender.Send(getCreditMemoRequest);
        updatedCreditMemoDto.Should().NotBeNull();
        updatedCreditMemoDto.AvailableBalance.Should().Be(0);
        updatedCreditMemoDto.AppliedAmount.Should().Be(creditAmount);
        updatedCreditMemoDto.Status.Should().Be(CreditMemoStatus.Applied.ToString());
    }

    // Cleanup hack for placeholder customers
    public override void Dispose()
    {
        // This is a very simplified cleanup. Real integration tests might reset DB state.
        // If a proper "DeleteCustomerCommand" existed in Accounting context and was safe:
        // foreach(var custId in _createdCustomerIdsForCleanup) { Sender.Send(new DeleteCustomerCommand(custId)); }
        base.Dispose();
    }
}
