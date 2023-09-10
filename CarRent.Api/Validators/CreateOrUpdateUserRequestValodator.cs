using CarRent.Application.Repositories;
using CarRent.Contracts.Requests;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CarRent.Api.Validators;

public partial class CreateOrUpdateUserRequestValodator : AbstractValidator<CreateOrUpdateUserRequest>
{
    
    public CreateOrUpdateUserRequestValodator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty();
        RuleFor(x => x.Email)
            .Must(x => MailRegex().IsMatch(x))
            .WithMessage("Mail has to be formated as 'xxxx@xx.xx'");
    }

    [GeneratedRegex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$")]
    private static partial Regex MailRegex();
}
