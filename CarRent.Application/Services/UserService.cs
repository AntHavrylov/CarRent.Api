using CarRent.Application.Models;
using CarRent.Application.Repositories;

namespace CarRent.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> CreateAsync(User user, CancellationToken token = default)
    {
       return await _userRepository.CreateAsync(user, token);
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _userRepository.DeleteByIdAsync(id, token);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken token = default)
    {
        return await _userRepository.GetAllAsync(token);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return _userRepository.GetByIdAsync(id,token);
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken token = default)
    {
        var exists = await _userRepository.ExistsByIdAsync(user.Id, token);
        if (!exists) 
        {
            return null;
        }
        var result = await _userRepository.UpdateAsync(user,token);
        return result ? user : null;
    }
}
