using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Multitenancy;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Common.Pdf;

public abstract class BasePdfDocument : IDocument
{
  private readonly SubscriptionType _subscriptionType;

  public BasePdfDocument(SubscriptionType subscriptionType)
  {
    _subscriptionType = subscriptionType;
  }

  public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

  protected bool IsDemo => _subscriptionType != SubscriptionType.Standard;

  public void Compose(IDocumentContainer container)
  {
    container.Page(page =>
    {
      SetupPage(page);

      page.Header()
        .Element(RenderHeader);

      page.Content()
        .Element(Body);

      page.Footer()
        .Element(Footer);

      if (IsDemo)
      {
        page.Foreground()
          .AlignMiddle()
          .AlignCenter()
          .Text("DEMO - Training")
          .FontSize(64)
          .FontColor(Colors.Blue.Lighten3);
      }
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