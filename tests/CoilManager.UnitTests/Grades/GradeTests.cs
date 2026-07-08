using CoilManager.Domain.Entities;

namespace CoilManager.UnitTests.Grades;

public sealed class GradeTests
{
    [Theory]
    [InlineData("23HP85D", 0.23, 0.85, "M3")]
    [InlineData("27HP90D", 0.27, 0.90, "M4")]
    [InlineData("30HP95D", 0.30, 0.95, "M5")]
    [InlineData("35HP100D", 0.35, 1.00, "M6")]
    public void Constructor_DerivesCategoryFromThickness(string gradeCode, double thicknessMm, double coreLossPerKg, string expectedCategory)
    {
        Grade grade = new(gradeCode, (decimal)thicknessMm, (decimal)coreLossPerKg);

        Assert.Equal(gradeCode, grade.Code);
        Assert.Equal((decimal)thicknessMm, grade.ThicknessMm);
        Assert.Equal(expectedCategory, grade.Category);
        Assert.Equal((decimal)coreLossPerKg, grade.CoreLossPerKg);
    }

    [Theory]
    [InlineData(0.20)]
    [InlineData(0.25)]
    [InlineData(0.40)]
    public void Constructor_RejectsUnsupportedThickness(double thicknessMm)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grade("BAD", (decimal)thicknessMm, 0.85m));
    }

    [Fact]
    public void Update_RecalculatesCategoryFromThickness()
    {
        Grade grade = new("23HP85D", 0.23m, 0.85m);

        grade.Update("35HP100D", 0.35m, 1.00m, true);

        Assert.Equal("35HP100D", grade.Code);
        Assert.Equal(0.35m, grade.ThicknessMm);
        Assert.Equal("M6", grade.Category);
        Assert.Equal(1.00m, grade.CoreLossPerKg);
    }
}
