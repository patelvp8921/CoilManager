using CoilManager.Application.DTOs.Coils;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Services;

public sealed class CoilService(
    IRawCoilRepository rawCoilRepository,
    ISlitCoilRepository slitCoilRepository,
    IInventoryTransactionRepository inventoryTransactionRepository) : ICoilService
{
    public async Task<Result<CoilSearchResultDto>> SearchAsync(string value, CancellationToken cancellationToken = default)
    {
        string search = Normalize(value);
        if (search.Length is 0 or > 100)
            return Result<CoilSearchResultDto>.Failure(Error.Validation("Enter a valid Coil Number, Barcode, or QR Code value."));

        IReadOnlyList<RawCoil> mothers = await rawCoilRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<SlitCoil> slits = await slitCoilRepository.GetAllWithDetailsAsync(cancellationToken);
        RawCoil[] motherMatches = mothers.Where(coil => EqualsValue(coil.RawCoilNumber, search) || EqualsValue(coil.CoilNumber, search)).ToArray();
        SlitCoil[] slitMatches = slits.Where(coil => EqualsValue(coil.CoilNumber, search) || EqualsValue(coil.BarcodeValue, search) || EqualsValue(coil.QrCodeValue, search)).ToArray();
        if (motherMatches.Length + slitMatches.Length > 1)
            return Result<CoilSearchResultDto>.Failure(Error.Conflict("More than one coil matched this business identifier."));
        if (motherMatches.Length == 1)
        {
            RawCoil coil = motherMatches[0];
            return Result<CoilSearchResultDto>.Success(new(CoilType.MotherCoil, coil.Id, coil.RawCoilNumber,
                coil.Status, coil.Grade?.Code, coil.Thickness, coil.Width, coil.Weight, null,
                coil.RawCoilNumber, null, $"/mother-coils/{coil.Id}/details"));
        }
        if (slitMatches.Length == 1)
        {
            SlitCoil coil = slitMatches[0];
            string mother = coil.MotherCoil?.RawCoilNumber ?? "-";
            string parent = slits.FirstOrDefault(item => item.Id == coil.ParentCoilId)?.CoilNumber ?? mother;
            return Result<CoilSearchResultDto>.Success(new(CoilType.SlitCoil, coil.Id, coil.CoilNumber,
                coil.Status, coil.Grade?.Code, coil.Thickness, coil.Width, coil.Weight, parent,
                mother, coil.SlittingJob?.SlittingJobNo, $"/slit-coils/{coil.Id}"));
        }
        return Result<CoilSearchResultDto>.Failure(Error.NotFound($"No coil was found for '{search}'."));
    }

    public async Task<Result<IReadOnlyList<InventoryTransactionDto>>> GetInventoryTransactionsAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        string value = Normalize(coilNumber);
        Result<CoilSearchResultDto> coil = await SearchAsync(value, cancellationToken);
        if (!coil.IsSuccess) return Result<IReadOnlyList<InventoryTransactionDto>>.Failure(coil.Error);
        IReadOnlyList<InventoryTransaction> rows = await inventoryTransactionRepository.GetByCoilNumberAsync(coil.Value.CoilNumber, cancellationToken);
        return Result<IReadOnlyList<InventoryTransactionDto>>.Success(rows.Select(MapTransaction).ToArray());
    }

    public async Task<Result<CoilTraceabilityDto>> GetTraceabilityAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        Result<CoilSearchResultDto> search = await SearchAsync(coilNumber, cancellationToken);
        if (!search.IsSuccess) return Result<CoilTraceabilityDto>.Failure(search.Error);
        IReadOnlyList<RawCoil> mothers = await rawCoilRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<SlitCoil> slits = await slitCoilRepository.GetAllWithDetailsAsync(cancellationToken);
        CoilSearchResultDto current = search.Value;
        RawCoil? root = current.CoilType == CoilType.MotherCoil
            ? mothers.FirstOrDefault(item => item.Id == current.Id)
            : mothers.FirstOrDefault(item => item.Id == slits.First(item => item.Id == current.Id).RootMotherCoilId)
                ?? mothers.FirstOrDefault(item => item.Id == slits.First(item => item.Id == current.Id).MotherCoilId);
        if (root is null) return Result<CoilTraceabilityDto>.Failure(Error.Validation("The root Mother Coil could not be resolved."));

        var visited = new HashSet<Guid>();
        TraceabilityCoilNodeDto BuildSlit(SlitCoil coil)
        {
            if (!visited.Add(coil.Id)) throw new InvalidOperationException("Circular coil genealogy detected.");
            string parent = slits.FirstOrDefault(item => item.Id == coil.ParentCoilId)?.CoilNumber ?? root.RawCoilNumber;
            var children = slits.Where(item => item.ParentCoilId == coil.Id).OrderBy(item => item.SlitSequence).Select(BuildSlit).ToArray();
            visited.Remove(coil.Id);
            return new(coil.Id, coil.CoilNumber, CoilType.SlitCoil, coil.Status, coil.GenerationLevel,
                parent, root.RawCoilNumber, coil.Grade?.Code, coil.Thickness, coil.Width, coil.Weight,
                coil.CreatedAtUtc, coil.SlittingJob?.SlittingJobNo, children);
        }

        try
        {
            TraceabilityCoilNodeDto rootNode = new(root.Id, root.RawCoilNumber, CoilType.MotherCoil,
                root.Status, 0, null, root.RawCoilNumber, root.Grade?.Code, root.Thickness, root.Width,
                root.Weight, root.CreatedAtUtc, null,
                slits.Where(item => item.RootMotherCoilId == root.Id && item.ParentCoilId == root.Id)
                    .OrderBy(item => item.SlitSequence).Select(BuildSlit).ToArray());
            TraceabilityCoilNodeDto currentNode = current.CoilType == CoilType.MotherCoil
                ? rootNode : FindNode(rootNode, current.Id)!;
            if (currentNode is null) return Result<CoilTraceabilityDto>.Failure(Error.Validation("The current coil is outside its root genealogy."));
            IReadOnlyList<TraceabilityCoilNodeDto> chain = BuildParentChain(rootNode, current.Id);
            TraceabilityCoilNodeDto[] descendants = Flatten(currentNode.Children).ToArray();
            IReadOnlyList<InventoryTransaction> transactions = await inventoryTransactionRepository.GetByCoilNumberAsync(current.CoilNumber, cancellationToken);
            var jobs = slits.Where(item => item.RootMotherCoilId == root.Id).Select(item => item.SlittingJob)
                .Where(job => job is not null).DistinctBy(job => job!.Id)
                .Select(job => new TraceabilitySlittingJobDto(job!.Id, job.SlittingJobNo, job.Status,
                    job.PlanningDate, job.ReleasedOn, job.StartedOn, job.CompletedOn)).ToArray();
            return Result<CoilTraceabilityDto>.Success(new(currentNode, rootNode, chain,
                currentNode.Children, descendants, jobs, transactions.Select(MapTransaction).ToArray()));
        }
        catch (InvalidOperationException)
        {
            return Result<CoilTraceabilityDto>.Failure(Error.Validation("Circular or malformed coil genealogy was detected."));
        }
    }

    private static TraceabilityCoilNodeDto? FindNode(TraceabilityCoilNodeDto node, Guid id) =>
        node.Id == id ? node : node.Children.Select(child => FindNode(child, id)).FirstOrDefault(found => found is not null);

    private static IReadOnlyList<TraceabilityCoilNodeDto> BuildParentChain(TraceabilityCoilNodeDto root, Guid currentId)
    {
        var path = new List<TraceabilityCoilNodeDto>();
        bool Walk(TraceabilityCoilNodeDto node)
        {
            if (node.Id == currentId) return true;
            path.Add(node);
            foreach (TraceabilityCoilNodeDto child in node.Children) if (Walk(child)) return true;
            path.RemoveAt(path.Count - 1);
            return false;
        }
        Walk(root);
        return path;
    }

    private static IEnumerable<TraceabilityCoilNodeDto> Flatten(IEnumerable<TraceabilityCoilNodeDto> nodes) =>
        nodes.SelectMany(node => new[] { node }.Concat(Flatten(node.Children)));

    private static InventoryTransactionDto MapTransaction(InventoryTransaction row) => new(row.Id,
        row.TransactionType, row.CoilType, row.CoilNumber, row.RelatedDocumentNumber, row.FromStatus,
        row.ToStatus, row.QuantityWeight, row.TransactionDate, row.Remarks, row.CreatedBy);
    private static string Normalize(string value) => Uri.UnescapeDataString(value ?? string.Empty).Trim();
    private static bool EqualsValue(string? left, string right) => string.Equals(left?.Trim(), right, StringComparison.OrdinalIgnoreCase);
}
