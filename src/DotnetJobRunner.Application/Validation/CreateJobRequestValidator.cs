using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using FluentValidation;

namespace DotnetJobRunner.Application.Validation;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    private static readonly string[] AllowedPriorities = ["low", "normal", "high"];

    public CreateJobRequestValidator(IJobHandlerResolver handlerResolver)
    {
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .Must(handlerResolver.Exists)
            .WithMessage(_ => $"Job type is not supported. Supported types: {string.Join(", ", handlerResolver.SupportedJobTypes)}.");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => AllowedPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Priority must be one of: low, normal, high.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10);
    }
}
