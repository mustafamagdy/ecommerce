using SkiaSharp.QrCode.Image;

namespace FSH.WebApi.Application.Operation.Orders;

public interface IVatQrCodeGenerator : ITransientService
{
  byte[] GenerateQrCode(IInvoiceBarcodeInfo info, int width, int height);
  byte[] GenerateQrCode(string base64QrCode, int width, int height);
  void SaveQrCode(QrCode qrcodeImage, string fullFilePath);
  string ToBase64(IInvoiceBarcodeInfo info);
}