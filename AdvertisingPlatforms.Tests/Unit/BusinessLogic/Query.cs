using AdvertisingPlatforms.BusinessLogic.Query;

namespace AdvertisingPlatforms.Test.BusinessLogic
{
    public class Query
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("  ", "")]
        [InlineData("   ", "")]
        [InlineData("/", "")]
        public void Should_Be_Return_False(string? input, string output)
        {
            // Arrange
            var query = input;

            // Act
            var success = QueryHelper.LocationValidator(ref query!);

            // Assert
            Assert.False(success);
            Assert.Equal(query, output, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData("api", "/api")]
        [InlineData("api/", "/api")]
        [InlineData("/api/", "/api")]
        [InlineData("API/USERS", "/api/users")]
        [InlineData("api/users/data", "/api/users/data")]
        [InlineData("//api", "/api")]
        [InlineData("api//users", "/api/users")]
        public void Should_Be_Return_True(string input, string output)
        {
            // Arrange
            var query = input;

            // Act
            var success = QueryHelper.LocationValidator(ref query);

            // Assert
            Assert.NotEqual(input, output, StringComparer.Ordinal);
            Assert.True(success);
        }
    }
}