using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Services;
using FluentAssertions;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Services;

public class OrdersServiceTests
{
    private readonly IOrdersRepository _ordersRepository =
        Substitute.For<IOrdersRepository>();

    private readonly IOrdersService _sut;

    public OrdersServiceTests()
    {
        _sut = new OrdersService(_ordersRepository);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenOrderBelongsToAnotherUser()
    {
        // Arrange
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(1),
            DateTo = DateTime.UtcNow.AddDays(2)
        };
        var attackerUpdate = new Order
        {
            Id = existingOrder.Id,
            UserId = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(3),
            DateTo = DateTime.UtcNow.AddDays(4)
        };
        _ordersRepository.GetByIdAsync(existingOrder.Id).Returns(existingOrder);

        // Act
        var result = await _sut.UpdateAsync(attackerUpdate);

        // Assert
        result.Should().BeNull();
        await _ordersRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(1),
            DateTo = DateTime.UtcNow.AddDays(2)
        };
        _ordersRepository.GetByIdAsync(order.Id).Returns((Order?)null);

        // Act
        var result = await _sut.UpdateAsync(order);

        // Assert
        result.Should().BeNull();
        await _ordersRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedOrder_WhenCallerOwnsTheOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(1),
            DateTo = DateTime.UtcNow.AddDays(2)
        };
        var update = new Order
        {
            Id = existingOrder.Id,
            UserId = userId,
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(3),
            DateTo = DateTime.UtcNow.AddDays(4)
        };
        _ordersRepository.GetByIdAsync(existingOrder.Id).Returns(existingOrder);
        _ordersRepository.UpdateAsync(update).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(update);

        // Assert
        result.Should().Be(update);
    }
}
