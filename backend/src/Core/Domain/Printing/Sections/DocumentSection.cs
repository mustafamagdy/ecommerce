using FSH.WebApi.Domain.Serialization;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.Printing.Sections;

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