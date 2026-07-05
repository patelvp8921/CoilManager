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

    private static CreateRawCoilRequest ValidCreateRequest()
    {
        return new CreateRawCoilRequest(
            "CN-001",
            "HN-001",
            "Prime Mill",
            "TC-001",
            "BIS-001",
            "Prime Supplier",
            "23HP85D",
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
            "Prime Mill",
            "TC-001",
            "BIS-001",
            "Prime Supplier",
            "23HP85D",
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
