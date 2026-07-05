using CarRent.Api.Controllers;
using CarRent.Api.Mapping;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using FluentAssertions;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Security.Claims;

namespace CarRent.Api.Tests.Unit.Controllers;

public class OrdersControllerTests
{
    private readonly IOrdersService _ordersService = Substitute.For<IOrdersService>();
    private readonly IValidator<CreateOrUpdateOrderRequest> _requestValidator = Substitute.For<IValidator<CreateOrUpdateOrderRequest>>();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _ = new MapsterConfiguration();
        _sut = new OrdersController(_ordersService, _requestValidator);
    }

    private void AuthenticateAs(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim("userid", userId.ToString()) });
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenOrderBelongsToAnotherUser()
    {
        // Arrange: the service enforces ownership (REVIEW #3) and returns null when the
        // caller doesn't own the order being updated.
        AuthenticateAs(Guid.NewGuid());
        var request = new CreateOrUpdateOrderRequest
        {
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(1),
            DateTo = DateTime.UtcNow.AddDays(2)
        };
        _ordersService.UpdateAsync(Arg.Any<Order>()).ReturnsNull();

        // Act
        var result = await _sut.Update(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ShouldMapCarIdAndDatesCorrectly_WhenCallerOwnsTheOrder()
    {
        // Regression test for REVIEW #16: Update's tuple must match the registered Mapster
        // config exactly (same shape as Create's), or CarId/DateFrom/DateTo silently zero out.
        var userId = Guid.NewGuid();
        AuthenticateAs(userId);
        var orderId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var dateFrom = DateTime.UtcNow.AddDays(1);
        var dateTo = DateTime.UtcNow.AddDays(2);
        var request = new CreateOrUpdateOrderRequest
        {
            CarId = carId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        Order? capturedOrder = null;
        _ordersService.UpdateAsync(Arg.Do<Order>(o => capturedOrder = o))
            .Returns(callInfo => callInfo.Arg<Order>());

        // Act
        var result = (OkObjectResult)await _sut.Update(orderId, request, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Id.Should().Be(orderId);
        capturedOrder.UserId.Should().Be(userId);
        capturedOrder.CarId.Should().Be(carId);
        capturedOrder.DateFrom.Should().Be(dateFrom);
        capturedOrder.DateTo.Should().Be(dateTo);
        result.Value.As<OrderResponse>().CarId.Should().Be(carId);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedOrder_WhenRequestIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthenticateAs(userId);
        var request = new CreateOrUpdateOrderRequest
        {
            CarId = Guid.NewGuid(),
            DateFrom = DateTime.UtcNow.AddDays(1),
            DateTo = DateTime.UtcNow.AddDays(2)
        };
        _ordersService.CreateAsync(Arg.Any<Order>()).Returns(true);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CancelOrder_ShouldReturnNotFound_WhenOrderCannotBeCancelled()
    {
        // Arrange
        AuthenticateAs(Guid.NewGuid());
        _ordersService.CancelAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.CancelOrder(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
