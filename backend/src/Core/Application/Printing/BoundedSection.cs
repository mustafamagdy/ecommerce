using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Printing;

public abstract class BoundedSection
{
  public abstract SectionPosition Position { get; }
  public abstract SectionAlignment Alignment { get; }
  protected abstract void EvaluateExpressionValues();
  public abstract void Render(ColumnDescriptor col);
}

public class BoundedTitleSection : BoundedSection
{
  private readonly TitleSection _decorated;
  private readonly object _dataSource;
  private string _content = string.Empty;

  public BoundedTitleSection(TitleSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
    var values = _decorated.BindingProperty.EvaluatePropertyValues(_dataSource);
    _content = values[0];
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col
      .Item()
      // .ShowDebugArea()
      .AlignCenter().AlignMiddle()
      .Text(_content)
      .FontSize(_decorated.FontSize);
  }
}

public class BoundedTwoItemRowSection : BoundedSection
{
  private readonly TwoItemRowSection _decorated;
  private readonly object _dataSource;
  private string _item1 = string.Empty;
  private string _item2 = string.Empty;

  public BoundedTwoItemRowSection(TwoItemRowSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
    var values = _decorated.BindingProperty.EvaluatePropertyValues(_dataSource);
    _item1 = values[0];
    _item2 = values[1];
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col.Item().Text(text =>
    {
      text.Span(_item1);
      text.Span(_item2);
    });
  }
}

public class BoundedBarcodeSection : BoundedSection
{
  private readonly BarcodeSection _decorated;
  private readonly object _dataSource;
  private string _content = string.Empty;

  public BoundedBarcodeSection(BarcodeSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
    var values = _decorated.BindingProperty.EvaluatePropertyValues(_dataSource);
    _content = values[0];
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col
      .Item()
      .Height(38)
      // .ShowDebugArea()
      .Background(Colors.White)
      .AlignCenter()
      .AlignMiddle()
      .Text($"*{_content}*")
      .LineHeight(1)
      .FontFamily("Libre Barcode 39") // use real font family name
      .FontSize(36);
  }
}

public class BoundedLogoSection : BoundedSection
{
  private readonly LogoSection _decorated;
  private readonly object _dataSource;

  public BoundedLogoSection(LogoSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}

public class BoundedTableSection : BoundedSection
{
  private readonly TableSection _decorated;
  private readonly object _dataSource;

  public BoundedTableSection(TableSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col.Item().Element(ComposeTable);

    var totalPrice = 0; //Data.TotalAmount;
    col.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}").SemiBold();
  }

  private void ComposeTable(IContainer container)
  {
    var headerStyle = TextStyle.Default.FontSize(8).SemiBold();

    container.Table(table =>
    {
      table.ColumnsDefinition(columns =>
      {
        columns.ConstantColumn(25);
        columns.ConstantColumn(40);
        columns.RelativeColumn();
      });

      table.Header(header =>
      {
        header.Cell().AlignLeft().Text("Price").Style(headerStyle);
        header.Cell().AlignLeft().Text("Qty").Style(headerStyle);
        header.Cell().AlignRight().Text("Item").Style(headerStyle);
      });

      // foreach (var item in Data)
      // {
      //   int qty = item.Qty;
      //   string itemName = item.ItemName;
      //   table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text($"{item.Price:F0}");
      //   table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text(qty);
      //   table.Cell().Element(CellStyle).AlignRight().AlignTop().Text(itemName);
      //
      //   static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);
      //
      //   // .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
      // }
    });
  }
}