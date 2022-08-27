using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using FSH.WebApi.Domain.Printing.Sections;
using FSH.WebApi.Domain.Serialization;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.Printing;

[JsonConverter(typeof(PrintableDocumentJsonConverter))]
public abstract class PrintableDocument : AuditableEntity, IAggregateRoot
{
  [JsonProperty(nameof(Sections))]
  private readonly List<DocumentSection> _sections = new();

  public bool Active { get; set; }
  public int? Width { get; set; }
  public bool ContinuousSize { get; set; }

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
  public static SectionAlignment Center = new(nameof(Center), "ac");
  public static SectionAlignment Right = new(nameof(Right), "ar");
  public static SectionAlignment Left = new(nameof(Left), "al");

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

  public static SectionType Table = new(nameof(Table), nameof(Table));
  // public static SectionType QrCode = new(nameof(QrCode), nameof(QrCode));

  public SectionType(string name, string value)
    : base(name, value)
  {
  }
}