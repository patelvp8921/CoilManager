using CoilManager.Application.DTOs.Dashboard;

namespace CoilManager.Application.Interfaces.Services;

public interface IOperationsDashboardService
{
    Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default);
}
