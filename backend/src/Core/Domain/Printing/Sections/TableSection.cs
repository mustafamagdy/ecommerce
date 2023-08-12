namespace FSH.WebApi.Domain.Printing.Sections;

public sealed class TableSection : DocumentSection
{
  private TableSection()
    : this(-1, SectionAlignment.Center, SectionPosition.Body)
  {
  }

  public TableSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }

  /// <summary>
  /// Comma separated text.
  /// </summary>
  public string HeaderTitle { get; set; }

  /// <summary>
  /// Comma separated column size (c25,c25,r):
  /// c25 => constant width, 25 value.
  /// r => relative.
  /// </summary>
  public string ColumnDefs { get; set; }

  /// <summary>
  /// Comma separated header style options:
  /// font=8,semi-bold.
  /// </summary>
  public string HeaderStyle { get; set; }
}