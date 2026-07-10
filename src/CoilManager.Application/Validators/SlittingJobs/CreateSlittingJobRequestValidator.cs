using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Services;
using FluentValidation;

namespace CoilManager.Application.Validators.SlittingJobs;

public sealed class CreateSlittingJobRequestValidator : AbstractValidator<CreateSlittingJobRequest>
{
    private const int MaximumSlitRows = 10;

    public CreateSlittingJobRequestValidator()
    {
        RuleFor(request => request.PlanningDate)
            .NotEmpty();

        RuleFor(request => request.MotherCoilId)
            .NotEmpty();

        RuleFor(request => request.Shift)
            .MaximumLength(30);

        RuleFor(request => request.Remarks)
            .MaximumLength(500);

        RuleFor(request => request.KnifeThickness)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.LeftEdgeTrim)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.RightEdgeTrim)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Items)
            .NotEmpty()
            .WithMessage("At least one slit row is required.")
            .Must(items => items is not null && items.Count <= MaximumSlitRows)
            .WithMessage($"Maximum {MaximumSlitRows} slit rows are allowed.");

        RuleForEach(request => request.Items)
            .SetValidator(new SlittingJobItemRequestValidator());

        RuleFor(request => request.Items)
            .Must(HaveSequentialRows)
            .WithMessage("Slit rows must be sequential starting from 1.");
    }

    private static bool HaveSequentialRows(IReadOnlyList<SlittingJobItemRequest>? items)
    {
        if (items is null || items.Count == 0)
        {
            return false;
        }

        return items
            .OrderBy(item => item.SequenceNo)
            .Select((item, index) => item.SequenceNo == index + 1)
            .All(isSequential => isSequential);
    }
}

public sealed class UpdateSlittingJobRequestValidator : AbstractValidator<UpdateSlittingJobRequest>
{
    private const int MaximumSlitRows = 10;

    public UpdateSlittingJobRequestValidator()
    {
        RuleFor(request => request.PlanningDate)
            .NotEmpty();

        RuleFor(request => request.MotherCoilId)
            .NotEmpty();

        RuleFor(request => request.RowVersion)
            .NotEmpty();

        RuleFor(request => request.Shift)
            .MaximumLength(30);

        RuleFor(request => request.Remarks)
            .MaximumLength(500);

        RuleFor(request => request.KnifeThickness)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.LeftEdgeTrim)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.RightEdgeTrim)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Items)
            .NotEmpty()
            .WithMessage("At least one slit row is required.")
            .Must(items => items is not null && items.Count <= MaximumSlitRows)
            .WithMessage($"Maximum {MaximumSlitRows} slit rows are allowed.");

        RuleForEach(request => request.Items)
            .SetValidator(new SlittingJobItemRequestValidator());

        RuleFor(request => request.Items)
            .Must(items => items
                .OrderBy(item => item.SequenceNo)
                .Select((item, index) => item.SequenceNo == index + 1)
                .All(isSequential => isSequential))
            .WithMessage("Slit rows must be sequential starting from 1.");
    }
}

public sealed class SlittingJobItemRequestValidator : AbstractValidator<SlittingJobItemRequest>
{
    public SlittingJobItemRequestValidator()
    {
        RuleFor(item => item.SequenceNo)
            .GreaterThan(0);

        RuleFor(item => item.Width)
            .GreaterThan(0);

        RuleFor(item => item.Remarks)
            .MaximumLength(250);
    }
}
