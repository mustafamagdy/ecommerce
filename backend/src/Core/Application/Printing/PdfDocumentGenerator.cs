using FSH.WebApi.Application.Common.Pdf;
using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace FSH.WebApi.Application.Printing;

public class InvoiceDocument : BasePdfDocument
{
  private readonly BoundTemplate _boundedTemplate;

  public InvoiceDocument(BoundTemplate boundedTemplate)
  {
    _boundedTemplate = boundedTemplate;
  }

  protected override void SetupPage(PageDescriptor page)
  {
    base.SetupPage(page);

    if (_boundedTemplate.ContinuousSize)
    {
      page.ContinuousSize(_boundedTemplate.Width ?? 3, Unit.Inch);
    }
  }

  protected override void RenderHeader(IContainer container)
  {
    container
      .ShowDebug(_boundedTemplate.ShowDebug, "header")
      .MaxHeight(200)
      .Column(col =>
      {
        var components = GetComponentsFor(SectionPosition.Header);
        foreach (var component in components)
        {
          component.Render(col);
        }
      });
  }

  private IReadOnlyCollection<BoundedSection> GetComponentsFor(SectionPosition position)
    => _boundedTemplate.Sections.Where(a => a.Position == position).ToList().AsReadOnly();

  protected override void Body(IContainer container)
  {
    container
      .ShowDebug(_boundedTemplate.ShowDebug, "body")
      .PaddingVertical(5)
      .Column(col =>
      {
        col.Spacing(5);
        var components = GetComponentsFor(SectionPosition.Body);
        foreach (var component in components)
        {
          component.Render(col);
        }
      });
  }

  protected override void Footer(IContainer container)
  {
    container
      .ShowDebug(_boundedTemplate.ShowDebug, "footer")
      .Column(col =>
      {
        var components = GetComponentsFor(SectionPosition.Footer);
        foreach (var component in components)
        {
          component.Render(col);
        }
      });
  }
}