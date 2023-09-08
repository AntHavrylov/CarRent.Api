using CarRent.Contracts.Requests;
using FluentValidation;

namespace CarRent.Api.Validators;

public class CreateOrUpdateUserRequestValodator : AbstractValidator<CreateOrUpdateUserRequest>
{
    public CreateOrUpdateUserRequestValodator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty();
    }
}
