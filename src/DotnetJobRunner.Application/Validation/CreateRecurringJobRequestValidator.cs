using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using FluentValidation;

namespace DotnetJobRunner.Application.Validation;

public class CreateRecurringJobRequestValidator : AbstractValidator<CreateRecurringJobRequest>
{
    private static readonly string[] AllowedPriorities = ["low", "normal", "high"];

    public CreateRecurringJobRequestValidator(IJobHandlerResolver handlerResolver)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .Must(handlerResolver.Exists)
            .WithMessage(_ => $"Job type is not supported. Supported types: {string.Join(", ", handlerResolver.SupportedJobTypes)}.");

        RuleFor(x => x.CronExpression)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => AllowedPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Priority must be one of: low, normal, high.");
    }
}
