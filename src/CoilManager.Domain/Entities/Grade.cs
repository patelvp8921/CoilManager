using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Grade : AuditableEntity, IMasterDataEntity
{
    private static readonly IReadOnlyDictionary<decimal, string> CategoriesByThickness = new Dictionary<decimal, string>
    {
        [0.23m] = "M3",
        [0.27m] = "M4",
        [0.30m] = "M5",
        [0.35m] = "M6"
    };

    private Grade()
    {
    }

    public Grade(string code, string description, bool isActive = true)
        : this(code, 0.23m, 0.85m, isActive)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public Grade(string code, string name, string? description, bool isActive = true)
        : this(code, 0.23m, 0.85m, isActive)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public Grade(string grade, decimal thicknessMm, decimal coreLossPerKg, bool isActive = true)
    {
        string normalizedGrade = grade.Trim();
        Code = normalizedGrade;
        Name = normalizedGrade;
        ThicknessMm = thicknessMm;
        Category = DeriveCategory(thicknessMm);
        CoreLossPerKg = coreLossPerKg;
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string GradeCode => Code;
    public decimal ThicknessMm { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal CoreLossPerKg { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Update(string code, string name, string? description, bool isActive)
    {
        Update(code, ThicknessMm, CoreLossPerKg, isActive);
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Update(string grade, decimal thicknessMm, decimal coreLossPerKg, bool isActive)
    {
        string normalizedGrade = grade.Trim();
        Code = normalizedGrade;
        Name = normalizedGrade;
        ThicknessMm = thicknessMm;
        Category = DeriveCategory(thicknessMm);
        CoreLossPerKg = coreLossPerKg;
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public static bool IsSupportedThickness(decimal thicknessMm)
    {
        return CategoriesByThickness.ContainsKey(thicknessMm);
    }

    public static string DeriveCategory(decimal thicknessMm)
    {
        return CategoriesByThickness.TryGetValue(thicknessMm, out string? category)
            ? category
            : throw new ArgumentOutOfRangeException(nameof(thicknessMm), "Thickness must be one of 0.23, 0.27, 0.30, or 0.35.");
    }
}
