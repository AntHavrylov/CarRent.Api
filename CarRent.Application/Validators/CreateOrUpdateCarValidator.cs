using CarRent.Application.Models;
using CarRent.Application.Repositories;
using FluentValidation;

namespace CarRent.Application.Validators;

public class CreateOrUpdateCarValidator : AbstractValidator<Car>
{
    private readonly ICarsRepository _carsRepository;

    public CreateOrUpdateCarValidator(ICarsRepository carsRepository)
    {
        _carsRepository = carsRepository;
        RuleFor(x => x)
            .MustAsync(async (x, token) =>
            {
                var result = await SlugExists(x.Id, x.Slug, token);
                return !result;
            })
            .WithMessage("Another car with the same brand, model and year of production already exists.");
    }

    private async Task<bool> SlugExists(Guid id, string slug, CancellationToken token = default) =>
        await _carsRepository.ExistsBySlugAndIdAsync(id, slug, token);
}
