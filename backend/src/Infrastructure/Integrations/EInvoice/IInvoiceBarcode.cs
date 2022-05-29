using System.Globalization;
using System.Text;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace FSH.WebApi.Infrastructure.Integrations.EInvoice;

public interface IInvoiceBarcode
{
  string Seller { get; }
  string VatNo { get; }
  DateTime DateTime { get; }
  decimal Total { get; }
  decimal Tax { get; }
}

public interface IInvoiceBarcodeGenerator
{
  Stream GenerateQrCode(IInvoiceBarcode barcodeInfo, int width, int height);
}

public class InvoiceBarcodeGenerator : IInvoiceBarcodeGenerator
{
  private readonly byte[] _seller;
  private readonly byte[] _vatNo;
  private readonly byte[] _dateTime;
  private readonly byte[] _total;
  private readonly byte[] _tax;

  public InvoiceBarcodeGenerator(IInvoiceBarcode info)
  {
    _seller = Encoding.UTF8.GetBytes(info.Seller);
    _vatNo = Encoding.UTF8.GetBytes(info.VatNo);
    _dateTime = Encoding.UTF8.GetBytes(info.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    _total = Encoding.UTF8.GetBytes(info.Total.ToString(CultureInfo.InvariantCulture));
    _tax = Encoding.UTF8.GetBytes(info.Tax.ToString(CultureInfo.InvariantCulture));
  }

  private string GetAsText(int tag, byte[] value) => tag.ToString("X2")
                                                     + value.Length.ToString("X2")
                                                     + BitConverter.ToString(value).Replace("-", string.Empty);

  private byte[] GetBytes(int id, byte[] value)
  {
    byte[] bytes = new byte[2 + value.Length];
    bytes[0] = (byte)id;
    bytes[1] = (byte)value.Length;
    value.CopyTo((Array)bytes, 2);
    return bytes;
  }

  private string getString() => GetAsText(1, _seller)
                                + GetAsText(2, _vatNo)
                                + GetAsText(3, _dateTime)
                                + GetAsText(4, _total)
                                + GetAsText(5, _tax);

  public override string ToString() => getString();

  private string ToBase64()
  {
    var byteList = new List<byte>();
    byteList.AddRange(GetBytes(1, _seller));
    byteList.AddRange(GetBytes(2, _vatNo));
    byteList.AddRange(GetBytes(3, _dateTime));
    byteList.AddRange(GetBytes(4, _total));
    byteList.AddRange(GetBytes(5, _tax));
    return Convert.ToBase64String(byteList.ToArray());
  }

  public Stream GenerateQrCode(IInvoiceBarcode barcodeInfo, int width, int height)
  {
    string base64 = ToBase64();
    var qrCode = new QrCode(base64, new Vector2Slim(width, height), SKEncodedImageFormat.Png);
    var stream = new MemoryStream();
    qrCode.GenerateImage(stream);
    return stream;
  }

  public void SaveQrCode(QrCode qrcodeImage, string fullFilePath)
  {
    using var output = new FileStream(fullFilePath, FileMode.OpenOrCreate);
    qrcodeImage.GenerateImage(output);
  }
}

public class KsaInvoiceBarcodeInfo : IInvoiceBarcode
{
  public KsaInvoiceBarcodeInfo(string seller, string vatNo, DateTime dateTime, decimal total, decimal tax)
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