using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class PaymentTerm : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public int DaysUntilDue { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true; // Default to true

    public PaymentTerm(string name, int daysUntilDue, string? description, bool isActive = true)
    {
        Name = name;
        DaysUntilDue = daysUntilDue;
        Description = description;
        IsActive = isActive;
    }

    public PaymentTerm Update(string? name, int? daysUntilDue, string? description, bool? isActive)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (daysUntilDue.HasValue && DaysUntilDue != daysUntilDue.Value) DaysUntilDue = daysUntilDue.Value;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (isActive.HasValue && IsActive != isActive.Value) IsActive = isActive.Value;
        return this;
    }
}
