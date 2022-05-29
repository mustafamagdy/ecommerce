using FSH.WebApi.Application.Common.Exporters;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Infrastructure.Common.Export.Pdf;

public class PdfWriter : IPdfWriter
{
  public Stream WriteToStream<T>(in T document)
    where T : IDocument
  {
    using var memStream = new MemoryStream();
    var pdf = document.GeneratePdf();
    memStream.Read(pdf);
    return memStream;
  }
}