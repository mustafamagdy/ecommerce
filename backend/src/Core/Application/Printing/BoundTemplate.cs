using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using FSH.WebApi.Shared.Extensions;
using QuestPDF.Helpers;

namespace FSH.WebApi.Application.Printing;

public class BoundTemplate
{
  private readonly PrintableDocument _decorated;
  private List<BoundedSection> _sections = new();

  public BoundTemplate(PrintableDocument decorated)
  {
    _decorated = decorated;
  }

  public int? Width => _decorated.Width;
  public bool ContinuousSize => _decorated.ContinuousSize;
  public IReadOnlyCollection<BoundedSection> Sections => _sections.AsReadOnly();

  public void BindTemplate<T>(T dto)
    where T : notnull
  {
    if (dto == null) throw new ArgumentNullException(nameof(dto));
    _sections = _decorated.Sections
      .Where(a => !string.IsNullOrEmpty(a.BindingProperty))
      .OrderBy(a => a.Order)
      .Select(s => BindSection(s, dto))
      .ToList();
  }

  private BoundedSection BindSection<T>(DocumentSection section, T dto)
    where T : notnull
  {
    if (section.BindingProperty.Contains(','))
    {
      var props = section.BindingProperty.Split(",");
      var propValues = props.Select(p => p.StartsWith("$") ? EvaluatePropertyValue(p, dto) : p).ToArray();

      if (propValues.Length != 2)
      {
        throw new ArgumentOutOfRangeException("We only support TwoItemRowSection, so we need only two props");
      }

      return new BoundedTwoItemRowSection(section as TwoItemRowSection, (propValues[0] ?? "").ToString(), (propValues[1] ?? "").ToString());
    }

    if (!section.BindingProperty.StartsWith("$"))
    {
      return new BoundedTitleSection(section as TitleSection, section.BindingProperty);
    }

    var value = EvaluatePropertyValue(section.BindingProperty, dto);
    switch (section)
    {
      case LogoSection logoSection:
        break;
      case BarcodeSection barcodeSection:
        return new BoundedBarcodeSection(barcodeSection, (value ?? "").ToString());
      case TitleSection titleSection:
        return new BoundedTitleSection(titleSection, (value ?? "").ToString());
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