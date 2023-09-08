using CarRent.Contracts.Requests;
using FluentValidation;

namespace CarRent.Api.Validators;

public  class GetAllCarsRequestValidator : AbstractValidator<GetAllCarsRequest>
{
    public GetAllCarsRequestValidator()
    {
        RuleFor(x => x.YearOfProduction)
            .Must(x => x == null || (x >= 1900 && x <= DateTime.Now.Year))
            .WithMessage($"Year of production can be between 1900 and {DateTime.Now.Year}");
    }
}
