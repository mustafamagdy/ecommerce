using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Common.Exporters;

public interface IPdfWriter : ITransientService
{
  Stream WriteToStream<T>(in T document)
      where T : IDocument;
}