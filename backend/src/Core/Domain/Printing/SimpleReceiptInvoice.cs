namespace FSH.WebApi.Domain.Printing;

public class SimpleReceiptInvoice : PrintableDocument
{
  public PrintableType Type => PrintableType.Receipt;
}