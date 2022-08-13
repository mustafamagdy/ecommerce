using FSH.WebApi.Domain.Printing;

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
    return section switch
    {
      LogoSection logoSection => new BoundedLogoSection(logoSection, dto),
      BarcodeSection barcodeSection => new BoundedBarcodeSection(barcodeSection, dto),
      TitleSection titleSection => new BoundedTitleSection(titleSection, dto),
      TwoItemRowSection twoItemRowSection => new BoundedTwoItemRowSection(twoItemRowSection, dto),
      TableSection tableSection => new BoundedTableSection(tableSection, dto),
      _ => throw new ArgumentException(nameof(section))
    };
  }
}