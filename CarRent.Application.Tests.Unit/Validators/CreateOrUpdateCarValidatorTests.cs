using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Validators;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Validators;

public class CreateOrUpdateCarValidatorTests
{
    private readonly ICarsRepository _carsRepository =
        Substitute.For<ICarsRepository>();

    private readonly IValidator<Car> _sut;

    public CreateOrUpdateCarValidatorTests()
    {
        _sut = new CreateOrUpdateCarValidator(_carsRepository);
    }

    private static Car CreateCar(Guid id) => new()
    {
        Id = id,
        YearOfProduction = 2021,
        Brand = "Honda",
        Model = "Accord",
        EngineType = EngineType.Gasoline,
        BodyType = BodyType.Sedan
    };

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenAnotherCarWithSameSlugExists()
    {
        // Arrange
        var car = CreateCar(Guid.NewGuid());
        _carsRepository.ExistsBySlugAndIdAsync(car.Id, car.Slug).Returns(true);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(car);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldNotThrowException_WhenNoOtherCarWithSameSlugExists()
    {
        // Arrange
        var car = CreateCar(Guid.NewGuid());
        _carsRepository.ExistsBySlugAndIdAsync(car.Id, car.Slug).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(car);

        // Assert
        await result.Should()
            .NotThrowAsync<ValidationException>();
    }
}
