using CoilManager.Application.DTOs.Coils;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Services;

public interface ICoilService
{
    Task<Result<CoilSearchResultDto>> SearchAsync(string value, CancellationToken cancellationToken = default);
    Task<Result<CoilTraceabilityDto>> GetTraceabilityAsync(string coilNumber, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryTransactionDto>>> GetInventoryTransactionsAsync(string coilNumber, CancellationToken cancellationToken = default);
}
