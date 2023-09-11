using CarRent.Application.Models;

namespace CarRent.Application.Repositories;

public interface IUsersRepository
{
    Task<bool> CreateAsync(User user, CancellationToken token = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);

    Task<IEnumerable<User>> GetAllAsync(CancellationToken token = default);

    Task<bool> UpdateAsync(User user, CancellationToken token = default);
    
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);

    Task<bool> ExistsByEmailAndIdAsync(Guid id,string email, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);

}
