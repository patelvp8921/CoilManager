using CoilManager.Application.DTOs.SlittingJobs;
using FluentValidation;

namespace CoilManager.Application.Validators.SlittingJobs;

public sealed class CompleteSlittingRequestValidator : AbstractValidator<CompleteSlittingRequest>
{
    public CompleteSlittingRequestValidator()
    {
        RuleFor(request => request.RowVersion)
            .NotEmpty();

        RuleFor(request => request.Slits)
            .NotEmpty()
            .WithMessage("Complete slitting request must include actual slit details.");

        RuleForEach(request => request.Slits)
            .SetValidator(new CompleteSlittingItemRequestValidator());
    }
}

public sealed class CompleteSlittingItemRequestValidator : AbstractValidator<CompleteSlittingItemRequest>
{
    public CompleteSlittingItemRequestValidator()
    {
        RuleFor(item => item.SlittingJobItemId)
            .NotEmpty();

        RuleFor(item => item.ActualWeight)
            .GreaterThan(0)
            .WithMessage("Actual weight must be greater than zero.");

        RuleFor(item => item.ActualWidth)
            .GreaterThan(0)
            .When(item => item.ActualWidth.HasValue)
            .WithMessage("Actual width must be greater than zero.");

        RuleFor(item => item.Remarks)
            .MaximumLength(250);
    }
}

public sealed class StartSlittingRequestValidator : AbstractValidator<StartSlittingRequest>
{
    public StartSlittingRequestValidator()
    {
        RuleFor(request => request.RowVersion)
            .NotEmpty();

        RuleFor(request => request.Shift)
            .MaximumLength(30);

        RuleFor(request => request.Remarks)
            .MaximumLength(500);
    }
}
