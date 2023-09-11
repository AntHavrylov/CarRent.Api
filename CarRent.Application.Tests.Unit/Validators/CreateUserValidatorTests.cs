using CarRent.Application.Models;
using CarRent.Application.Repositories;
using CarRent.Application.Validators;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace CarRent.Application.Tests.Unit.Validators;

public class CreateUserValidatorTests
{
    private readonly IUsersRepository _userRepository = 
        Substitute.For<IUsersRepository>();

    private readonly IValidator<User> _sut;

    public CreateUserValidatorTests()
    {
        _sut = new CreateUserValidator(_userRepository);
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenUserWithTestedEmailExists() 
    {
        // Arrange 
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "some@test.ml"
        };
        _userRepository.ExistsByEmailAndIdAsync(user.Id,user.Email).Returns(true);
        
        // Act
        var result = () => _sut.ValidateAndThrowAsync(user);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldNotThrowException_WhenUserWithTestedEmailNotExists()
    {
        // Arrange 
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "some@test.ml"
        };
        _userRepository.ExistsByEmailAndIdAsync(user.Id, user.Email).Returns(false);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(user);

        // Assert
        await result.Should()
            .NotThrowAsync<ValidationException>();
    }
}
