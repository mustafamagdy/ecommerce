using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class Supplier : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? ContactInfo { get; private set; }
    public string? Address { get; private set; }
    public string? TaxId { get; private set; }
    public Guid? DefaultPaymentTermId { get; private set; } // Link to PaymentTerm entity
    public string? BankDetails { get; private set; }

    // Navigation property for PaymentTerm (if needed, and if PaymentTerm is an entity)
    // public virtual PaymentTerm? DefaultPaymentTerm { get; private set; }

    public Supplier(string name, string? contactInfo, string? address, string? taxId, Guid? defaultPaymentTermId, string? bankDetails)
    {
        Name = name;
        ContactInfo = contactInfo;
        Address = address;
        TaxId = taxId;
        DefaultPaymentTermId = defaultPaymentTermId;
        BankDetails = bankDetails;
    }

    public Supplier Update(string? name, string? contactInfo, string? address, string? taxId, Guid? defaultPaymentTermId, string? bankDetails)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (contactInfo is not null && ContactInfo?.Equals(contactInfo) is not true) ContactInfo = contactInfo;
        if (address is not null && Address?.Equals(address) is not true) Address = address;
        if (taxId is not null && TaxId?.Equals(taxId) is not true) TaxId = taxId;
        if (defaultPaymentTermId.HasValue && DefaultPaymentTermId != defaultPaymentTermId.Value) DefaultPaymentTermId = defaultPaymentTermId.Value;
        if (bankDetails is not null && BankDetails?.Equals(bankDetails) is not true) BankDetails = bankDetails;
        return this;
    }
}
