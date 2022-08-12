using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using FSH.WebApi.Domain.Serialization;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.Printing;

[JsonConverter(typeof(PrintableDocumentJsonConverter))]
public abstract class PrintableDocument : AuditableEntity
{
  [JsonProperty(nameof(Sections))]
  private readonly List<DocumentSection> _sections = new();

  public bool Active { get; set; }

  [JsonIgnore]
  public IReadOnlyCollection<DocumentSection> Sections => _sections.AsReadOnly();

  protected void AddSection(DocumentSection section) => _sections.Add(section);
}

[JsonConverter(typeof(SmartEnumNameConverter<PrintableType, string>))]
public class PrintableType : SmartEnum<PrintableType, string>
{
  public static PrintableType Receipt = new(nameof(Receipt), nameof(Receipt));
  public static PrintableType Wide = new(nameof(Wide), nameof(Wide));

  public PrintableType(string name, string value)
    : base(name, value)
  {
  }
}

[JsonConverter(typeof(SmartEnumNameConverter<SectionAlignment, string>))]
public class SectionAlignment : SmartEnum<SectionAlignment, string>
{
  public static SectionAlignment Center = new(nameof(Center), nameof(Center));

  public SectionAlignment(string name, string value)
    : base(name, value)
  {
  }
}

[JsonConverter(typeof(SmartEnumNameConverter<SectionPosition, string>))]
public class SectionPosition : SmartEnum<SectionPosition, string>
{
  public static SectionPosition Header = new(nameof(Header), nameof(Header));
  public static SectionPosition Body = new(nameof(Body), nameof(Body));
  public static SectionPosition Footer = new(nameof(Footer), nameof(Footer));

  public SectionPosition(string name, string value)
    : base(name, value)
  {
  }
}

[JsonConverter(typeof(SmartEnumNameConverter<SectionType, string>))]
public class SectionType : SmartEnum<SectionType, string>
{
  public static SectionType Logo = new(nameof(Logo), nameof(Logo));
  public static SectionType Title = new(nameof(Title), nameof(Title));
  public static SectionType Barcode = new(nameof(Barcode), nameof(Barcode));

  public static SectionType TwoPartTitle = new(nameof(TwoPartTitle), nameof(TwoPartTitle));
  // public static SectionType QrCode = new(nameof(QrCode), nameof(QrCode));

  public SectionType(string name, string value)
    : base(name, value)
  {
  }
}

[JsonConverter(typeof(DocumentSectionJsonConverter))]
public abstract class DocumentSection : BaseEntity
{
  protected DocumentSection(int order, SectionAlignment alignment, SectionPosition position, bool showDebug)
  {
    Order = order;
    Alignment = alignment;
    Position = position;
    ShowDebug = showDebug;
  }

  public int Order { get; set; }
  public SectionAlignment Alignment { get; set; }
  public SectionPosition Position { get; set; }
  public bool ShowDebug { get; set; }

  /// <summary>
  /// Dynamic values starts with $ (ex $Name)
  /// Array access using [] (ex $Items[].Name)
  /// Static values don't have start with anything.
  /// </summary>
  public string BindingProperty { get; set; }

  public Guid DocumentId { get; set; }
  public PrintableDocument Document { get; set; }
}