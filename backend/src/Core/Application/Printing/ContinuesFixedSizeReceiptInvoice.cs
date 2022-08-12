// using System.Reflection;
// using FSH.WebApi.Application.Operation.Orders;
// using FSH.WebApi.Domain.Printing;
//
// namespace FSH.WebApi.Application.Printing;
//
// public class ContinuesFixedSizeReceiptInvoice
// {
//   private readonly OrderExportDto _model;
//   private readonly IVatQrCodeGenerator _qrGenerator;
//   private readonly List<DocumentSection> _sections = new();
//   private byte[] _qrCode = null!;
//   private byte[] _logo = null;
//
//   private ContinuesFixedSizeReceiptInvoice()
//   {
//   }
//
//   public ContinuesFixedSizeReceiptInvoice(OrderExportDto model, IVatQrCodeGenerator qrGenerator)
//   {
//     _model = model;
//     _qrGenerator = qrGenerator;
//
//     _qrCode = _qrGenerator.GenerateQrCode(_model.Base64QrCode, 100, 100);
//
//     string? appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//     _logo = File.ReadAllBytes(appPath + "/Files/logos/tenant_logo.png");
//
//     _sections.Add(new LogoSection(1, SectionAlignment.Center, SectionPosition.Header, _logo));
//     _sections.Add(new TextSection(2, SectionAlignment.Center, SectionPosition.Header, "فاتورة ضريبية مبسطة"));
//     _sections.Add(new TwoItemRowSection(3, SectionPosition.Header, "VAT1234567", "رقم ضريبي ١٢٣٤٥٦٧"));
//   }
//
//   public IReadOnlyCollection<DocumentSection> Sections => _sections.AsReadOnly();
//   public int Width => 3;
// }