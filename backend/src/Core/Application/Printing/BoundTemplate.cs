using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Extensions;

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

    string[] values = EvaluatePropertyValues(section.BindingProperty, dto);
    switch (section)
    {
      case LogoSection logoSection:
        break;
      case BarcodeSection barcodeSection:
        return new BoundedBarcodeSection(barcodeSection, values[0]);
      case TitleSection titleSection:
        return new BoundedTitleSection(titleSection, values[0]);
      case TwoItemRowSection twoItemRowSection:
        return new BoundedTwoItemRowSection(twoItemRowSection, values[0], values[1]);
    }

    throw new ArgumentException(nameof(section));
  }

  private string[] EvaluatePropertyValues<T>(string exp, T dto)
    where T : notnull
  {
    var exps = exp.Split(',');
    return exps.Select(e => EvaluatePropertyValue(e, dto)).ToArray();
  }

  private string EvaluatePropertyValue<T>(string exp, T dto)
    where T : notnull
  {
    if (!exp.StartsWith("$"))
    {
      return exp;
    }

    var propExp = exp[1..];

    // $name -> done
    // $sub.name -> done
    // $sub.sub.name -> done
    // $items[].name
    var propValue = GetPropertyValue(dto, propExp);
    var value = (propValue ?? "").ToString();
    return value;
  }

  private object? GetPropertyValue(object obj, string exp)
  {
    if (!exp.Contains('.'))
    {
      return obj.TryGetPropertyValue<object?>(exp);
    }

    foreach (var prop in exp.Split('.').Select(s => obj.GetType().GetProperty(s)))
      obj = prop.GetValue(obj, null);

    return obj;
  }
}