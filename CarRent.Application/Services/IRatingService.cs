using CarRent.Application.Models;

namespace CarRent.Application.Services;

public interface IRatingService
{
    Task<bool> RateCarAsync(CarRating rating, CancellationToken token = default);

    Task<float?> GetRatingAsync(Guid carId, CancellationToken token = default);

    Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid userId, Guid carId,  CancellationToken token = default);

    Task<bool> DeleteRatingAsync(Guid userId, Guid carId, CancellationToken token = default);

    Task<IEnumerable<CarRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default);
}
