namespace FSH.WebApi.Application.Operation.Orders;

public interface IInvoiceBarcodeInfo
{
  string Seller { get; }
  string VatNo { get; }
  DateTime DateTime { get; }
  decimal Total { get; }
  decimal Tax { get; }
}