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

    builder.Property(a => a.Type)
      .HasConversion(
        p => p.Value,
        p => PrintableType.FromValue(p));

    builder.Property(a => a.Type)
      .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .HasDiscriminator<string>("type")
      .HasValue<SimpleReceiptInvoice>(PrintableType.Receipt.Name)
      .HasValue<WideReceiptInvoice>(PrintableType.Wide.Name);

    builder.HasMany(a => a.Sections).WithOne().HasForeignKey(a => a.DocumentId);
    var sections = builder.Navigation(nameof(PrintableDocument.Sections));
    sections.Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class DocumentSectionConfig : BaseTenantEntityConfiguration<DocumentSection, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<DocumentSection> builder)
  {
    base.Configure(builder);

    builder.Property(a => a.Type)
      .HasConversion(
        p => p.Value,
        p => SectionType.FromValue(p));

    builder.Property(a => a.Type)
      .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

    builder
      .HasDiscriminator<string>("type")
      .HasValue<LogoSection>(SectionType.Logo.Name)
      .HasValue<BarcodeSection>(SectionType.Barcode.Name)
      .HasValue<TitleSection>(SectionType.Title.Name);
  }
}