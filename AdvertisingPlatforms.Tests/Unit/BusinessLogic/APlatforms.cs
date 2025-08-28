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
    }
}