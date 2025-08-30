using AdvertisingPlatforms.BusinessLogic.APlatforms;
using System.Text;
using Xunit.Abstractions;

namespace AdvertisingPlatforms.Tests.Unit.BusinessLogic
{
    public class APlatforms()
    {
        private readonly ApPlatformsRepository _repository = new();

        [Fact]
        public void SetPlatforms_WithValidData_Return_True()
        {
            var data = Encoding.UTF8.GetBytes("Яндекс.Директ:/ru");
            var result = _repository.SetPlatforms(data, Encoding.UTF8);

            Assert.True(result);
        }

        [Fact]
        public void SetPlatforms_WithValidData_UpdatesCount()
        {
            var data = Encoding.UTF8.GetBytes("Яндекс.Директ:/ru");
            _repository.SetPlatforms(data, Encoding.UTF8);

            Assert.Equal(1, _repository.Count);
            Assert.False(_repository.IsEmpty);
        }

        [Fact]
        public void GetPlatforms_WithExistingLocation_Return_Platforms()
        {
            var data = Encoding.UTF8.GetBytes("Яндекс.Директ:/ru");
            _repository.SetPlatforms(data, Encoding.UTF8);

            var platforms = _repository.GetPlatforms("/ru");

            Assert.Contains("Яндекс.Директ", platforms);
        }

        [Fact]
        public void GetPlatforms_WithNonExistingLocation_Return_Empty()
        {
            var data = Encoding.UTF8.GetBytes("Яндекс.Директ:/ru");
            _repository.SetPlatforms(data, Encoding.UTF8);

            var platforms = _repository.GetPlatforms("/nonexistent");

            Assert.Empty(platforms);
        }

        [Fact]
        public void GetAllPlatforms_WithMultipleEntries_Return_AllData()
        {
            var testData = @"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl";

            var data = Encoding.UTF8.GetBytes(testData);
            if (_repository.SetPlatforms(data, Encoding.UTF8))
            {
                var allPlatforms = _repository.GetAllPlatforms();

                Assert.Equal(6, _repository.Count);
                Assert.Contains("/ru:Яндекс.Директ", allPlatforms);
                Assert.Contains("/ru/svrd/revda:Яндекс.Директ,Ревдинский рабочий", allPlatforms);
            }
        }

        [Fact]
        public void SetPlatforms_WithMultipleLocationsPerPlatform_SplitsCorrectly()
        {
            var testData = "Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik";
            var data = Encoding.UTF8.GetBytes(testData);
            _repository.SetPlatforms(data, Encoding.UTF8);

            var platforms1 = _repository.GetPlatforms("/ru/svrd/revda");
            var platforms2 = _repository.GetPlatforms("/ru/svrd/pervik");

            Assert.Contains("Ревдинский рабочий", platforms1);
            Assert.Contains("Ревдинский рабочий", platforms2);
            Assert.Equal(2, _repository.Count);
        }

        [Fact]
        public void SetDictionary_ValidMultiLineText_Return_True()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru\nРевдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_SingleLineText_Return_True()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_EmptyText_Return_False()
        {
            // Arrange
            var text = "";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithoutColon_Return_False()
        {
            // Arrange
            var text = "InvalidLineWithoutColon";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithEmptyLocations_Return_False()
        {
            // Arrange
            var text = "Яндекс.Директ:";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_NullText_Return_False()
        {
            // Arrange
            string? text = null;

            // Act
            var result = ApPlatformsRepository.SetDictionary(text!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithWhitespaceLines_Return_True_ForValidLines()
        {
            // Arrange
            var text = "Яндекс.Директ:/ru\n   \nРевдинский рабочий:/ru/svrd";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetDictionary_OnlyWhitespaceText_Return_False()
        {
            // Arrange
            var text = "   \n\t\n   ";

            // Act
            var result = ApPlatformsRepository.SetDictionary(text);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetDictionary_TextWithMultipleValidEntries_Return_True()
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
        public void SplitLines_ValidInput_Return_CorrectListAndSetsValue()
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
        public void SplitLines_WithSpacesAndTrimEntries_Return_TrimmedValues()
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
        public void SplitLines_EmptyString_Return_EmptyList()
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
        public void SplitLines_NullString_Return_EmptyList()
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
        public void SplitLines_OnlyKeyWithoutValues_Return_EmptyList()
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
        public void SplitLines_SingleValue_Return_SingleItemList()
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