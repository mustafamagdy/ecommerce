namespace FSH.WebApi.Application.Operation.CashRegisters;

public class BasicCashRegisterDto : IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public bool Opened { get; set; }
  public string Color { get; set; }
}

public class CashRegisterWithBalanceDto : BasicCashRegisterDto
{
  public decimal Balance { get; set; }
}