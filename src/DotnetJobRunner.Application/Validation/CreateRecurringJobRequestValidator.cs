using DotnetJobRunner.Application.DTOs;
using FluentValidation;

namespace DotnetJobRunner.Application.Validation;

public class CreateRecurringJobRequestValidator : AbstractValidator<CreateRecurringJobRequest>
{
    private static readonly string[] AllowedPriorities = ["low", "normal", "high", "critical"];

    public CreateRecurringJobRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.CronExpression)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => AllowedPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Priority must be one of: low, normal, high, critical.");
    }
}
