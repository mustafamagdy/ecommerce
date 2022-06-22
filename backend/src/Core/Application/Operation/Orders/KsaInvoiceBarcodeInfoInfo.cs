namespace FSH.WebApi.Application.Operation.Orders;

public class KsaInvoiceBarcodeInfoInfo : IInvoiceBarcodeInfo
{
  public KsaInvoiceBarcodeInfoInfo(string seller, string vatNo, DateTime dateTime, decimal total, decimal tax)
  {
    Seller = seller;
    VatNo = vatNo;
    DateTime = dateTime;
    Total = total;
    Tax = tax;
  }

  public string Seller { get; }
  public string VatNo { get; }
  public DateTime DateTime { get; }
  public decimal Total { get; }
  public decimal Tax { get; }
}