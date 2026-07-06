using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Validators.RawCoils;
using CoilManager.Domain.Enums;

namespace CoilManager.UnitTests.RawCoils;

public sealed class RawCoilValidatorTests
{
    [Fact]
    public async Task CreateValidator_RejectsMissingCoilNumber()
    {
        CreateRawCoilRequest request = ValidCreateRequest() with { CoilNumber = string.Empty };
        CreateRawCoilRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRawCoilRequest.CoilNumber));
    }

    [Fact]
    public async Task UpdateValidator_RejectsInvalidStatus()
    {
        UpdateRawCoilRequest request = ValidUpdateRequest() with { Status = (CoilStatus)999 };
        UpdateRawCoilRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRawCoilRequest.Status));
    }

    [Fact]
    public async Task CreateValidator_RejectsMissingSupplierId()
    {
        CreateRawCoilRequest request = ValidCreateRequest() with { SupplierId = Guid.Empty };
        CreateRawCoilRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRawCoilRequest.SupplierId));
    }

    [Fact]
    public async Task CreateValidator_RejectsMissingGradeId()
    {
        CreateRawCoilRequest request = ValidCreateRequest() with { GradeId = Guid.Empty };
        CreateRawCoilRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRawCoilRequest.GradeId));
    }

    [Fact]
    public async Task CreateValidator_RejectsMissingManufacturerId()
    {
        CreateRawCoilRequest request = ValidCreateRequest() with { ManufacturerId = Guid.Empty };
        CreateRawCoilRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRawCoilRequest.ManufacturerId));
    }

    private static CreateRawCoilRequest ValidCreateRequest()
    {
        return new CreateRawCoilRequest(
            "CN-001",
            "HN-001",
            "PO-001",
            "INV-001",
            "TC-001",
            "BIS-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            10,
            0,
            null,
            "A1",
            DateOnly.FromDateTime(DateTime.UtcNow));
    }

    private static UpdateRawCoilRequest ValidUpdateRequest()
    {
        return new UpdateRawCoilRequest(
            "CN-001",
            "HN-001",
            "PO-001",
            "INV-001",
            "TC-001",
            "BIS-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            10,
            0,
            null,
            "A1",
            CoilStatus.Available,
            DateOnly.FromDateTime(DateTime.UtcNow),
            Convert.ToBase64String([1, 2, 3]));
    }
}
