using CarRent.Application.Models;
using CarRent.Application.Repositories;
using FluentValidation;

namespace CarRent.Application.Validators;

public class CreateUserValidator : AbstractValidator<User>
{
    private readonly IUserRepository _userRepository;

    public CreateUserValidator(IUserRepository userRepository, CancellationToken token = default)
    {
        _userRepository = userRepository;
        RuleFor(x => x)
            .MustAsync(async (x, token) =>
            {
                var result = await EmailExists(x.Id, x.Email, token);
                return !result;
            })
            .WithMessage($" Another user with current email already exists.");
    }

    private async Task<bool> EmailExists(Guid id, string email, CancellationToken token) =>
        await _userRepository.ExistsByEmailAndIdAsync(id,email, token);
}
