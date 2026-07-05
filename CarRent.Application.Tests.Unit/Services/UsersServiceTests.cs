using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Services;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Services;

public class UsersServiceTests
{
    private readonly IUsersRepository _userRepository = Substitute.For<IUsersRepository>();
    private readonly IValidator<User> _userValidator = Substitute.For<IValidator<User>>();

    private readonly IUsersService _sut;

    public UsersServiceTests()
    {
        _sut = new UsersService(_userRepository, _userValidator);
    }

    private static User CreateUser(Guid id) => new()
    {
        Id = id,
        Name = "Test",
        Email = "test@test.com"
    };

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());
        _userRepository.ExistsByIdAsync(user.Id).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(user);

        // Assert
        result.Should().BeNull();
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedUser_WhenUserExists()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());
        _userRepository.ExistsByIdAsync(user.Id).Returns(true);
        _userRepository.UpdateAsync(user).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(user);

        // Assert
        result.Should().Be(user);
    }
}
