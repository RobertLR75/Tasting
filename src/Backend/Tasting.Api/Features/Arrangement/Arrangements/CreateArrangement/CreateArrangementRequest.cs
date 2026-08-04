using FluentValidation;

namespace Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;

public sealed record CreateArrangementRequest(string Name, string? Description);

public sealed class CreateArrangementRequestValidator : AbstractValidator<CreateArrangementRequest>
{
    public CreateArrangementRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
