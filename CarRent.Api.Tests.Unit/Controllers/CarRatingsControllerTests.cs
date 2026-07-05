using CarRent.Api.Controllers;
using CarRent.Api.Mapping;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace CarRent.Api.Tests.Unit.Controllers;

public class CarRatingsControllerTests
{
    private readonly IRatingsService _ratingsService = Substitute.For<IRatingsService>();
    private readonly IValidator<RateCarRequest> _rateCarRequestValidator = Substitute.For<IValidator<RateCarRequest>>();
    private readonly CarRatingsController _sut;

    public CarRatingsControllerTests()
    {
        _ = new MapsterConfiguration();
        _sut = new CarRatingsController(_ratingsService, _rateCarRequestValidator);
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
    public async Task RateCar_ShouldReturnOk_WhenRatingSucceeds()
    {
        // Arrange
        AuthenticateAs(Guid.NewGuid());
        var request = new RateCarRequest { Rating = 5 };
        _ratingsService.RateCarAsync(Arg.Any<CarRating>()).Returns(true);

        // Act
        var result = await _sut.RateCar(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RateCar_ShouldReturnNotFound_WhenRatingFails()
    {
        // Arrange
        AuthenticateAs(Guid.NewGuid());
        var request = new RateCarRequest { Rating = 5 };
        _ratingsService.RateCarAsync(Arg.Any<CarRating>()).Returns(false);

        // Act
        var result = await _sut.RateCar(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteRating_ShouldReturnNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        AuthenticateAs(Guid.NewGuid());
        _ratingsService.DeleteRatingAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteRating(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
