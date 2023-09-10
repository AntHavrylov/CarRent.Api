using CarRent.Application.Models;

namespace CarRent.Application.Repositories;

public interface IRatingRepository
{
    Task<bool> RateCarAsync(Guid userId, Guid carId, int rating, CancellationToken token = default);

    Task<float?> GetRatingAsync(Guid carId, CancellationToken token = default);

    Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid userId, Guid carId, CancellationToken token = default);

    Task<bool> DeleteRatingAsync(Guid userId, Guid carId, CancellationToken token = default);

    Task<IEnumerable<CarRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default);

    Task<bool> ExistsCarRatingForUser(Guid userId, Guid carId, CancellationToken token = default);
}
