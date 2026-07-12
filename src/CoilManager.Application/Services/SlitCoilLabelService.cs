using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Settings;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Results;
using Microsoft.Extensions.Options;

namespace CoilManager.Application.Services;

public sealed class SlitCoilLabelService(
    ISlitCoilRepository slitCoilRepository,
    ISlitCoilLabelPrintHistoryRepository historyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IOptions<LabelSettings> settings) : ISlitCoilLabelService
{
    private readonly LabelSettings _settings = settings.Value;

    public async Task<Result<SlitCoilLabelDto>> GetLabelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return coil is null ? Result<SlitCoilLabelDto>.Failure(Error.NotFound("Slit Coil was not found."))
            : string.IsNullOrWhiteSpace(coil.CoilNumber) ? Result<SlitCoilLabelDto>.Failure(Error.Validation("Coil Number is required."))
            : Result<SlitCoilLabelDto>.Success(MapLabel(coil));
    }

    public async Task<Result<PrintSlitCoilLabelResultDto>> PrintAsync(Guid id, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken = default) =>
        await PrintInternalAsync(id, request, null, cancellationToken);

    public async Task<Result<SlitCoilLabelDto>> IncrementVersionAsync(Guid id, IncrementLabelVersionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return Result<SlitCoilLabelDto>.Failure(Error.Validation("A reason is required to increment Label Version."));
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (coil is null) return Result<SlitCoilLabelDto>.Failure(Error.NotFound("Slit Coil was not found."));
        coil.IncrementLabelVersion();
        slitCoilRepository.Update(coil);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SlitCoilLabelDto>.Success(MapLabel(coil));
    }

    public async Task<Result<IReadOnlyList<LabelPrintHistoryDto>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (coil is null) return Result<IReadOnlyList<LabelPrintHistoryDto>>.Failure(Error.NotFound("Slit Coil was not found."));
        IReadOnlyList<SlitCoilLabelPrintHistory> rows = await historyRepository.GetBySlitCoilIdAsync(id, cancellationToken);
        return Result<IReadOnlyList<LabelPrintHistoryDto>>.Success(rows.Select(row => new LabelPrintHistoryDto(
            row.PrintedOn, row.PrintedBy, row.Copies, row.LabelVersion, row.PrinterName, row.PrintType, row.Remarks)).ToArray());
    }

    public async Task<BatchPrintSlitCoilLabelsResultDto> BatchPrintAsync(BatchPrintSlitCoilLabelsRequest request, CancellationToken cancellationToken = default)
    {
        Guid[] ids = request.SlitCoilIds.Distinct().ToArray();
        var labels = new List<PrintSlitCoilLabelResultDto>();
        var failures = new List<BatchPrintFailureDto>();
        foreach (Guid id in ids)
        {
            Result<PrintSlitCoilLabelResultDto> result = await PrintInternalAsync(id,
                new(request.CopiesPerLabel, request.PrinterName, request.Remarks), LabelPrintType.BatchPrint, cancellationToken);
            if (result.IsSuccess) labels.Add(result.Value); else failures.Add(new(id, result.Error.Message));
        }
        return new(ids.Length, labels.Count, failures, labels);
    }

    public async Task<Result<IReadOnlyList<SlitCoilLabelDto>>> GetJobLabelsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SlitCoil> coils = await slitCoilRepository.GetBySlittingJobIdAsync(jobId, cancellationToken);
        return coils.Count == 0
            ? Result<IReadOnlyList<SlitCoilLabelDto>>.Failure(Error.NotFound("No generated Slit Coil labels were found for this Slitting Job."))
            : Result<IReadOnlyList<SlitCoilLabelDto>>.Success(coils.Select(MapLabel).ToArray());
    }

    public async Task<Result<BatchPrintSlitCoilLabelsResultDto>> PrintJobLabelsAsync(Guid jobId, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SlitCoil> coils = await slitCoilRepository.GetBySlittingJobIdAsync(jobId, cancellationToken);
        if (coils.Count == 0) return Result<BatchPrintSlitCoilLabelsResultDto>.Failure(Error.NotFound("No generated Slit Coils were found for this Slitting Job."));
        return Result<BatchPrintSlitCoilLabelsResultDto>.Success(await BatchPrintAsync(
            new(coils.Select(coil => coil.Id).ToArray(), request.Copies, request.PrinterName, request.Remarks), cancellationToken));
    }

    private async Task<Result<PrintSlitCoilLabelResultDto>> PrintInternalAsync(Guid id, PrintSlitCoilLabelRequest request,
        LabelPrintType? forcedType, CancellationToken cancellationToken)
    {
        if (request.Copies is < 1 or > 100) return Result<PrintSlitCoilLabelResultDto>.Failure(Error.Validation("Copies must be between 1 and 100."));
        SlitCoil? coil = await slitCoilRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (coil is null) return Result<PrintSlitCoilLabelResultDto>.Failure(Error.NotFound("Slit Coil was not found."));
        if (string.IsNullOrWhiteSpace(coil.CoilNumber)) return Result<PrintSlitCoilLabelResultDto>.Failure(Error.Validation("Coil Number is required."));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string printedBy = currentUser.UserName ?? currentUser.UserId ?? "System";
        LabelPrintType printType = forcedType ?? (coil.LabelPrinted ? LabelPrintType.Reprint : LabelPrintType.Initial);
        coil.RecordLabelPrint(now, printedBy);
        var history = new SlitCoilLabelPrintHistory(coil.Id, coil.CoilNumber, coil.LabelVersion, printedBy,
            now, request.Copies, request.PrinterName, printType, request.Remarks);
        slitCoilRepository.Update(coil);
        await historyRepository.AddAsync(history, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PrintSlitCoilLabelResultDto>.Success(new(coil.Id, coil.CoilNumber, coil.LabelVersion,
            coil.LabelPrintCount, now, printedBy, request.Copies, printType));
    }

    private SlitCoilLabelDto MapLabel(SlitCoil coil)
    {
        string mother = coil.MotherCoil?.RawCoilNumber ?? "-";
        return new(coil.Id, coil.CoilNumber, mother, coil.SlittingJob?.SlittingJobNo ?? "-",
            coil.Grade?.Code, coil.Thickness, coil.Category, coil.CoreLossPerKg, coil.Width, coil.Weight,
            coil.MotherCoil?.Supplier?.Name ?? coil.Supplier?.Name,
            coil.MotherCoil?.Manufacturer?.Name ?? coil.Manufacturer?.Name, coil.HeatNumber,
            coil.CoilNumber, coil.CoilNumber, coil.LabelVersion, coil.LabelPrinted, coil.LabelPrintCount,
            coil.LabelLastPrintedOn, coil.LabelLastPrintedBy, _settings.CompanyName, _settings.CompanyAddress,
            _settings.CompanyLogoUrl, _settings.WidthMm, _settings.HeightMm);
    }
}
