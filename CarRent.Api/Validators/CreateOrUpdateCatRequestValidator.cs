using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using FluentValidation;

namespace CarRent.Api.Validators;

public class CreateOrUpdateCatRequestValidator : AbstractValidator<CreateOrUpdateCarRequest>
{
    private static readonly string[] AcceptableEngineTypes = Enum.GetNames(typeof(EngineType));
    private static readonly string[] AcceptableBodyTypes = Enum.GetNames(typeof(BodyType));

    public CreateOrUpdateCatRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotEmpty();
        RuleFor(x => x.Brand)
            .NotEmpty();
        RuleFor(x => x.YearOfProduction)
            .InclusiveBetween(1900, DateTime.Now.Year)
            .WithMessage($"Year of production has to be between 1900 and {DateTime.Now.Year}");
        RuleFor(x => x.EngineType)
            .Must(x => AcceptableEngineTypes.Contains(x))
            .WithMessage($"Engine type must be one from list: {String.Join(',',AcceptableEngineTypes)}");
        RuleFor(x => x.BodyType)
            .Must(x => AcceptableBodyTypes.Contains(x))
            .WithMessage($"Body type must be one from list: {String.Join(',', AcceptableBodyTypes)}");
    }
}
