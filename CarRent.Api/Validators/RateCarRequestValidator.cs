using CarRent.Contracts.Requests;
using FluentValidation;

namespace CarRent.Api.Validators;

public class RateCarRequestValidator : AbstractValidator<RateCarRequest>
{
    public RateCarRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating value has to be between 1 and 5 inclusivelly.");
    }
}
