using DotnetJobRunner.Application.DTOs;
using FluentValidation;

namespace DotnetJobRunner.Application.Validation;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    private static readonly string[] AllowedPriorities = ["low", "normal", "high", "critical"];

    public CreateJobRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => AllowedPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Priority must be one of: low, normal, high, critical.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10);

        RuleFor(x => x)
            .Must(x => x.RunAt is null || string.IsNullOrWhiteSpace(x.CronExpression))
            .WithMessage("Use either RunAt or CronExpression, not both.");

        RuleFor(x => x.CronExpression)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Use the recurring-jobs endpoint to create recurring jobs.");
    }
}
