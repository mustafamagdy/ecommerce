using FSH.WebApi.Domain.Printing;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class PrintableTemplateConfig : BaseTenantEntityConfiguration<PrintableDocument, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<PrintableDocument> builder)
  {
    base.Configure(builder);

    // builder.Property(a => a.Type)
    //   .HasConversion(
    //     p => p.Name,
    //     p => PrintableType.FromValue(p));
    //
    // builder.Property(a => a.Type)
    //   .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .HasDiscriminator<string>("Type")
      .HasValue<SimpleReceiptInvoice>(PrintableType.Receipt.Name)
      .HasValue<WideReceiptInvoice>(PrintableType.Wide.Name);

    builder.HasMany(a => a.Sections).WithOne(a => a.Document).HasForeignKey(a => a.DocumentId);
    var sections = builder.Navigation(nameof(PrintableDocument.Sections));
    sections.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class DocumentSectionConfig : BaseTenantEntityConfiguration<DocumentSection, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<DocumentSection> builder)
  {
    base.Configure(builder);

    builder.Property(a => a.Alignment)
      .HasConversion(
        p => p.Name,
        p => SectionAlignment.FromValue(p));

    builder.Property(a => a.Position)
      .HasConversion(
        p => p.Name,
        p => SectionPosition.FromValue(p));

    // builder.Property(a => a.Type)
    //   .HasConversion(
    //     p => p.Name,
    //     p => SectionType.FromValue(p));
    //
    // builder.Property(a => a.Type)
    //   .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .HasDiscriminator<string>("Type")
      .HasValue<LogoSection>(SectionType.Logo.Name)
      .HasValue<BarcodeSection>(SectionType.Barcode.Name)
      .HasValue<TitleSection>(SectionType.Title.Name)
      .HasValue<TwoItemRowSection>(SectionType.TwoPartTitle.Name)
      .HasValue<TableSection>(SectionType.Table.Name);
  }
}