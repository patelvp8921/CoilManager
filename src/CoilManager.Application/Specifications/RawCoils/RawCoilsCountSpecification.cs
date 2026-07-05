using System.Linq.Expressions;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Specifications.RawCoils;

public sealed class RawCoilsCountSpecification : BaseSpecification<RawCoil>
{
    public RawCoilsCountSpecification(RawCoilQueryRequest request)
        : base(BuildCriteria(request))
    {
    }

    private static Expression<Func<RawCoil, bool>> BuildCriteria(RawCoilQueryRequest request)
    {
        string? search = Normalize(request.Search);
        string? grade = Normalize(request.Grade);
        string? manufacturer = Normalize(request.Manufacturer);

        return rawCoil =>
            (search == null
                || rawCoil.CoilNumber.Contains(search)
                || rawCoil.HeatNumber.Contains(search)
                || rawCoil.SupplierName.Contains(search)
                || rawCoil.MillName.Contains(search)
                || rawCoil.Grade.Contains(search))
            && (grade == null || rawCoil.Grade == grade)
            && (manufacturer == null || rawCoil.MillName == manufacturer)
            && (!request.Status.HasValue || rawCoil.Status == request.Status.Value);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
