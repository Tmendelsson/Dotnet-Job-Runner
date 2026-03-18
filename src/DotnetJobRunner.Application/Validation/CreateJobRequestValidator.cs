using DotnetJobRunner.Application.DTOs;
using FluentValidation;

namespace DotnetJobRunner.Application.Validation;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    private static readonly string[] AllowedPriorities = ["low", "normal", "high"];

    public CreateJobRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => AllowedPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Priority must be one of: low, normal, high.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10);
    }
}
