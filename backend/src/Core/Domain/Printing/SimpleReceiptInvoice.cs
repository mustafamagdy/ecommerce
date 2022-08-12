namespace FSH.WebApi.Domain.Printing;

public class SimpleReceiptInvoice : PrintableDocument
{
  public SimpleReceiptInvoice()
    : base(PrintableType.Receipt)
  {
  }
}

public class WideReceiptInvoice : PrintableDocument
{
  public WideReceiptInvoice()
    : base(PrintableType.Wide)
  {
  }
}