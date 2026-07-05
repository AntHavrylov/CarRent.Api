using CarRent.Api.Validators;
using CarRent.Contracts.Requests;
using FluentAssertions;
using FluentValidation;

namespace CarRent.Api.Tests.Unit.Validators;

public class GetAllCarsRequestValidatorTests
{
    private readonly IValidator<GetAllCarsRequest> _sut;

    public GetAllCarsRequestValidatorTests()
    {
        _sut = new GetAllCarsRequestValidator();
    }

    private static GetAllCarsRequest CreateRequest(int page = 1, int pageSize = 10) => new()
    {
        Page = page,
        PageSize = pageSize,
        SortBy = null
    };

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldNotThrowException_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should().NotThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenPageIsLessThanOne(int page)
    {
        // Arrange
        var request = CreateRequest(page: page);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000000)]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenPageSizeIsOutOfRange(int pageSize)
    {
        // Arrange
        var request = CreateRequest(pageSize: pageSize);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task ValidateAndThrowAsync_ShouldNotThrowException_WhenPageSizeIsAtBoundary(int pageSize)
    {
        // Arrange
        var request = CreateRequest(pageSize: pageSize);

        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should().NotThrowAsync<ValidationException>();
    }
}
