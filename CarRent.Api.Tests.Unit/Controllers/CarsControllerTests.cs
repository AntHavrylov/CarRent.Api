using CarRent.Api.Controllers;
using CarRent.Api.Mapping;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using FluentAssertions;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CarRent.Api.Tests.Unit.Controllers;

public class CarsControllerTests
{
    private readonly ICarsService _carsService = Substitute.For<ICarsService>();
    private readonly IValidator<CreateOrUpdateCarRequest> _createCarRequestValidator = Substitute.For<IValidator<CreateOrUpdateCarRequest>>();
    private readonly IValidator<GetAllCarsRequest> _getAllCarsRequestValidator = Substitute.For<IValidator<GetAllCarsRequest>>();
    private readonly CarsController _sut;

    public CarsControllerTests()
    {
        _ = new MapsterConfiguration();
        _sut = new CarsController(_carsService, _createCarRequestValidator, _getAllCarsRequestValidator);
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
    public async Task GetById_ShouldReturnNotFound_WhenCarDoesNotExist()
    {
        // Arrange
        _carsService.GetById(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithCar_WhenCarExists()
    {
        // Arrange
        var car = CreateCar(Guid.NewGuid());
        _carsService.GetById(car.Id).Returns(car);

        // Act
        var result = (OkObjectResult)await _sut.GetById(car.Id, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(car.Adapt<CarResponse>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPagedResult_WhenRequestIsValid()
    {
        // Arrange
        var cars = new[] { CreateCar(Guid.NewGuid()) };
        var request = new GetAllCarsRequest { Page = 1, PageSize = 10, SortBy = null };
        _carsService.GetAllAsync(Arg.Any<GetAllCarsOptions>()).Returns(cars);
        _carsService.GetCountAsync(Arg.Any<GetAllCarsOptions>()).Returns(cars.Length);

        // Act
        var result = (OkObjectResult)await _sut.GetAll(request, CancellationToken.None);

        // Assert
        var response = result.Value.As<CarsResponse>();
        response.Total.Should().Be(cars.Length);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.Items.Should().BeEquivalentTo(cars.Adapt<IEnumerable<CarResponse>>());
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCarDoesNotExist()
    {
        // Arrange
        var request = new CreateOrUpdateCarRequest
        {
            Model = "Accord",
            Brand = "Honda",
            EngineType = "Gasoline",
            BodyType = "Sedan",
            YearOfProduction = 2021
        };
        _carsService.UpdateAsync(Arg.Any<Car>()).ReturnsNull();

        // Act
        var result = await _sut.Update(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
