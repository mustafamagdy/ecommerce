using SkiaSharp.QrCode.Image;

namespace FSH.WebApi.Application.Operation.Orders;

public interface IInvoiceBarcodeGenerator : ITransientService
{
  byte[] GenerateQrCode(IInvoiceBarcodeInfo info, int width, int height);
  void SaveQrCode(QrCode qrcodeImage, string fullFilePath);
  string ToBase64(IInvoiceBarcodeInfo info);
}