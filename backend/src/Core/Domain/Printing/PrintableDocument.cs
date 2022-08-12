using System.ComponentModel.DataAnnotations.Schema;
using Ardalis.SmartEnum;
using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public abstract class PrintableDocument : AuditableEntity
{
  private readonly List<DocumentSection> _sections = new();

  public bool Active { get; set; }
  public IReadOnlyCollection<DocumentSection> Sections => _sections.AsReadOnly();

  protected void AddSection(DocumentSection section) => _sections.Add(section);
}

public class PrintableType : SmartEnum<PrintableType>
{
  public static PrintableType Receipt = new(nameof(Receipt), 1);

  public PrintableType(string name, int value)
    : base(name, value)
  {
  }
}

public class SectionAlignment : SmartEnum<SectionAlignment>
{
  public static SectionAlignment Center = new(nameof(Center), 1);

  public SectionAlignment(string name, int value)
    : base(name, value)
  {
  }
}

public class SectionPosition : SmartEnum<SectionPosition>
{
  public static SectionPosition Header = new(nameof(Header), 1);
  public static SectionPosition Body = new(nameof(Body), 2);
  public static SectionPosition Footer = new(nameof(Footer), 3);

  public SectionPosition(string name, int value)
    : base(name, value)
  {
  }
}

public class SectionType : SmartEnum<SectionType>
{
  public static SectionType Logo = new(nameof(Logo), 1);
  public static SectionType Title = new(nameof(Title), 2);
  public static SectionType Barcode = new(nameof(Barcode), 3);
  public static SectionType QrCode = new(nameof(QrCode), 4);

  public SectionType(string name, int value)
    : base(name, value)
  {
  }
}

public abstract class DocumentSection
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

  [NotMapped]
  public abstract SectionType Type { get; }
}