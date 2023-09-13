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

public class UsersControllerTests
{
    private readonly IUsersService _userService = Substitute.For<IUsersService>();
    private readonly IValidator<CreateOrUpdateUserRequest> _userValidator = Substitute.For<IValidator<CreateOrUpdateUserRequest>>();
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _sut = new UsersController(_userService,_userValidator);
    }

    [Fact]
    public async Task GetById_ReturnOkAndObject_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "TestName",
            Email = "TestEmail"
        };
        _userService.GetByIdAsync(user.Id).Returns(user);
        var userResponse = user.Adapt<UserResponse>();

        // Act
        var result = (OkObjectResult)await _sut.GetById(user.Id, new CancellationToken());

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(userResponse);
    }

    [Fact]
    public async Task GetById_ReturnNotFound_WhenUserDoesntExists()
    {
        // Arrange
        _userService.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetById(Guid.NewGuid(), new CancellationToken());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _userService.GetAllAsync().Returns(Enumerable.Empty<User>());

        // Act
        var result = (OkObjectResult)await _sut.GetAll(new CancellationToken());
                
        // Assert        
        result.StatusCode.Should().Be(200);
        result.Value.As<IEnumerable<UserResponse>>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnUsersResponse_WhenUsersExist()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "TestName",
            Email = "TestEmail"
        };
        var users = new[] { user };
        var usersResponse = users.Select(x => x.Adapt<UserResponse>());
        _userService.GetAllAsync().Returns(users);

        // Act
        var result = (OkObjectResult)await _sut.GetAll(new CancellationToken());

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.As<IEnumerable<UserResponse>>().Should().BeEquivalentTo(usersResponse);
    }

}
