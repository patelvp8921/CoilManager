using System.Text.RegularExpressions;
using AutoMapper;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Specifications.RawCoils;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;
using FluentValidation;

namespace CoilManager.Application.Services;

public sealed partial class RawCoilService(
    IRawCoilRepository rawCoilRepository,
    IRepository<Supplier> supplierRepository,
    IRepository<Manufacturer> manufacturerRepository,
    IRepository<Grade> gradeRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<CreateRawCoilRequest> createValidator,
    IValidator<UpdateRawCoilRequest> updateValidator) : IRawCoilService
{
    private const decimal DefaultWidth = 1250m;

    public async Task<PagedResult<RawCoilDto>> GetAsync(RawCoilQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        RawCoilsSpecification specification = new(request);
        RawCoilsCountSpecification countSpecification = new(request);

        IReadOnlyList<RawCoil> rawCoils = await rawCoilRepository.GetAllAsync(specification, cancellationToken);
        int totalCount = await rawCoilRepository.CountAsync(countSpecification, cancellationToken);

        IReadOnlyList<RawCoilDto> items = rawCoils.Count == 0
            ? []
            : rawCoils.Select(mapper.Map<RawCoilDto>).ToList();

        return new PagedResult<RawCoilDto>(
            items,
            request.NormalizedPage,
            request.NormalizedPageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<RawCoilDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RawCoil> rawCoils = await rawCoilRepository.GetAllAsync(cancellationToken);

        return rawCoils.Count == 0
            ? []
            : rawCoils.Select(mapper.Map<RawCoilDto>).ToList();
    }

    public async Task<Result<RawCoilDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RawCoil? rawCoil = await rawCoilRepository.GetByIdAsync(id, cancellationToken);
        if (rawCoil is null)
        {
            return Result<RawCoilDto>.Failure(Error.NotFound($"Raw coil '{id}' was not found."));
        }

        return Result<RawCoilDto>.Success(mapper.Map<RawCoilDto>(rawCoil));
    }

    public Task<string> GetNextRawCoilNumberAsync(CancellationToken cancellationToken = default)
    {
        return BuildNextRawCoilNumberAsync(cancellationToken);
    }

    public async Task<Result<RawCoilDto>> CreateAsync(CreateRawCoilRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<RawCoilDto>.Failure(Error.Validation(string.Join("; ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        if (await rawCoilRepository.ExistsByCoilNumberAsync(request.CoilNumber, cancellationToken: cancellationToken))
        {
            return Result<RawCoilDto>.Failure(Error.Conflict($"Raw coil number '{request.CoilNumber}' already exists."));
        }

        Supplier? supplier = supplierRepository.Query().FirstOrDefault(supplier => supplier.Id == request.SupplierId && supplier.IsActive);
        if (supplier is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Supplier is required and must be active."));
        }

        Manufacturer? manufacturer = manufacturerRepository.Query().FirstOrDefault(manufacturer => manufacturer.Id == request.ManufacturerId && manufacturer.IsActive);
        if (manufacturer is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Manufacturer is required and must be active."));
        }

        Grade? grade = gradeRepository.Query().FirstOrDefault(grade => grade.Id == request.GradeId && grade.IsActive);
        if (grade is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Grade is required and must be active."));
        }

        Result<GradeProperties> gradePropertiesResult = ResolveGradeProperties(grade.Code, request.Thickness, request.WattLossPerKg);
        if (gradePropertiesResult.IsFailure)
        {
            return Result<RawCoilDto>.Failure(gradePropertiesResult.Error);
        }

        GradeProperties gradeProperties = gradePropertiesResult.Value;
        string rawCoilNumber = await BuildNextRawCoilNumberAsync(cancellationToken);

        RawCoil rawCoil = new(
            rawCoilNumber,
            request.CoilNumber.Trim(),
            request.HeatNumber.Trim(),
            Normalize(request.PONumber),
            Normalize(request.InvoiceNo),
            Normalize(request.MillTCNo),
            Normalize(request.BISLicNumber),
            request.SupplierId,
            request.ManufacturerId,
            request.GradeId,
            gradeProperties.Thickness,
            request.Width ?? DefaultWidth,
            request.Weight,
            request.Length,
            gradeProperties.WattLossPerKg,
            Normalize(request.WarehouseLocation),
            request.ReceivedDate,
            request.Status ?? CoilStatus.Available);
        rawCoil.SetLookupReferences(supplier, manufacturer, grade);

        await rawCoilRepository.AddAsync(rawCoil, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RawCoilDto>.Success(mapper.Map<RawCoilDto>(rawCoil));
    }

    public async Task<Result<RawCoilDto>> UpdateAsync(Guid id, UpdateRawCoilRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<RawCoilDto>.Failure(Error.Validation(string.Join("; ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        RawCoil? rawCoil = await rawCoilRepository.GetByIdAsync(id, cancellationToken);
        if (rawCoil is null)
        {
            return Result<RawCoilDto>.Failure(Error.NotFound($"Raw coil '{id}' was not found."));
        }

        if (await rawCoilRepository.ExistsByCoilNumberAsync(request.CoilNumber, id, cancellationToken))
        {
            return Result<RawCoilDto>.Failure(Error.Conflict($"Raw coil number '{request.CoilNumber}' already exists."));
        }

        if (!RowVersionMatches(rawCoil.RowVersion, request.RowVersion))
        {
            return Result<RawCoilDto>.Failure(Error.Conflict("The raw coil was modified by another process. Reload and try again."));
        }

        Supplier? supplier = supplierRepository.Query().FirstOrDefault(supplier => supplier.Id == request.SupplierId && supplier.IsActive);
        if (supplier is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Supplier is required and must be active."));
        }

        Manufacturer? manufacturer = manufacturerRepository.Query().FirstOrDefault(manufacturer => manufacturer.Id == request.ManufacturerId && manufacturer.IsActive);
        if (manufacturer is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Manufacturer is required and must be active."));
        }

        Grade? grade = gradeRepository.Query().FirstOrDefault(grade => grade.Id == request.GradeId && grade.IsActive);
        if (grade is null)
        {
            return Result<RawCoilDto>.Failure(Error.Validation("Grade is required and must be active."));
        }

        Result<GradeProperties> gradePropertiesResult = ResolveGradeProperties(grade.Code, request.Thickness, request.WattLossPerKg);
        if (gradePropertiesResult.IsFailure)
        {
            return Result<RawCoilDto>.Failure(gradePropertiesResult.Error);
        }

        GradeProperties gradeProperties = gradePropertiesResult.Value;

        rawCoil.Update(
            request.CoilNumber.Trim(),
            request.HeatNumber.Trim(),
            Normalize(request.PONumber),
            Normalize(request.InvoiceNo),
            Normalize(request.MillTCNo),
            Normalize(request.BISLicNumber),
            request.SupplierId,
            request.ManufacturerId,
            request.GradeId,
            gradeProperties.Thickness,
            request.Width ?? DefaultWidth,
            request.Weight,
            request.Length,
            gradeProperties.WattLossPerKg,
            Normalize(request.WarehouseLocation),
            request.Status,
            request.ReceivedDate);
        rawCoil.SetLookupReferences(supplier, manufacturer, grade);

        rawCoilRepository.Update(rawCoil);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RawCoilDto>.Success(mapper.Map<RawCoilDto>(rawCoil));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RawCoil? rawCoil = await rawCoilRepository.GetByIdAsync(id, cancellationToken);
        if (rawCoil is null)
        {
            return Result.Failure(Error.NotFound($"Raw coil '{id}' was not found."));
        }

        rawCoilRepository.Delete(rawCoil);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Result<GradeProperties> ResolveGradeProperties(string grade, decimal? requestedThickness, decimal? requestedWattLoss)
    {
        Match match = GradePattern().Match(grade);
        if (match.Success
            && decimal.TryParse(match.Groups["thickness"].Value, out decimal gradeThickness)
            && decimal.TryParse(match.Groups["loss"].Value, out decimal gradeLoss))
        {
            return Result<GradeProperties>.Success(new GradeProperties(gradeThickness / 100m, gradeLoss / 100m));
        }

        if (requestedThickness.HasValue && requestedWattLoss.HasValue)
        {
            return Result<GradeProperties>.Success(new GradeProperties(requestedThickness.Value, requestedWattLoss.Value));
        }

        return Result<GradeProperties>.Failure(Error.Validation("Grade must contain derivable thickness and watt loss, or both values must be provided."));
    }

    private static bool RowVersionMatches(byte[] currentRowVersion, string requestRowVersion)
    {
        try
        {
            byte[] decoded = Convert.FromBase64String(requestRowVersion);
            return currentRowVersion.SequenceEqual(decoded);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<string> BuildNextRawCoilNumberAsync(CancellationToken cancellationToken)
    {
        int currentYear = DateTime.UtcNow.Year;
        int nextSequence = await rawCoilRepository.CountByRawCoilYearAsync(currentYear, cancellationToken) + 1;
        string rawCoilNumber;

        do
        {
            rawCoilNumber = RawCoilNumberGenerator.Generate(currentYear, nextSequence);
            nextSequence++;
        }
        while (await rawCoilRepository.ExistsByRawCoilNumberAsync(rawCoilNumber, cancellationToken));

        return rawCoilNumber;
    }

    [GeneratedRegex(@"^(?<thickness>\d{2}).*?(?<loss>\d{2})[A-Za-z]?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GradePattern();

    private sealed record GradeProperties(decimal Thickness, decimal WattLossPerKg);
}
