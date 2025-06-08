using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Catalog;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FSH.WebApi.Application.Tests.Catalog;

public class ProductExportTests
{
    private readonly IReadRepository<Product> _repository;
    private readonly IExcelWriter _excelWriter;

    public ProductExportTests()
    {
        _repository = Substitute.For<IReadRepository<Product>>();
        _excelWriter = Substitute.For<IExcelWriter>();
    }

    [Fact]
    public async Task ExportProducts_Should_Return_Excel_Stream()
    {
        var request = new ExportProductsRequest { BrandId = Guid.NewGuid() };

        var products = new List<ProductExportDto>
        {
            new() { Name = "P1", Description = "D1", Rate = 10m, BrandName = "B1" }
        };

        _repository.ListAsync(Arg.Any<ExportProductsWithBrandsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(products));

        var stream = new MemoryStream();
        _excelWriter.WriteToStream(products).Returns(stream);

        var handler = new ExportProductsRequestHandler(_repository, _excelWriter);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(stream);
        await _repository.Received(1).ListAsync(Arg.Any<ExportProductsWithBrandsSpecification>(), Arg.Any<CancellationToken>());
        _excelWriter.Received(1).WriteToStream(products);
    }

    [Fact]
    public async Task ExportProducts_Should_Handle_Empty_List()
    {
        var request = new ExportProductsRequest();
        var products = new List<ProductExportDto>();
        _repository.ListAsync(Arg.Any<ExportProductsWithBrandsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(products));
        var stream = new MemoryStream();
        _excelWriter.WriteToStream(products).Returns(stream);
        var handler = new ExportProductsRequestHandler(_repository, _excelWriter);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(stream);
        _excelWriter.Received(1).WriteToStream(products);
    }
}
