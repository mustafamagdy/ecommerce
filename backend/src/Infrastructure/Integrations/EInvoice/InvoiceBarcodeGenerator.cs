using System.Globalization;
using System.Text;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Operation.Orders;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace FSH.WebApi.Infrastructure.Integrations.EInvoice;

public class InvoiceBarcodeGenerator : IInvoiceBarcodeGenerator
{
  public string ToBase64(IInvoiceBarcodeInfo info)
  {
    byte[] seller = Encoding.UTF8.GetBytes(info.Seller);
    byte[] vatNo = Encoding.UTF8.GetBytes(info.VatNo);
    byte[] dateTime = Encoding.UTF8.GetBytes(info.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    byte[] total = Encoding.UTF8.GetBytes(info.Total.ToString(CultureInfo.InvariantCulture));
    byte[] tax = Encoding.UTF8.GetBytes(info.Tax.ToString(CultureInfo.InvariantCulture));

    var byteList = new List<byte>();
    byteList.AddRange(GetBytes(1, seller));
    byteList.AddRange(GetBytes(2, vatNo));
    byteList.AddRange(GetBytes(3, dateTime));
    byteList.AddRange(GetBytes(4, total));
    byteList.AddRange(GetBytes(5, tax));
    return Convert.ToBase64String(byteList.ToArray());
  }

  public byte[] GenerateQrCode(IInvoiceBarcodeInfo info, int width, int height)
  {
    string base64 = ToBase64(info);
    var qrCode = new QrCode(base64, new Vector2Slim(width, height), SKEncodedImageFormat.Png);
    var stream = new MemoryStream();
    qrCode.GenerateImage(stream);
    return stream.ToArray();
  }

  public void SaveQrCode(QrCode qrcodeImage, string fullFilePath)
  {
    using var output = new FileStream(fullFilePath, FileMode.OpenOrCreate);
    qrcodeImage.GenerateImage(output);
  }

  private string GetAsText(int tag, byte[] value) => tag.ToString("X2")
                                                     + value.Length.ToString("X2")
                                                     + BitConverter.ToString(value).Replace("-", string.Empty);

  private byte[] GetBytes(int id, byte[] value)
  {
    byte[] bytes = new byte[2 + value.Length];
    bytes[0] = (byte)id;
    bytes[1] = (byte)value.Length;
    value.CopyTo(bytes, 2);
    return bytes;
  }
}