using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using FSH.WebApi.Shared.Extensions;

namespace FSH.WebApi.Application.Printing;

public class BoundTemplate
{
  private readonly PrintableDocument _decorated;
  private List<BoundSection> _sections = new();

  public BoundTemplate(PrintableDocument decorated)
  {
    _decorated = decorated;
  }

  public IReadOnlyCollection<BoundSection> Sections => _sections.AsReadOnly();

  public void BindTemplate<T>(T dto)
    where T : notnull
  {
    if (dto == null) throw new ArgumentNullException(nameof(dto));
    _sections = _decorated.Sections.Select(s => BindSection(s, dto)).ToList();
  }

  private BoundSection BindSection<T>(DocumentSection section, T dto)
    where T : notnull
  {
    if (string.IsNullOrEmpty(section.BindingProperty))
    {
      throw new NullReferenceException(nameof(section.BindingProperty));
    }

    if (section.BindingProperty.Contains(","))
    {
      var props = section.BindingProperty.Split(",");
      var propValues = props.Select(p =>
      {
        if (p.StartsWith("$"))
          return EvaluatePropertyValue(p, dto);
        else
          return p;
      }).ToArray();

      if (propValues.Length != 2)
      {
        throw new ArgumentOutOfRangeException("We only support TwoItemRowSection, so we need only two props");
      }

      return new BoundTwoItemRowSection(section as TwoItemRowSection, (propValues[0] ?? "").ToString(), (propValues[1] ?? "").ToString());
    }

    if (!section.BindingProperty.StartsWith("$"))
    {
      return new BoundTitleSection(section as TitleSection, section.BindingProperty);
    }

    var value = EvaluatePropertyValue(section.BindingProperty, dto);
    switch (section)
    {
      case LogoSection logoSection:
        break;
      case BarcodeSection barcodeSection:
        return new BoundBarcodeSection(barcodeSection, (value ?? "").ToString());
      case TitleSection titleSection:
        return new BoundTitleSection(titleSection, (value ?? "").ToString());
    }

    throw new ArgumentException(nameof(section));
  }

  private object? EvaluatePropertyValue<T>(string exp, T dto)
    where T : notnull
  {
    var propName = exp[1..];
    var value = dto.TryGetPropertyValue<object?>(propName);
    return value;
  }
}

public abstract class BoundSection
{
  public abstract void RenderInColumn(ColumnDescriptor col);
}

public class BoundTitleSection : BoundSection
{
  public string Content { get; }
  private readonly TitleSection _decorated;

  public BoundTitleSection(TitleSection decorated, string content)
  {
    Content = content;
    _decorated = decorated;
  }

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}

public class BoundTwoItemRowSection : BoundSection
{
  public string Item1 { get; }
  public string Item2 { get; }
  private readonly TwoItemRowSection _decorated;

  public BoundTwoItemRowSection(TwoItemRowSection decorated, string item1, string item2)
  {
    Item1 = item1;
    Item2 = item2;
    _decorated = decorated;
  }

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}

public class BoundBarcodeSection : BoundSection
{
  public string Content { get; }
  private readonly BarcodeSection _decorated;

  public BoundBarcodeSection(BarcodeSection decorated, string content)
  {
    Content = content;
    _decorated = decorated;
  }

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}

public class BoundLogoSection : BoundSection
{
  private readonly LogoSection _decorated;

  public BoundLogoSection(LogoSection decorated)
  {
    _decorated = decorated;
  }

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}