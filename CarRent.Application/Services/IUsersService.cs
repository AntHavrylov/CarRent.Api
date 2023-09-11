using CarRent.Application.Models;

namespace CarRent.Application.Services;

public interface IUsersService
{
    Task<bool> CreateAsync(User user, CancellationToken token = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);

    Task<IEnumerable<User>> GetAllAsync(CancellationToken token = default);

    Task<User?> UpdateAsync(User user, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
}
