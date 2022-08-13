using System.Collections;
using System.Text.RegularExpressions;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Extensions;
using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Printing;

public abstract class BoundedSection
{
  public abstract bool ShowDebug { get; }
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

  public override bool ShowDebug => _decorated.ShowDebug;
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
      .ShowDebug(_decorated)
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

  public override bool ShowDebug => _decorated.ShowDebug;
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
    col.Item()
      .ShowDebug(_decorated)
      .Align(_decorated.Alignment)
      .Text(text =>
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

  public override bool ShowDebug => _decorated.ShowDebug;
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
      .ShowDebug(_decorated)
      .Height(38)
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

  public override bool ShowDebug => _decorated.ShowDebug;
  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col.Item()
      .ShowDebug(_decorated)
      .Height(30)
      .AlignMiddle()
      .AlignCenter()
      .Width(100)
      // .Image(ImageData, ImageScaling.Resize);
      .Image(QuestPDF.Helpers.Placeholders.Image(100, 100), ImageScaling.Resize);
  }
}

public class BoundedTableSection : BoundedSection
{
  private readonly TableSection _decorated;
  private readonly object _dataSource;
  private string[] _columnHeaders;
  private string[] _columnDefs;
  private string[] _headerStyles;
  private IEnumerable? _list;
  private List<Func<object, object?>> _propAccessort;
  private Dictionary<int, string> _columnAlignment = new();

  public BoundedTableSection(TableSection decorated, object dataSource)
  {
    _decorated = decorated;
    _dataSource = dataSource;
  }

  public override bool ShowDebug => _decorated.ShowDebug;
  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  protected override void EvaluateExpressionValues()
  {
    _columnHeaders = _decorated.HeaderTitle.Split(',');
    _columnDefs = _decorated.ColumnDefs.Split(',');
    _headerStyles = _decorated.HeaderStyle.Split(',');

    var sourceProp = GetArrayPropName();
    _list = sourceProp.GetPropertyValue(_dataSource) as IEnumerable;

    _propAccessort = GetBindingProperties().ToList();
  }

  private IEnumerable<Func<object, object?>> GetBindingProperties()
  {
    var regex = new Regex(@"\((?<props>.+)\)");
    var propNames = regex.Match(_decorated.BindingProperty).Groups["props"].Value;
    var props = propNames.Split(",");
    foreach (var prop in props)
    {
      yield return src => prop.GetPropertyValue(src);
    }
  }

  private string GetArrayPropName()
  {
    return _decorated.BindingProperty.Split("[]")[0][1..];
  }

  public override void Render(ColumnDescriptor col)
  {
    EvaluateExpressionValues();
    col.Item().Element(ComposeTable);
  }

  private void ComposeTable(IContainer container)
  {
    container
      .ShowDebug(_decorated)
      .AlignTop()
      .Table(table =>
      {
        DefineTableColumns(table);

        BuildTableHeaders(table);

        BuildTableContent(table);
      });
  }

  private void BuildTableContent(TableDescriptor table)
  {
    _ = _list ?? throw new InvalidOperationException("Table source is not an IEnumerable");

    var enumerator = _list.GetEnumerator();
    while (enumerator.MoveNext())
    {
      var item = enumerator.Current;
      var values = _propAccessort.Select(p => p(item)).ToArray();
      for (int idx = 0; idx < values.Length; idx++)
      {
        object? value = values[idx];
        table.Cell().Element(CellStyle).Align(_columnAlignment[idx]).AlignTop().Text(value);
      }
    }

    static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);
  }

  private void DefineTableColumns(TableDescriptor table)
  {
    table.ColumnsDefinition(columns =>
    {
      foreach (var colDef in _columnDefs)
      {
        if (colDef.ToLower().Trim() == "r")
        {
          columns.RelativeColumn();
        }
        else
        {
          var colWidth = Convert.ToInt32(colDef[1..]);
          columns.ConstantColumn(colWidth);
        }
      }
    });
  }

  private void BuildTableHeaders(TableDescriptor table)
  {
    var headerStyle = EvaluateHeaderStyles();
    table.Header(header =>
    {
      for (int cidx = 0; cidx < _columnHeaders.Length; cidx++)
      {
        string? colHeader = _columnHeaders[cidx];
        var headerProps = colHeader.Split('%');
        var title = headerProps[0];
        var align = headerProps[1];
        _columnAlignment.Add(cidx, align);
        header.Cell().Align(align).Text(title).Style(headerStyle);
      }
    });
  }

  private static TextStyle EvaluateHeaderStyles()
  {
    // apply header styles from _headerStyles
    return TextStyle.Default.FontSize(8).SemiBold();
  }
}

public static class PdfExtensions
{
  public static IContainer ShowDebug(this IContainer container, DocumentSection section)
    => ShowDebug(container, section.ShowDebug, section.GetType().Name);

  public static IContainer ShowDebug(this IContainer container, bool showDebug, string title)
    => showDebug ? container.DebugArea(title, RandomColor) : container;

  public static IContainer Align(this ITableCellContainer cell, string align)
  {
    return align switch
    {
      "al" => cell.AlignLeft(),
      "ar" => cell.AlignRight(),
      "ac" => cell.AlignCenter(),
      _ => throw new ArgumentOutOfRangeException(nameof(align))
    };
  }

  public static IContainer Align(this IContainer cell, string align)
  {
    return align switch
    {
      "al" => cell.AlignLeft(),
      "ar" => cell.AlignRight(),
      "ac" => cell.AlignCenter(),
      _ => throw new ArgumentOutOfRangeException(nameof(align))
    };
  }

  private static string[] colors =
  {
    "#ff8a80", "#ff80ab", "#ea80fc", "#b388ff",
    "#8c9eff", "#82b1ff", "#80d8ff", "#84ffff",
    "#a7ffeb", "#b9f6ca", "#ccff90", "#f4ff81",
    "#ffff8d", "#ffe57f", "#ffd180", "#ff9e80",
  };

  private static string RandomColor => colors.OrderBy(a => Guid.NewGuid()).First();
}