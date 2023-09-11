using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Validators;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Validators;

public class CreateRatingValidatorTests
{
    private readonly IRatingsRepository _ratingRepository = Substitute.For<IRatingsRepository>();
    private readonly IValidator<CarRating> _sut;

    public CreateRatingValidatorTests()
    {
        _sut = new CreateRatingValidator(_ratingRepository);
    }

    [Fact]
    public async void ValidateAndThrowAsync_ShouldThrowValidationException_WhenOrderNotExists() 
    {
        // Arrange
        var carRating = new CarRating()
        {
            UserId = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            Rating = 1
        };
        _ratingRepository.ExistsCarRatingForUser(carRating.UserId, carRating.CarId).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(carRating);

        // Assert
        await result.Should()            
            .ThrowAsync<ValidationException>();

    }

    [Fact]
    public async void ValidateAndThrowAsync_ShouldNotThrowValidationException_WhenOrderExists()
    {
        // Arrange
        var carRating = new CarRating()
        {
            UserId = Guid.NewGuid(),
            CarId = Guid.NewGuid(),
            Rating = 1
        };
        _ratingRepository.ExistsCarRatingForUser(carRating.UserId, carRating.CarId).Returns(true);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(carRating);

        // Assert
        await result.Should().NotThrowAsync();         
    }
}
