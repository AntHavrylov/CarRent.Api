using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Services;

public class RatingsServiceTests
{
    private readonly IRatingsRepository _ratingsRepository = 
        Substitute.For<IRatingsRepository>();
    private readonly IValidator<CarRating> _ratingValidator =
        Substitute.For<IValidator<CarRating>>();

    private readonly IRatingsService _sut;

    public RatingsServiceTests()
    {
        _sut = new RatingsService(_ratingsRepository, _ratingValidator);
    }

    [Fact]
    public async Task RateCarAsync_ShouldReturnTrue_WhenServiceReturnsTrue() 
    {
        // Arrange
        var carRating = new CarRating()
        {
            CarId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Rating = 5
        };

        _ratingsRepository
            .RateCarAsync(carRating.UserId, carRating.CarId, carRating.Rating)
            .Returns(true);

        // Act
        var res = await _sut.RateCarAsync(carRating);

        //Assert
        res.Should().BeTrue();
    }



}
