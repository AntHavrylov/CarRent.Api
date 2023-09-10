using CarRent.Application.Models;

namespace CarRent.Application.Repositories;

public interface IOrdersRepository
{
    Task<bool> CreateAsync(Order order, CancellationToken token = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default);

    Task<IEnumerable<Order>> GetAllAsync(CancellationToken token = default);

    Task<IEnumerable<Order>> GetAllByUserIdAsync(Guid id, CancellationToken token = default);

    Task<bool> UpdateAsync(Order order, CancellationToken token = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);

    Task<bool> ExistsByCarIdAndDateAsync(Guid id, DateTime from, DateTime to, CancellationToken token = default);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);

    Task<bool> CancelByUserIdAsync(Guid userId, Guid orderId, CancellationToken token);
}
