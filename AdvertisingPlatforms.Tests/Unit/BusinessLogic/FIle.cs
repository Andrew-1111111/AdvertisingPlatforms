using AdvertisingPlatforms.BusinessLogic.File;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace AdvertisingPlatforms.Test.BusinessLogic
{
    public class File
    {
        private readonly FileRepository _repository;

        public File()
        {
            _repository = new FileRepository();
        }

        [Fact]
        public void Instance_HasDefaultValues()
        {
            Assert.Equal(string.Empty, _repository.Name);
            Assert.Equal(string.Empty, _repository.Extension);
            Assert.False(_repository.IsUploaded);
            Assert.Equal(new byte[] { 0 }, _repository.Data);
        }

        [Fact]
        public async Task Set_New_File()
        {
            // Arrange
            var fileName = "TestCaseFile.txt";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var success = await _repository.SetFileAsync(file);

            // Assert
            Assert.True(success);
            Assert.True(_repository.IsUploaded);
            Assert.NotEmpty(_repository.Name);
            Assert.Equal(".txt", _repository.Extension);
            Assert.Equal(ms.ToArray(), _repository.Data);
        }

        [Fact]
        public async Task Validation_ValidFile_Return_True()
        {
            // Arrange
            var fileName = "TestCaseFile.txt";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Validation_WrongExtension_Return_False()
        {
            // Arrange
            var fileName = "TestCaseFile.mkv";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_ZeroLengthFile_Return_False()
        {
            // Arrange
            var fileName = "TestCaseFile.mkv";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            // Droping writing a file to a stream: await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_NullFileName_Return_False()
        {
            // Arrange
            var fileName = "TestCaseFile.mkv";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", null!);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_EmptyFileName_Return_False()
        {
            // Arrange
            var fileName = "";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_WhitespaceFileName_Return_False()
        {
            // Arrange
            var fileName = " ";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_FileNameWithInvalidChars_Return_False()
        {
            // Arrange
            var fileName = "<TestCaseFile>.txt";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_ExtensionCaseInsensitive_Return_True()
        {
            // Arrange
            var fileName = "TestCaseFile.TXT";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Validation_FileNameWithPath_Return_False()
        {
            // Arrange
            var fileName = "Path/To/TestCaseFile.txt";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Validation_FileNameWithInvalidPathChars_Return_False()
        {
            // Arrange
            var fileName = "File:TestCaseFile.txt";

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, Encoding.UTF8);
            await sw.WriteAsync(fileName);
            await sw.FlushAsync();
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "id_from_form", fileName);

            // Act
            var result = FileRepository.Validation(file);

            // Assert
            Assert.False(result);
        }
    }
}