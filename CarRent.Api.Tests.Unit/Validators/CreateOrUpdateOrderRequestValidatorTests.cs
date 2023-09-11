using CarRent.Api.Validators;
using CarRent.Application.Repositories;
using CarRent.Contracts.Requests;
using FluentAssertions;
using FluentValidation;
using Newtonsoft.Json.Linq;
using NSubstitute;

namespace CarRent.Api.Tests.Unit.Validators;

public class CreateOrUpdateOrderRequestValidatorTests
{

    private readonly IOrdersRepository _ordersRepository = Substitute.For<IOrdersRepository>();
    private readonly ICarsRepository _carsRepository = Substitute.For<ICarsRepository>();
    private readonly IValidator<CreateOrUpdateOrderRequest> _sut;

    public CreateOrUpdateOrderRequestValidatorTests()
    {
        _sut = new CreateOrUpdateOrderRequestValidator(_ordersRepository, _carsRepository);
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldNotThrowValidationException_WhenRequestIsComplete()
    {
        // Complete request is:
        // DateFrom - DateTime Greater than DateTime.Now
        // DateTo - Greater than DateFrom
        // There is car in db with corresponding guid CarId
        // The period doesn't overlap other orders with current car

        // Arrange 
        var DateFrom = DateTime.Now;
        var request = new CreateOrUpdateOrderRequest()
        {
            DateFrom = DateFrom,
            DateTo = DateFrom.AddDays(10),
            CarId = Guid.NewGuid(),
        };
        _carsRepository.ExistsByIdAsync(Arg.Any<Guid>()).Returns(true);
        _ordersRepository.ExistsByCarIdAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert 
        await result.Should()
            .NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowValidationException_WhenDateFromIsLessThanDateTimeNow() 
    {
        // Arrange 
        var DateFrom = DateTime.Now;
        var request = new CreateOrUpdateOrderRequest()
        {
            DateFrom = DateFrom.AddDays(-1),
            DateTo = DateFrom.AddDays(10),
            CarId = Guid.NewGuid(),
        };

        _carsRepository.ExistsByIdAsync(request.CarId).Returns(true);
        _ordersRepository.ExistsByCarIdAndDateAsync(request.CarId, request.DateFrom, request.DateTo).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert 
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowValidationException_WhenDateFromIsGreaterThanDateTo()
    {
        // Arrange 
        var DateFrom = DateTime.Now;        
        var request = new CreateOrUpdateOrderRequest()
        {
            DateFrom = DateFrom,
            DateTo = DateFrom.AddDays(-1),
            CarId = Guid.NewGuid(),
        };
        _carsRepository.ExistsByIdAsync(Arg.Any<Guid>()).Returns(true);
        _ordersRepository.ExistsByCarIdAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert 
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowValidationException_WhenOrderDateIsOverlappedWithExistsOrder()
    {
        // Arrange 
        var DateFrom = DateTime.Now;
        var request = new CreateOrUpdateOrderRequest()
        {
            DateFrom = DateFrom,
            DateTo = DateFrom.AddDays(+10),
            CarId = Guid.NewGuid(),
        };
        _carsRepository.ExistsByIdAsync(Arg.Any<Guid>()).Returns(true);
        _ordersRepository.ExistsByCarIdAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(true);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert 
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowValidationException_WhenCarNotExists()
    {
        // Arrange 
        var DateFrom = DateTime.Now;
        var request = new CreateOrUpdateOrderRequest()
        {
            DateFrom = DateFrom,
            DateTo = DateFrom.AddDays(+10),
            CarId = Guid.NewGuid(),
        };
        _carsRepository.ExistsByIdAsync(Arg.Any<Guid>()).Returns(false);
        _ordersRepository.ExistsByCarIdAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert 
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

}
