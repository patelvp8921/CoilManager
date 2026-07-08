using CoilManager.Application.DTOs.Lookups;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Services;

public sealed class LookupService(
    IRepository<Supplier> supplierRepository,
    IRepository<Manufacturer> manufacturerRepository,
    IRepository<Grade> gradeRepository) : ILookupService
{
    public Task<IReadOnlyList<LookupItemDto>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LookupItemDto> suppliers = supplierRepository.Query()
            .Where(supplier => supplier.IsActive)
            .OrderBy(supplier => supplier.Name)
            .Select(supplier => new LookupItemDto(supplier.Id, supplier.Code, supplier.Name, null, null, null))
            .ToList();

        return Task.FromResult(suppliers);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetActiveManufacturersAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LookupItemDto> manufacturers = manufacturerRepository.Query()
            .Where(manufacturer => manufacturer.IsActive)
            .OrderBy(manufacturer => manufacturer.Name)
            .Select(manufacturer => new LookupItemDto(manufacturer.Id, manufacturer.Code, manufacturer.Name, null, null, null))
            .ToList();

        return Task.FromResult(manufacturers);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetActiveGradesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LookupItemDto> grades = gradeRepository.Query()
            .Where(grade => grade.IsActive)
            .OrderBy(grade => grade.Code)
            .Select(grade => new LookupItemDto(grade.Id, grade.Code, grade.Name, grade.ThicknessMm, grade.Category, grade.CoreLossPerKg))
            .ToList();

        return Task.FromResult(grades);
    }
}
