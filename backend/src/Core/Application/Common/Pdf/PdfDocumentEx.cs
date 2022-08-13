using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Common.Pdf;

public static class PdfDocumentEx
{
  public static IContainer SeparatorLine(this IContainer element, char c = 'x', int charCount = 80)
  {
    string lineStr = new(c, charCount);
    element.AlignCenter().AlignTop().Text(lineStr).FontSize(5);
    return element;
  }
}