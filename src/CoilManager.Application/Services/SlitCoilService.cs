using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Services;

public sealed class SlitCoilService(ISlitCoilRepository slitCoilRepository) : ISlitCoilService
{
    public Task<PagedResult<SlitCoilListItemDto>> GetAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default) =>
        slitCoilRepository.GetPagedAsync(request, cancellationToken);

    public async Task<Result<SlitCoilDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return coil is null
            ? Result<SlitCoilDetailsDto>.Failure(Error.NotFound("Slit Coil was not found."))
            : Result<SlitCoilDetailsDto>.Success(await MapDetailsAsync(coil, cancellationToken));
    }

    public async Task<Result<SlitCoilDetailsDto>> GetByNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        string value = coilNumber.Trim();
        SlitCoil? coil = await slitCoilRepository.GetByNumberWithDetailsAsync(value, cancellationToken);
        return coil is null
            ? Result<SlitCoilDetailsDto>.Failure(Error.NotFound($"Slit Coil '{value}' was not found."))
            : Result<SlitCoilDetailsDto>.Success(await MapDetailsAsync(coil, cancellationToken));
    }

    public async Task<Result<SlitCoilGenealogyDto>> GetGenealogyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (coil is null) return Result<SlitCoilGenealogyDto>.Failure(Error.NotFound("Slit Coil was not found."));
        SlitCoilDetailsDto details = await MapDetailsAsync(coil, cancellationToken);
        return Result<SlitCoilGenealogyDto>.Success(new(coil.Id, coil.CoilNumber, details.ParentCoilNumber,
            details.RootMotherCoilNumber, details.MotherCoilNumber, details.SlittingJobNo,
            coil.SlitSequence, coil.GenerationLevel));
    }

    private async Task<SlitCoilDetailsDto> MapDetailsAsync(SlitCoil coil, CancellationToken cancellationToken)
    {
        IReadOnlyList<SlitCoil> all = await slitCoilRepository.GetAllWithDetailsAsync(cancellationToken);
        string motherNumber = coil.MotherCoil?.RawCoilNumber ?? "-";
        string parentNumber = all.FirstOrDefault(item => item.Id == coil.ParentCoilId)?.CoilNumber ?? motherNumber;
        string rootNumber = all.FirstOrDefault(item => item.Id == coil.RootMotherCoilId)?.MotherCoil?.RawCoilNumber ?? motherNumber;
        return new(coil.Id, coil.CoilNumber, coil.Status, coil.GenerationLevel, coil.SlitSequence,
            coil.LabelVersion, coil.BarcodeValue, coil.QrCodeValue, coil.CreatedAtUtc, coil.CreatedBy,
            coil.UpdatedAtUtc, coil.UpdatedBy, coil.Grade?.Code, coil.Thickness, coil.Category,
            coil.CoreLossPerKg, coil.Width, coil.Weight, coil.HeatNumber, coil.MotherCoil?.Supplier?.Name,
            coil.MotherCoil?.Manufacturer?.Name, coil.WarehouseLocation, coil.ParentCoilId, parentNumber,
            coil.RootMotherCoilId, rootNumber, coil.MotherCoilId, motherNumber, coil.SlittingJobId,
            coil.SlittingJob?.SlittingJobNo ?? "-", true, coil.ParentCoilId != coil.MotherCoilId,
            true, false);
    }
}
