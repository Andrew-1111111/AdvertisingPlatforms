using AdvertisingPlatforms.BusinessLogic.File;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;

namespace AdvertisingPlatforms.Test.BusinessLogic
{
    public class File
    {
        private readonly FileRepository _repository;
        private readonly Mock<IFormFile> _fileMock;

        public File()
        {
            _repository = new FileRepository();
            _fileMock = new Mock<IFormFile>();
        }

        [Fact]
        public void NewInstance_HasDefaultValues()
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

            //_fileMock.Setup(_ => _.Name).Returns(fileName);
            //_fileMock.Setup(_ => _.Length).Returns(ms.Length);
            //_fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            //var file = _fileMock.Object;
            ////_fileMock
            ////    .Setup(file => file.CopyToAsync(It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
            ////    .Callback<Stream, CancellationToken>(async (destination, token) =>
            ////    {
            ////        await ms.CopyToAsync(destination, token);
            ////        ms.Position = 0;
            ////    })
            ////    .Returns(Task.CompletedTask);

            // Act
            var success = await _repository.SetFileAsync(file);

            // Assert
            Assert.True(success);
            Assert.True(_repository.IsUploaded);
            Assert.NotEmpty(_repository.Name);
            Assert.Equal(".txt", _repository.Extension);
            Assert.Equal(ms.ToArray(), _repository.Data);
        }

    }
}