using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Services;

public class CarsServiceTests
{
    private readonly ICarsRepository _carsRepository = Substitute.For<ICarsRepository>();
    private readonly IValidator<Car> _carValidator = Substitute.For<IValidator<Car>>();

    private readonly ICarsService _sut;

    public CarsServiceTests()
    {
        _sut = new CarsService(_carsRepository, _carValidator);
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
    public async Task UpdateAsync_ShouldReturnNull_WhenCarDoesNotExist()
    {
        // Arrange
        var car = CreateCar(Guid.NewGuid());
        _carsRepository.ExistsByIdAsync(car.Id).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(car);

        // Assert
        result.Should().BeNull();
        await _carsRepository.DidNotReceive().UpdateAsync(Arg.Any<Car>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCar_WhenCarExists()
    {
        // Arrange
        var car = CreateCar(Guid.NewGuid());
        _carsRepository.ExistsByIdAsync(car.Id).Returns(true);
        _carsRepository.UpdateAsync(car).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(car);

        // Assert
        result.Should().Be(car);
    }
}
