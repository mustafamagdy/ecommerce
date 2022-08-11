using FSH.WebApi.Application.Common.Pdf;
using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace FSH.WebApi.Application.Printing;

public class InvoiceDocument : BasePdfDocument
{
  private readonly ContinuesFixedSizeReceiptInvoice _template;

  public InvoiceDocument(ContinuesFixedSizeReceiptInvoice template)
  {
    _template = template;
  }

  protected override void SetupPage(PageDescriptor page)
  {
    base.SetupPage(page);

    page.ContinuousSize(_template.Width, Unit.Inch);
  }

  protected override void RenderHeader(IContainer container)
  {
    container.Column(col =>
    {
      var components = GetComponentsFor(SectionPosition.Header);
      foreach (var component in components)
      {
        component.RenderInColumn(col);
      }
    });
  }

  private IReadOnlyCollection<IDocumentSection> GetComponentsFor(SectionPosition position)
    => _template.Sections.Where(a => a.Position == position).OrderBy(a => a.Order).ToList().AsReadOnly();

  protected override void Body(IContainer container)
  {
    container.Column(col =>
    {
      var components = GetComponentsFor(SectionPosition.Body);
      foreach (var component in components)
      {
        component.RenderInColumn(col);
      }
    });
  }

  protected override void Footer(IContainer container)
  {
    container.Column(col =>
    {
      var components = GetComponentsFor(SectionPosition.Footer);
      foreach (var component in components)
      {
        component.RenderInColumn(col);
      }
    });
  }
}