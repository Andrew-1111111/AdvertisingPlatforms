using AdvertisingPlatforms.BusinessLogic.APlatforms;

namespace AdvertisingPlatforms.Tests.Unit.BusinessLogic
{
    public class APlatforms
    {
        private readonly ApPlatformsRepository _repository;

        public APlatforms()
        {
            _repository = new ApPlatformsRepository();
        }

        [Fact]
        public void CleanList_ValidList_Equal()
        {
            // Arrange
            var input = new List<string>() { "item0", "item1", "item2" };
            var originalImput = input.ToList();

            // Act
            var result = ApPlatformsRepository.CleanList(input);

            // Assert
            Assert.Equal(originalImput, result);
        }

        [Fact]
        public void CleanList_ValidList_NotEqual()
        {
            // Arrange
            var input = new List<string>() { "Item1 ", " Item2", "ITEM3" };
            var originalImput = input.ToList();

            // Act
            ApPlatformsRepository.CleanList(input);

            // Assert
            Assert.NotEqual(originalImput, input);
        }

        [Fact]
        public void SplitLines_ValidInput_ReturnsCorrectListAndSetsValue()
        {
            // Arrange
            var singleLine = "Key: value1, value2, value3";
            var value = "";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Equal("Key", value);
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result[0]);
            Assert.Equal("value2", result[1]);
            Assert.Equal("value3", result[2]);
        }

        [Fact]
        public void SplitLines_WithSpacesAndTrimEntries_ReturnsTrimmedValues()
        {
            // Arrange
            var singleLine = "  Key  :  value1  ,  value2  ,  value3  ";
            var value = "";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Equal("Key", value);
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result[0]);
            Assert.Equal("value2", result[1]);
            Assert.Equal("value3", result[2]);
        }

        [Fact]
        public void SplitLines_EmptyEntries_AreRemoved()
        {
            // Arrange
            var singleLine = "Key: value1,,value2, ,value3,";
            var value = "";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Equal("Key", value);
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result[0]);
            Assert.Equal("value2", result[1]);
            Assert.Equal("value3", result[2]);
        }

        [Fact]
        public void SplitLines_EmptyString_ReturnsEmptyList()
        {
            // Arrange
            var singleLine = "";
            var value = "originalValue";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Empty(result);
            Assert.Equal("originalValue", value);
        }

        [Fact]
        public void SplitLines_NullString_ReturnsEmptyList()
        {
            // Arrange
            string? singleLine = null;
            var value = "originalValue";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine!, ref value);

            // Assert
            Assert.Empty(result);
            Assert.Equal("originalValue", value);
        }

        [Fact]
        public void SplitLines_OnlyKeyWithoutValues_ReturnsEmptyList()
        {
            // Arrange
            var singleLine = "Key:";
            var value = "";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Equal("Key", value);
            Assert.Empty(result);
        }

        [Fact]
        public void SplitLines_SingleValue_ReturnsSingleItemList()
        {
            // Arrange
            var singleLine = "Key: single_value";
            var value = "";

            // Act
            var result = ApPlatformsRepository.SplitLines(singleLine, ref value);

            // Assert
            Assert.Equal("Key", value);
            Assert.Single(result);
            Assert.Equal("single_value", result[0]);
        }
    }
}