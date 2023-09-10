using CarRent.Application.Models;
using CarRent.Application.Repositories;
using FluentValidation;

namespace CarRent.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<CarRating> _ratingValidator;

    public RatingService(IRatingRepository ratingRepository, IValidator<CarRating> ratingValidator)
    {
        _ratingRepository = ratingRepository;
        _ratingValidator = ratingValidator;
    }

    public async Task<bool> RateCarAsync(CarRating carRating, CancellationToken token = default)
    {
        await _ratingValidator.ValidateAndThrowAsync(carRating, token);
        return await _ratingRepository.RateCarAsync(carRating.UserId, carRating.CarId, carRating.Rating, token);
    }

    public async Task<bool> DeleteRatingAsync(Guid userId, Guid carId, CancellationToken token = default)
    {
        return await _ratingRepository.DeleteRatingAsync(userId, carId, token);
    }

    public async Task<float?> GetRatingAsync(Guid carId, CancellationToken token = default)
    {
        return await _ratingRepository.GetRatingAsync(carId, token);
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid userId, Guid carId, CancellationToken token = default)
    {
        return await _ratingRepository.GetRatingAsync(userId, carId, token);
    }

    public async Task<IEnumerable<CarRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default)
    {
        return await _ratingRepository.GetRatingsForUserAsync(userId, token);
    }


}
