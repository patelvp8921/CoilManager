using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Mappings;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Services;

public sealed class SlitCoilService(ISlitCoilRepository slitCoilRepository) : ISlitCoilService
{
    public Task<PagedResult<SlitCoilDto>> GetAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default)
    {
        return slitCoilRepository.GetPagedAsync(request, cancellationToken);
    }

    public async Task<Result<SlitCoilDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return coil is null
            ? Result<SlitCoilDto>.Failure(Error.NotFound($"Slit coil '{id}' was not found."))
            : Result<SlitCoilDto>.Success(SlitCoilDtoMapper.MapToDto(coil));
    }

    public async Task<Result<SlitCoilDto>> GetByNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByNumberWithDetailsAsync(coilNumber, cancellationToken);
        return coil is null
            ? Result<SlitCoilDto>.Failure(Error.NotFound($"Slit coil '{coilNumber}' was not found."))
            : Result<SlitCoilDto>.Success(SlitCoilDtoMapper.MapToDto(coil));
    }

    public async Task<Result<SlitCoilGenealogyDto>> GetGenealogyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (coil is null)
        {
            return Result<SlitCoilGenealogyDto>.Failure(Error.NotFound($"Slit coil '{id}' was not found."));
        }

        string motherCoilNumber = coil.MotherCoil?.RawCoilNumber ?? "-";
        string slittingJobNo = coil.SlittingJob?.SlittingJobNo ?? "-";
        return Result<SlitCoilGenealogyDto>.Success(new(
            coil.Id,
            coil.CoilNumber,
            motherCoilNumber,
            motherCoilNumber,
            motherCoilNumber,
            slittingJobNo,
            coil.SlitSequence,
            coil.GenerationLevel));
    }
}
