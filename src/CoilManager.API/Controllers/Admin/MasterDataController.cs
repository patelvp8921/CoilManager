using CoilManager.Application.DTOs.Masters;
using CoilManager.Domain.Common;
using CoilManager.Persistence;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers.Admin;

public abstract class MasterDataController<TEntity>(ApplicationDbContext dbContext) : BaseApiController
    where TEntity : AuditableEntity, IMasterDataEntity
{
    protected abstract DbSet<TEntity> Masters { get; }
    protected abstract TEntity CreateEntity(CreateMasterRequest request, string code);
    protected abstract void UpdateEntity(TEntity entity, UpdateMasterRequest request, string code);
    protected abstract void SetActive(TEntity entity, bool isActive);
    protected virtual string? GetCountry(TEntity entity) => null;
    protected virtual string? GetAddress(TEntity entity) => null;
    protected virtual string? GetGST(TEntity entity) => null;
    protected virtual string? GetEmail(TEntity entity) => null;
    protected virtual string? GetContactNo(TEntity entity) => null;
    protected virtual Task<bool> IsUsedByRawCoilAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    protected virtual string GetCreateCode(CreateMasterRequest request) => request.Code?.Trim() ?? string.Empty;
    protected virtual string GetUpdateCode(TEntity entity, UpdateMasterRequest request) => request.Code?.Trim() ?? string.Empty;

    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<MasterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiPagedResponse<MasterDto>>> GetAll(
        [FromQuery] MasterQueryRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<TEntity> query = ApplyFilters(Masters.AsNoTracking(), request);
        int totalCount = await query.CountAsync(cancellationToken);

        List<TEntity> records = await ApplySorting(query, request)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        IReadOnlyList<MasterDto> items = records.Select(ToDto).ToList();

        return Paged(items, new PaginationResult(request.NormalizedPage, request.NormalizedPageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MasterDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        TEntity? entity = await Masters
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? Failure<MasterDto>(StatusCodes.Status404NotFound, "Master record was not found.")
            : Success(ToDto(entity));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MasterDto>>> Create(
        [FromBody] CreateMasterRequest request,
        CancellationToken cancellationToken)
    {
        string? validation = Validate(request.Code, request.Name);
        if (validation is not null)
        {
            return Failure<MasterDto>(StatusCodes.Status400BadRequest, validation, [validation]);
        }

        string code = GetCreateCode(request);
        if (await Masters.AnyAsync(entity => entity.Code == code, cancellationToken))
        {
            return Failure<MasterDto>(StatusCodes.Status409Conflict, $"Code '{code}' already exists.", [$"Code '{code}' already exists."]);
        }

        TEntity entity = CreateEntity(request, code);
        await Masters.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        MasterDto dto = ToDto(entity);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, ApiResponse<MasterDto>.Ok(dto, "Master record created successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MasterDto>>> Update(
        Guid id,
        [FromBody] UpdateMasterRequest request,
        CancellationToken cancellationToken)
    {
        string? validation = Validate(request.Code, request.Name);
        if (validation is not null)
        {
            return Failure<MasterDto>(StatusCodes.Status400BadRequest, validation, [validation]);
        }

        TEntity? entity = await Masters.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (entity is null)
        {
            return Failure<MasterDto>(StatusCodes.Status404NotFound, "Master record was not found.");
        }

        string code = GetUpdateCode(entity, request);
        if (await Masters.AnyAsync(candidate => candidate.Id != id && candidate.Code == code, cancellationToken))
        {
            return Failure<MasterDto>(StatusCodes.Status409Conflict, $"Code '{code}' already exists.", [$"Code '{code}' already exists."]);
        }

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
        {
            try
            {
                dbContext.Entry(entity).Property(nameof(IMasterDataEntity.RowVersion)).OriginalValue = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return Failure<MasterDto>(StatusCodes.Status400BadRequest, "Invalid row version.");
            }
        }

        if (entity.IsActive && !request.IsActive && await IsUsedByRawCoilAsync(id, cancellationToken))
        {
            return Failure<MasterDto>(
                StatusCodes.Status409Conflict,
                "This master record is used by one or more raw coils and cannot be deactivated.",
                ["This master record is used by one or more raw coils and cannot be deactivated."]);
        }

        UpdateEntity(entity, request, code);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure<MasterDto>(StatusCodes.Status409Conflict, "The master record was modified by another process. Reload and try again.");
        }

        return Success(ToDto(entity), "Master record updated successfully.");
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status404NotFound)]
    public Task<ActionResult<ApiResponse<MasterDto>>> Activate(Guid id, CancellationToken cancellationToken)
    {
        return SetStatus(id, true, cancellationToken);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status404NotFound)]
    public Task<ActionResult<ApiResponse<MasterDto>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        return SetStatus(id, false, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status404NotFound)]
    public Task<ActionResult<ApiResponse<MasterDto>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return SetStatus(id, false, cancellationToken);
    }

    private async Task<ActionResult<ApiResponse<MasterDto>>> SetStatus(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        TEntity? entity = await Masters.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (entity is null)
        {
            return Failure<MasterDto>(StatusCodes.Status404NotFound, "Master record was not found.");
        }

        if (!isActive && entity.IsActive && await IsUsedByRawCoilAsync(id, cancellationToken))
        {
            return Failure<MasterDto>(
                StatusCodes.Status409Conflict,
                "This master record is used by one or more raw coils and cannot be deactivated.",
                ["This master record is used by one or more raw coils and cannot be deactivated."]);
        }

        SetActive(entity, isActive);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Success(ToDto(entity), isActive ? "Master record activated successfully." : "Master record deactivated successfully.");
    }

    private IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, MasterQueryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            query = ApplySearch(query, search);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == request.IsActive.Value);
        }

        return query;
    }

    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string search)
    {
        return query.Where(entity =>
            entity.Code.Contains(search)
            || entity.Name.Contains(search)
            || (entity.Description != null && entity.Description.Contains(search)));
    }

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, MasterQueryRequest request)
    {
        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? string.Empty;
        return (sortBy, request.SortDescending) switch
        {
            ("name", true) => query.OrderByDescending(entity => entity.Name),
            ("name", false) => query.OrderBy(entity => entity.Name),
            ("description", true) => query.OrderByDescending(entity => entity.Description),
            ("description", false) => query.OrderBy(entity => entity.Description),
            ("status" or "isactive", true) => query.OrderByDescending(entity => entity.IsActive),
            ("status" or "isactive", false) => query.OrderBy(entity => entity.IsActive),
            ("createdon" or "createdatutc", true) => query.OrderByDescending(entity => entity.CreatedAtUtc),
            ("createdon" or "createdatutc", false) => query.OrderBy(entity => entity.CreatedAtUtc),
            ("code", true) => query.OrderByDescending(entity => entity.Code),
            _ => query.OrderBy(entity => entity.Code)
        };
    }

    protected virtual string? Validate(string? code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Code is required.";
        }

        return string.IsNullOrWhiteSpace(name) ? "Name is required." : null;
    }

    private MasterDto ToDto(TEntity entity)
    {
        return new MasterDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            GetCountry(entity),
            GetAddress(entity),
            GetGST(entity),
            GetEmail(entity),
            GetContactNo(entity),
            entity.IsActive,
            entity.CreatedAtUtc,
            entity.CreatedBy,
            entity.UpdatedAtUtc,
            entity.UpdatedBy,
            Convert.ToBase64String(entity.RowVersion));
    }
}
