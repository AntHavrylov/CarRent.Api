using CarRent.Application.Models;
using CarRent.Application.Repositories;
using System.Net.Http.Headers;

namespace CarRent.Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;

    public OrdersService(IOrdersRepository ordersRepository)
    {
        this._ordersRepository = ordersRepository;
    }

    public async Task<bool> CancelAsync(Guid userId, Guid id, CancellationToken token = default)
    {
        return await _ordersRepository.CancelByUserIdAsync(userId, id, token);
    }

    public async Task<bool> CreateAsync(Order order, CancellationToken token = default)
    {        
        return await _ordersRepository.CreateAsync(order, token);
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _ordersRepository.DeleteByIdAsync(id, token);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken token = default)
    {
        return await _ordersRepository.GetAllAsync(token);
    }

    public async Task<IEnumerable<Order>> GetAllByUserIdAsync(Guid id, CancellationToken token = default)
    {
       return await _ordersRepository.GetAllByUserIdAsync(id, token);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _ordersRepository.GetByIdAsync(id, token);
    }

    public async Task<Order?> UpdateAsync(Order order, CancellationToken token = default)
    {
        var exists = await _ordersRepository.ExistsByIdAsync(order.Id, token);
        if(!exists) 
        {
            return null;
        }
        var bRes = await _ordersRepository.UpdateAsync(order, token);
        return bRes ? order : null;
    }
}
