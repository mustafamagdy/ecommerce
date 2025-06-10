using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.Suppliers; // Contains Requests, Handlers, DTOs, Specs
using FSH.WebApi.Domain.Accounting; // Contains Supplier entity
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Contracts; // For IAggregateRoot, AuditableEntity (though not directly mocked)
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using Ardalis.Specification; // For ISpecification and AnyAsync

namespace Application.Tests.Accounting.Suppliers;

public class SupplierHandlerTests
{
    private readonly Mock<IRepository<Supplier>> _mockSupplierRepo;
    private readonly Mock<IReadRepository<Supplier>> _mockSupplierReadRepo;
    // Assuming handlers use specific localizers and loggers, e.g. IStringLocalizer<CreateSupplierHandler>
    // For simplicity in this combined test file, general mocks are created.
    // In a real scenario with separate test files per handler, specific mocks would be used.
    private readonly Mock<IStringLocalizer<CreateSupplierHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateSupplierHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateSupplierHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateSupplierHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetSupplierHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchSuppliersHandler>> _mockSearchLocalizer;
    private readonly Mock<IStringLocalizer<DeleteSupplierHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteSupplierHandler>> _mockDeleteLogger;

    // Mock for dependencies in DeleteSupplierHandler (if any, e.g., checking for related VendorInvoices)
    // For now, Supplier deletion doesn't have such explicit checks in its handler, but this is where they'd go.
    // private readonly Mock<IReadRepository<VendorInvoice>> _mockVendorInvoiceReadRepo;


    public SupplierHandlerTests()
    {
        _mockSupplierRepo = new Mock<IRepository<Supplier>>();
        _mockSupplierReadRepo = new Mock<IReadRepository<Supplier>>();
        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateSupplierHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateSupplierHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateSupplierHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateSupplierHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetSupplierHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchSuppliersHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteSupplierHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteSupplierHandler>>();

        // Setup default localization messages if needed for tests
        _mockCreateLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString(It.IsAny<string>(), It.IsAny<string>()));
        _mockUpdateLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString(It.IsAny<string>(), It.IsAny<string>()));
        _mockGetLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString(It.IsAny<string>(), It.IsAny<string>()));
        _mockDeleteLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString(It.IsAny<string>(), It.IsAny<string>()));
    }

    private Supplier CreateSampleSupplier(Guid id, string name) =>
        new Supplier(name, "contact@sample.com", "123 Sample St", "TAX123", Guid.NewGuid(), "Bank Details") { Id = id };


    // === CreateSupplierHandler Tests ===
    [Fact]
    public async Task CreateSupplierHandler_Should_CreateSupplier_WhenNameIsUnique()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "New Unique Supplier",
            ContactInfo = "contact@unique.com",
            Address = "1 Unique St",
            TaxId = "UNIQUE123",
            DefaultPaymentTermId = Guid.NewGuid(),
            BankDetails = "Unique Bank"
        };

        _mockSupplierRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<SupplierByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier)null!); // Simulate name is unique

        _mockSupplierRepo.Setup(r => r.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier s, CancellationToken ct) => { s.Id = Guid.NewGuid(); return s; }); // Simulate AddAsync sets ID

        var handler = new CreateSupplierHandler(_mockSupplierRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockSupplierRepo.Verify(r => r.AddAsync(It.Is<Supplier>(s => s.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSupplierHandler_Should_ThrowConflictException_WhenNameIsNotUnique()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Existing Supplier" };
        var existingSupplier = CreateSampleSupplier(Guid.NewGuid(), request.Name);

        _mockSupplierRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<SupplierByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSupplier); // Simulate name exists

        var handler = new CreateSupplierHandler(_mockSupplierRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
        _mockSupplierRepo.Verify(r => r.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // === UpdateSupplierHandler Tests ===
    [Fact]
    public async Task UpdateSupplierHandler_Should_UpdateSupplier_WhenFoundAndNameIsUnique()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var request = new UpdateSupplierRequest
        {
            Id = supplierId,
            Name = "Updated Supplier Name",
            ContactInfo = "updated@contact.com"
        };
        var existingSupplier = CreateSampleSupplier(supplierId, "Old Supplier Name");

        _mockSupplierRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSupplier);
        _mockSupplierRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<SupplierByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier)null!); // Simulate new name is unique (or unchanged)

        var handler = new UpdateSupplierHandler(_mockSupplierRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(supplierId);
        _mockSupplierRepo.Verify(r => r.UpdateAsync(It.Is<Supplier>(s => s.Id == supplierId && s.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        existingSupplier.Name.Should().Be(request.Name); // Check if Update method on entity was effective
        existingSupplier.ContactInfo.Should().Be(request.ContactInfo);
    }

    [Fact]
    public async Task UpdateSupplierHandler_Should_ThrowNotFoundException_WhenSupplierNotFound()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), Name = "Any Name" };
        _mockSupplierRepo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier)null!);

        var handler = new UpdateSupplierHandler(_mockSupplierRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateSupplierHandler_Should_ThrowConflictException_WhenNewNameConflictsWithAnotherSupplier()
    {
        // Arrange
        var supplierIdToUpdate = Guid.NewGuid();
        var conflictingSupplierId = Guid.NewGuid();
        var request = new UpdateSupplierRequest { Id = supplierIdToUpdate, Name = "Conflicting Name" };
        var supplierToUpdate = CreateSampleSupplier(supplierIdToUpdate, "Original Name");
        var otherSupplierWithConflictingName = CreateSampleSupplier(conflictingSupplierId, request.Name);

        _mockSupplierRepo.Setup(r => r.GetByIdAsync(supplierIdToUpdate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplierToUpdate);
        _mockSupplierRepo.Setup(r => r.FirstOrDefaultAsync(It.Is<SupplierByNameSpec>(spec => spec.Name == request.Name), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherSupplierWithConflictingName); // Name conflict with a *different* supplier

        var handler = new UpdateSupplierHandler(_mockSupplierRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetSupplierHandler Tests ===
    [Fact]
    public async Task GetSupplierHandler_Should_ReturnSupplierDto_WhenFound()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var request = new GetSupplierRequest(supplierId);
        var supplierEntity = CreateSampleSupplier(supplierId, "Test Supplier");

        // Assuming GetSupplierHandler uses GetByIdAsync, not a spec, as per its implementation.
        // If it used FirstOrDefaultAsync with a spec, the setup would be:
        // _mockSupplierReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<SupplierByIdSpec>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(supplierEntity.Adapt<SupplierDto>()); // If spec returns DTO
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplierEntity);

        var handler = new GetSupplierHandler(_mockSupplierReadRepo.Object, _mockGetLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(supplierId);
        result.Name.Should().Be(supplierEntity.Name);
        // Mapster does the rest of the mapping.
    }

    [Fact]
    public async Task GetSupplierHandler_Should_ThrowNotFoundException_WhenSupplierNotFound()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var request = new GetSupplierRequest(supplierId);
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier)null!);

        var handler = new GetSupplierHandler(_mockSupplierReadRepo.Object, _mockGetLocalizer.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === SearchSuppliersHandler Tests ===
    [Fact]
    public async Task SearchSuppliersHandler_Should_ReturnPaginatedSupplierDtos()
    {
        // Arrange
        var request = new SearchSuppliersRequest { PageNumber = 1, PageSize = 10, Keyword = "Test" };
        var suppliersList = new List<Supplier>
        {
            CreateSampleSupplier(Guid.NewGuid(), "Test Supplier One"),
            CreateSampleSupplier(Guid.NewGuid(), "Another Test Co")
        };
        // If the spec returns DTOs directly (Specification<Supplier, SupplierDto>)
        // var supplierDtosList = suppliersList.Adapt<List<SupplierDto>>();
        // _mockSupplierReadRepo.Setup(r => r.ListAsync(It.IsAny<SuppliersBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(supplierDtosList);

        // Assuming spec returns entities and handler adapts
        _mockSupplierReadRepo.Setup(r => r.ListAsync(It.IsAny<SuppliersBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(suppliersList);
        _mockSupplierReadRepo.Setup(r => r.CountAsync(It.IsAny<SuppliersBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(suppliersList.Count);

        var handler = new SearchSuppliersHandler(_mockSupplierReadRepo.Object, _mockSearchLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(suppliersList.Count);
        result.TotalCount.Should().Be(suppliersList.Count);
        result.Data.First().Name.Should().Be(suppliersList.First().Name);
        // Mapster handles the mapping from entity to DTO.
    }

    // === DeleteSupplierHandler Tests ===
    [Fact]
    public async Task DeleteSupplierHandler_Should_DeleteSupplier_WhenFoundAndNoDependencies()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var request = new DeleteSupplierRequest(supplierId);
        var supplierEntity = CreateSampleSupplier(supplierId, "ToDelete");

        _mockSupplierRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplierEntity);
        // Mock dependency checks if DeleteSupplierHandler implements them.
        // e.g., _mockVendorInvoiceReadRepo.Setup(r => r.AnyAsync(It.IsAny<ISpecification<VendorInvoice>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new DeleteSupplierHandler(_mockSupplierRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(supplierId);
        _mockSupplierRepo.Verify(r => r.DeleteAsync(supplierEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSupplierHandler_Should_ThrowNotFoundException_WhenSupplierNotFound()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var request = new DeleteSupplierRequest(supplierId);
        _mockSupplierRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier)null!);

        var handler = new DeleteSupplierHandler(_mockSupplierRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // Example if DeleteSupplierHandler had dependency checks:
    // [Fact]
    // public async Task DeleteSupplierHandler_Should_ThrowConflictException_WhenDependenciesExist()
    // {
    //     // Arrange
    //     var supplierId = Guid.NewGuid();
    //     var request = new DeleteSupplierRequest(supplierId);
    //     var supplierEntity = CreateSampleSupplier(supplierId, "ToDeleteWithDeps");
    //
    //     _mockSupplierRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(supplierEntity);
    //     // _mockVendorInvoiceReadRepo.Setup(r => r.AnyAsync(It.IsAny<ISpecification<VendorInvoice>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); // Simulate dependency
    //
    //     var handler = new DeleteSupplierHandler(_mockSupplierRepo.Object, _mockVendorInvoiceReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);
    //
    //     // Act & Assert
    //     await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    // }
}

// Helper classes for Specifications if not accessible or to simplify mock setup
// public class SupplierByNameSpec : Specification<Supplier>, ISingleResultSpecification
// {
//    public string Name { get; }
//    public SupplierByNameSpec(string name) { Name = name; Query.Where(s => s.Name == name); }
// }

// public class SuppliersBySearchFilterSpec : Specification<Supplier, SupplierDto> { /* ... */ }
// public class SupplierByIdSpec : Specification<Supplier, SupplierDto>, ISingleResultSpecification { /* ... */ }
