using FSH.WebApi.Application.Common.Exporters;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Infrastructure.Common.Export;

public sealed class PdfWriter : IPdfWriter
{
  public Stream WriteToStream<T>(in T document)
    where T : IDocument
  {
    byte[] pdfBytes = document.GeneratePdf();
    Stream stream = new MemoryStream(pdfBytes);
    stream.Seek(0, SeekOrigin.Begin);
    return stream;
  }
}