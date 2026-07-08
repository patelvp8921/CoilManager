using CoilManager.Application.DTOs.Masters;
using CoilManager.Domain.Entities;
using CoilManager.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers.Admin;

[Route("api/admin/grades")]
public sealed class GradesController : MasterDataController<Grade>
{
    private readonly ApplicationDbContext _dbContext;

    public GradesController(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    protected override DbSet<Grade> Masters => _dbContext.Grades;
    protected override Grade CreateEntity(CreateMasterRequest request, string code) => new(code, request.ThicknessMm!.Value, request.CoreLossPerKg!.Value, request.IsActive);
    protected override void UpdateEntity(Grade entity, UpdateMasterRequest request, string code) => entity.Update(code, request.ThicknessMm!.Value, request.CoreLossPerKg!.Value, request.IsActive);
    protected override void SetActive(Grade entity, bool isActive) => entity.SetActive(isActive);
    protected override string GetCreateCode(CreateMasterRequest request) => (request.Grade ?? request.Code)?.Trim() ?? string.Empty;
    protected override string GetUpdateCode(Grade entity, UpdateMasterRequest request) => (request.Grade ?? request.Code)?.Trim() ?? string.Empty;
    protected override string DuplicateCodeMessage(string code) => $"Grade '{code}' already exists.";
    protected override string? GetGrade(Grade entity) => entity.Code;
    protected override decimal? GetThicknessMm(Grade entity) => entity.ThicknessMm;
    protected override string? GetCategory(Grade entity) => entity.Category;
    protected override decimal? GetCoreLossPerKg(Grade entity) => entity.CoreLossPerKg;

    protected override string? Validate(string? code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Grade is required.";
        }

        return null;
    }

    protected override IQueryable<Grade> ApplySearch(IQueryable<Grade> query, string search)
    {
        return query.Where(grade =>
            grade.Code.Contains(search)
            || grade.Category.Contains(search));
    }

    protected override IQueryable<Grade> ApplySorting(IQueryable<Grade> query, CoilManager.Application.DTOs.Masters.MasterQueryRequest request)
    {
        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? string.Empty;
        return (sortBy, request.SortDescending) switch
        {
            ("grade" or "code", true) => query.OrderByDescending(grade => grade.Code),
            ("thicknessmm" or "thickness", true) => query.OrderByDescending(grade => grade.ThicknessMm),
            ("thicknessmm" or "thickness", false) => query.OrderBy(grade => grade.ThicknessMm),
            ("category", true) => query.OrderByDescending(grade => grade.Category),
            ("category", false) => query.OrderBy(grade => grade.Category),
            ("corelossperkg" or "coreloss", true) => query.OrderByDescending(grade => grade.CoreLossPerKg),
            ("corelossperkg" or "coreloss", false) => query.OrderBy(grade => grade.CoreLossPerKg),
            ("status" or "isactive", true) => query.OrderByDescending(grade => grade.IsActive),
            ("status" or "isactive", false) => query.OrderBy(grade => grade.IsActive),
            _ => query.OrderBy(grade => grade.Code)
        };
    }

    protected override string? Validate(CreateMasterRequest request)
    {
        return ValidateGradeRequest((request.Grade ?? request.Code)?.Trim(), request.ThicknessMm, request.CoreLossPerKg);
    }

    protected override string? Validate(UpdateMasterRequest request)
    {
        return ValidateGradeRequest((request.Grade ?? request.Code)?.Trim(), request.ThicknessMm, request.CoreLossPerKg);
    }

    protected override Task<bool> IsUsedByRawCoilAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.RawCoils.AnyAsync(rawCoil => rawCoil.GradeId == id, cancellationToken);
    }

    private static string? ValidateGradeRequest(string? grade, decimal? thicknessMm, decimal? coreLossPerKg)
    {
        if (string.IsNullOrWhiteSpace(grade))
        {
            return "Grade is required.";
        }

        if (!thicknessMm.HasValue)
        {
            return "Thickness(mm) is required.";
        }

        if (!Grade.IsSupportedThickness(thicknessMm.Value))
        {
            return "Thickness(mm) must be one of: 0.23, 0.27, 0.30, 0.35.";
        }

        if (!coreLossPerKg.HasValue || coreLossPerKg.Value <= 0)
        {
            return "Core Loss/Kg is required and must be greater than 0.";
        }

        return null;
    }
}
