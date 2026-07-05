using CoilManager.Application.DTOs.RawCoils;
using FluentValidation;

namespace CoilManager.Application.Validators.RawCoils;

public sealed class CreateRawCoilRequestValidator : AbstractValidator<CreateRawCoilRequest>
{
    public CreateRawCoilRequestValidator()
    {
        RuleFor(request => request.CoilNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.HeatNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.SupplierName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Grade)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.ThicknessMm)
            .GreaterThan(0);

        RuleFor(request => request.WidthMm)
            .GreaterThan(0);

        RuleFor(request => request.WeightMt)
            .GreaterThan(0);
    }
}
