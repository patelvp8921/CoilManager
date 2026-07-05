using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Domain.Enums;
using FluentValidation;

namespace CoilManager.Application.Validators.RawCoils;

public sealed class UpdateRawCoilRequestValidator : AbstractValidator<UpdateRawCoilRequest>
{
    public UpdateRawCoilRequestValidator()
    {
        RuleFor(request => request.CoilNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.HeatNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.MillName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.MillTCNo)
            .MaximumLength(100);

        RuleFor(request => request.BISLicNumber)
            .MaximumLength(100);

        RuleFor(request => request.SupplierName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Grade)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.Thickness)
            .GreaterThan(0)
            .When(request => request.Thickness.HasValue);

        RuleFor(request => request.Width)
            .GreaterThan(0)
            .When(request => request.Width.HasValue);

        RuleFor(request => request.Weight)
            .GreaterThan(0);

        RuleFor(request => request.Length)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.WattLossPerKg)
            .GreaterThan(0)
            .When(request => request.WattLossPerKg.HasValue);

        RuleFor(request => request.WarehouseLocation)
            .MaximumLength(100);

        RuleFor(request => request.Status)
            .Must(status => Enum.IsDefined(typeof(CoilStatus), status))
            .WithMessage("Status must be valid.");

        RuleFor(request => request.ReceivedDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(request => request.RowVersion)
            .NotEmpty();
    }
}
