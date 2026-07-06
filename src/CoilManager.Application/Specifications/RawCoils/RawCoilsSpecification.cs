using System.Linq.Expressions;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Specifications.RawCoils;

public sealed class RawCoilsSpecification : BaseSpecification<RawCoil>
{
    public RawCoilsSpecification(RawCoilQueryRequest request)
        : base(BuildCriteria(request))
    {
        AddInclude(rawCoil => rawCoil.Supplier!);
        AddInclude(rawCoil => rawCoil.Manufacturer!);
        AddInclude(rawCoil => rawCoil.Grade!);
        ApplySorting(request);

        int skip = (request.NormalizedPage - 1) * request.NormalizedPageSize;
        ApplyPaging(skip, request.NormalizedPageSize);
    }

    private void ApplySorting(RawCoilQueryRequest request)
    {
        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? string.Empty;

        Expression<Func<RawCoil, object>> keySelector = sortBy switch
        {
            "coilid" or "rawcoilnumber" => rawCoil => rawCoil.RawCoilNumber,
            "coilnumber" => rawCoil => rawCoil.CoilNumber,
            "grade" => rawCoil => rawCoil.Grade!.Code,
            "manufacturer" or "millname" => rawCoil => rawCoil.Manufacturer!.Name,
            "status" => rawCoil => rawCoil.Status,
            "receiveddate" => rawCoil => rawCoil.ReceivedDate,
            "weight" => rawCoil => rawCoil.Weight,
            _ => rawCoil => rawCoil.CreatedAtUtc
        };

        if (request.SortDescending)
        {
            ApplyOrderByDescending(keySelector);
            return;
        }

        ApplyOrderBy(keySelector);
    }

    private static Expression<Func<RawCoil, bool>> BuildCriteria(RawCoilQueryRequest request)
    {
        string? search = Normalize(request.Search);
        string? grade = Normalize(request.Grade);
        string? manufacturer = Normalize(request.Manufacturer);

        return rawCoil =>
            (search == null
                || rawCoil.CoilNumber.Contains(search)
                || rawCoil.RawCoilNumber.Contains(search)
                || rawCoil.HeatNumber.Contains(search)
                || rawCoil.Supplier!.Name.Contains(search)
                || rawCoil.Manufacturer!.Name.Contains(search)
                || rawCoil.Grade!.Code.Contains(search))
            && (grade == null || rawCoil.Grade!.Code == grade)
            && (manufacturer == null || rawCoil.Manufacturer!.Name == manufacturer)
            && (!request.Status.HasValue || rawCoil.Status == request.Status.Value);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
