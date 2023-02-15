namespace FSH.WebApi.Application.Operation.Payments;

public class PaymentMethodDto:IDto
{
    public Guid Id { get; }
    public string Name { get; }

    public PaymentMethodDto(Guid Id, string name)
    {
        this.Id = Id;
        Name = name;
    }
}