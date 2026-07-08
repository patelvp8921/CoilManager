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

        RuleFor(request => request.PONumber)
            .MaximumLength(50);

        RuleFor(request => request.InvoiceNo)
            .MaximumLength(50);

        RuleFor(request => request.MillTCNo)
            .MaximumLength(100);

        RuleFor(request => request.BISLicNumber)
            .MaximumLength(100);

        RuleFor(request => request.SupplierId)
            .NotEmpty();

        RuleFor(request => request.ManufacturerId)
            .NotEmpty();

        RuleFor(request => request.GradeId)
            .NotEmpty();

        RuleFor(request => request.Width)
            .GreaterThan(0)
            .When(request => request.Width.HasValue);

        RuleFor(request => request.Weight)
            .GreaterThan(0);

        RuleFor(request => request.Length)
            .GreaterThanOrEqualTo(0);

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
