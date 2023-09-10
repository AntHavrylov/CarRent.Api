using CarRent.Application.Models;

namespace CarRent.Application.Services;

public interface IOrdersService
{
    Task<bool> CreateAsync(Order order, CancellationToken token = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default);

    Task<bool> CancelAsync(Guid userId, Guid id, CancellationToken token = default);
    
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken token = default);

    Task<IEnumerable<Order>> GetAllByUserIdAsync(Guid id, CancellationToken token = default);

    Task<Order?> UpdateAsync(Order order, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
    
}
