using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using FluentAssertions;
using Mapster;
using System.Collections;

namespace CarRent.Api.Tests.Unit.Mapping;

public class MapsterTests
{

    [Theory]
    [ClassData(typeof(AdaptOrderClassData))]
    public void Adapt_ShouldReturnOrder_WhenCreateOrUpdateOrderRequestIsPassed(
        CreateOrUpdateOrderRequest request,
        Guid userId,
        Guid orderId,
        Order expected) 
    {
        // Arrange
        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateOrUpdateOrderRequest request, Guid id, Guid UserId), Order>()
            .Map(dest => dest, source => source.request)
            .Map(dest => dest.Id, source => source.id)
            .Map(dest => dest.UserId, source => source.UserId);

        // Act
        var result = (request, orderId, userId).Adapt<Order>();
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    public class AdaptOrderClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var dateTime = DateTime.Now;
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var carId = Guid.NewGuid(); 
            yield return new object[]
            {
                new CreateOrUpdateOrderRequest
                {
                    CarId = carId,
                    DateFrom = dateTime,
                    DateTo = dateTime.AddDays(10),
                },
                userId,
                orderId,
                new Order
                {
                    Id = orderId,
                    UserId = userId,
                    DateFrom = dateTime,
                    DateTo = dateTime.AddDays(10),
                    CarId = carId,
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
