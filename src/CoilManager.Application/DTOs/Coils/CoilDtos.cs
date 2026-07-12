using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.Coils;

public sealed record CoilSearchResultDto(
    CoilType CoilType, Guid Id, string CoilNumber, CoilStatus Status, string? Grade,
    decimal Thickness, decimal Width, decimal Weight, string? ParentCoilNumber,
    string RootMotherCoilNumber, string? SlittingJobNo, string NavigationRoute);

public sealed record InventoryTransactionDto(
    Guid Id, InventoryTransactionType TransactionType, CoilType CoilType, string CoilNumber,
    string? RelatedDocumentNumber, CoilStatus? FromStatus, CoilStatus ToStatus,
    decimal QuantityWeight, DateTimeOffset TransactionDate, string? Remarks, string? CreatedBy);

public sealed record TraceabilityCoilNodeDto(
    Guid Id, string CoilNumber, CoilType CoilType, CoilStatus Status, int GenerationLevel,
    string? ParentCoilNumber, string RootMotherCoilNumber, string? Grade, decimal Thickness,
    decimal Width, decimal Weight, DateTimeOffset CreatedOn, string? SlittingJobNo,
    IReadOnlyList<TraceabilityCoilNodeDto> Children);

public sealed record TraceabilitySlittingJobDto(
    Guid Id, string SlittingJobNo, SlittingJobStatus Status, DateOnly PlanningDate,
    DateTimeOffset? ReleasedOn, DateTimeOffset? StartedOn, DateTimeOffset? CompletedOn);

public sealed record CoilTraceabilityDto(
    TraceabilityCoilNodeDto CurrentCoil, TraceabilityCoilNodeDto RootMotherCoil,
    IReadOnlyList<TraceabilityCoilNodeDto> ParentChain,
    IReadOnlyList<TraceabilityCoilNodeDto> DirectChildren,
    IReadOnlyList<TraceabilityCoilNodeDto> Descendants,
    IReadOnlyList<TraceabilitySlittingJobDto> RelatedSlittingJobs,
    IReadOnlyList<InventoryTransactionDto> InventoryTransactions);
