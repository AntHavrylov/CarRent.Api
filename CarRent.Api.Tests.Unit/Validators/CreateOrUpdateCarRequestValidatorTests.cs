using CarRent.Api.Validators;
using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using FluentAssertions;
using FluentValidation;
using System.Collections;

namespace CarRent.Api.Tests.Unit.Validators;

public class CreateOrUpdateCarRequestValidatorTests
{
    private readonly IValidator<CreateOrUpdateCarRequest> _sut;

    public CreateOrUpdateCarRequestValidatorTests()
    {
        _sut = new CreateOrUpdateCatRequestValidator();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldMotThrowException_WhenRequestIsComplete()
    {
        // Complete request:
        // Model and Brand is not empty
        // EngineType one of "Gasoline,Diesel,Hybrid,Electric"
        // BodyType one of "Sedan,HatchBack,Universal,Coupe,Suv,Crossover,Van,Micro"
        // YearOfProduction shoul be between 1900 and current year

        // Arrange 
        var request = new CreateOrUpdateCarRequest()
        {
            Model = "test",
            Brand = "test",
            EngineType = "Diesel",
            BodyType = "Sedan",
            YearOfProduction = DateTime.Now.Year
        };
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenModelIsEmpty() 
    {
        // Arrange 
        var request = new CreateOrUpdateCarRequest()
        {
            Model = string.Empty,
            Brand = "test",
            EngineType = "Diesel",
            BodyType = "Sedan",
            YearOfProduction = DateTime.Now.Year
        };
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenBrandIsEmpty()
    {
        // Arrange 
        var request = new CreateOrUpdateCarRequest()
        {
            Model = "test",
            Brand = string.Empty,
            EngineType = "Diesel",
            BodyType = "Sedan",
            YearOfProduction = DateTime.Now.Year
        };
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Theory]
    [ClassData(typeof(ValidateAndThrowAsyncYearOfProductionClassData))]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenYearOfProductionIsLessThan1900OrGreaterThanCurrent(CreateOrUpdateCarRequest request)
    {
        // Arrange 
        
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    public class ValidateAndThrowAsyncYearOfProductionClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new CreateOrUpdateCarRequest()
                {
                    Model = "test",
                    Brand = "test",
                    EngineType = "Diesel",
                    BodyType = "Sedan",
                    YearOfProduction = DateTime.Now.Year + 1
                }
            };
            yield return new object[]
            {
                new CreateOrUpdateCarRequest()
                {
                    Model = "test",
                    Brand = "test",
                    EngineType = "Diesel",
                    BodyType = "Sedan",
                    YearOfProduction = 1900-1
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenEngineTypeIsNotInTypeList()
    {
        // Arrange 
        var request = new CreateOrUpdateCarRequest()
        {
            Model = "test",
            Brand = "test",
            EngineType = "Water",
            BodyType = "Sedan",
            YearOfProduction = DateTime.Now.Year
        };
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidateAndThrowAsync_ShouldThrowException_WhenBodyTypeIsNotInTypeList()
    {
        // Arrange 
        var request = new CreateOrUpdateCarRequest()
        {
            Model = "test",
            Brand = "test",
            EngineType = "Diesel",
            BodyType = "Bus",
            YearOfProduction = DateTime.Now.Year
        };
        // Act
        var result = () => _sut.ValidateAndThrowAsync(request);

        // Assert
        await result.Should()
            .ThrowAsync<ValidationException>();
    }

}
