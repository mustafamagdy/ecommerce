using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Common.Pdf;

public abstract class BasePdfDocument : IDocument
{
  public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

  public void Compose(IDocumentContainer container)
  {
    container.Page(page =>
    {
      SetupPage(page);

      page.Header().Height(200)
#if troubleshoot
        .Background(Colors.Blue.Accent1)
#endif
        .Element(RenderHeader);

      page.Content().MinHeight(100)
#if troubleshoot
        .Background(Colors.Red.Accent1)
#endif
        .Element(Body);

      page.Footer().Height(150)
#if troubleshoot
        .Background(Colors.Yellow.Accent1)
#endif
        .Element(Footer);
    });
  }

  protected virtual void SetupPage(PageDescriptor page)
  {
    page.Margin(5);
    page.DefaultTextStyle(TextStyle.Default.FontFamily("Arial"));
  }

  protected abstract void RenderHeader(IContainer container);
  protected abstract void Body(IContainer container);
  protected abstract void Footer(IContainer container);
}