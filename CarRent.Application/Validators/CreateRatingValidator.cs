using CarRent.Application.Models;
using CarRent.Application.Repositories;
using FluentValidation;

namespace CarRent.Application.Validators;

public class CreateRatingValidator : AbstractValidator<CarRating>
{
    private readonly IRatingRepository _ratingRepository;
      

    public CreateRatingValidator(IRatingRepository ratingRepository,CancellationToken token = default)
    {
        _ratingRepository = ratingRepository;
        RuleFor(x => x)
            .MustAsync(async (x, token) =>
            {
                var result = await OrderExistsAsync(x.UserId, x.CarId, token);
                return result;
            })
            .WithMessage("To rate has to be an order for corresponding car.");
    }

    private async Task<bool> OrderExistsAsync(Guid userId, Guid carId, CancellationToken token = default) =>
        await _ratingRepository.ExistsCarRatingForUser(userId,carId, token);


}
