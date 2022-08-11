using Ardalis.SmartEnum;
using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public abstract class PrintableDocument
{
}

public abstract class PrintableReceipt : PrintableDocument
{
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

public interface IDocumentSection
{
  SectionType Type { get; }
  int Order { get; }
  SectionAlignment Alignment { get; }
  SectionPosition Position { get; }

  void RenderInColumn(ColumnDescriptor col);
}