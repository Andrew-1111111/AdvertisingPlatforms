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
        public void SetDictionary_ValidMultiLineText_ReturnsTrue()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru\nРевдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_SingleLineText_ReturnsTrue()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_EmptyText_ReturnsFalse()
        {
            // Arrange
            var text = "";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithoutColon_ReturnsFalse()
        {
            // Arrange
            var text = "InvalidLineWithoutColon";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithEmptyLocations_ReturnsFalse()
        {
            // Arrange
            var text = "Яндекс.Директ:";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_NullText_ReturnsFalse()
        {
            // Arrange
            string? text = null;

            // Act
            var result = ApPlatformsRepository.SetDictionary(text!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithWhitespaceLines_ReturnsTrueForValidLines()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru\n   \nРевдинский рабочий:/ru/svrd";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_OnlyWhitespaceText_ReturnsFalse()
        {
            // Arrange
            var text = "   \n\t\n   ";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithMultipleValidEntries_ReturnsTrue()
        {
            // Arrange
            var text = @"Яндекс.Директ:/ru 
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik 
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddNesting_ShouldAggregateNestedPaths()
        {
            // Arrange
            var tempDict = new Dictionary<string, List<string>>
            {
                { "/ru", new List<string> { "Яндекс.Директ" } },
                { "/ru/svrd/revda", new List<string> { "Ревдинский рабочий" }},
                { "/ru/svrd/pervik", new List<string> { "Ревдинский рабочий" }}
            };

            // Act
            ApPlatformsRepository.AddNesting(tempDict);

            // Assert
            Assert.Equal(3, tempDict.Count);
            Assert.Contains("Ревдинский рабочий", tempDict["/ru/svrd/revda"]);
            Assert.Contains("Ревдинский рабочий", tempDict["/ru/svrd/pervik"]);
        }

        [Fact]
        public void AddNesting_ShouldHandleParentChildRelationships()
        {
            // Arrange
            var tempDict = new Dictionary<string, List<string>>
            {
                { "/ru", new List<string> { "Яндекс.Директ" }},
                { "/ru/svrd", new List<string> { "Ревдинский рабочий" }},
                { "/ru/svrd/revda", new List<string> { "Газета" }}
            };

            // Act
            ApPlatformsRepository.AddNesting(tempDict);

            // Assert
            Assert.Equal(3, tempDict.Count);
            Assert.Contains("Ревдинский рабочий", tempDict["/ru/svrd"]);
            Assert.Contains("Газета", tempDict["/ru/svrd/revda"]);
        }

        [Fact]
        public void AddNesting_ShouldRemoveDuplicatesInValues()
        {
            // Arrange
            var tempDict = new Dictionary<string, List<string>>
            {
                { "/ru", new List<string> { "Яндекс.Директ", "Яндекс.Директ" }},
                { "/ru/svrd", new List<string> { "Ревдинский рабочий" }}
            };

            // Act
            ApPlatformsRepository.AddNesting(tempDict);

            // Assert
            Assert.Equal(2, tempDict.Count);
            Assert.Single(tempDict["/ru"]);
            Assert.Equal([ "Яндекс.Директ", "Ревдинский рабочий" ], tempDict["/ru/svrd"]);
        }

        [Fact]
        public void AddNesting_WithEmptyDictionary_ShouldDoNothing()
        {
            // Arrange
            var tempDict = new Dictionary<string, List<string>>();

            // Act
            ApPlatformsRepository.AddNesting(tempDict);

            // Assert
            Assert.Empty(tempDict);
        }

        [Fact]
        public void AddNesting_ShouldHandleSingleKey()
        {
            // Arrange
            var tempDict = new Dictionary<string, List<string>>
            {
                { "/ru", new List<string> { "Яндекс.Директ" }}
            };

            // Act
            ApPlatformsRepository.AddNesting(tempDict);

            // Assert
            Assert.Single(tempDict);
            Assert.Single(tempDict["/ru"]);
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

    }
}