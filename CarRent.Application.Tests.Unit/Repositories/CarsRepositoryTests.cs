using CarRent.Application.Repositories;
using FluentAssertions;

namespace CarRent.Application.Tests.Unit.Repositories;

public class CarsRepositoryTests
{
    [Theory]
    [InlineData("brand", "brand")]
    [InlineData("BRAND", "brand")]
    [InlineData("yearOfProduction", "yearofproduction")]
    [InlineData("rating", "rating")]
    public void ResolveSortColumn_ShouldReturnWhitelistedColumn_ForKnownSortField(string sortField, string expectedColumn)
    {
        // Act
        var result = CarsRepository.ResolveSortColumn(sortField);

        // Assert
        result.Should().Be(expectedColumn);
    }

    [Fact]
    public void ResolveSortColumn_ShouldReturnNull_WhenSortFieldIsNull()
    {
        // Act
        var result = CarsRepository.ResolveSortColumn(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("id; drop table cars;--")]
    [InlineData("1=1")]
    [InlineData("brand, (select password from users)")]
    [InlineData("nonexistentcolumn")]
    public void ResolveSortColumn_ShouldReturnNull_ForUnrecognizedOrMaliciousInput(string sortField)
    {
        // Act
        var result = CarsRepository.ResolveSortColumn(sortField);

        // Assert
        result.Should().BeNull("only whitelisted column names should ever reach the SQL string");
    }
}
