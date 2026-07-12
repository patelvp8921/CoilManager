using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Services;

public interface ISlitCoilLabelService
{
    Task<Result<SlitCoilLabelDto>> GetLabelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PrintSlitCoilLabelResultDto>> PrintAsync(Guid id, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlitCoilLabelDto>> IncrementVersionAsync(Guid id, IncrementLabelVersionRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LabelPrintHistoryDto>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BatchPrintSlitCoilLabelsResultDto> BatchPrintAsync(BatchPrintSlitCoilLabelsRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SlitCoilLabelDto>>> GetJobLabelsAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<Result<BatchPrintSlitCoilLabelsResultDto>> PrintJobLabelsAsync(Guid jobId, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken = default);
}
