using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using EcpSigner.Infrastructure.Configuration;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace EcpSigner.Infrastructure.Configuration
{
    public class JsonConfigurationProviderTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly string _testFilePath;

        public JsonConfigurationProviderTests()
        {
            _loggerMock = new Mock<ILogger>();
            _testFilePath = Path.GetTempFileName();
        }

        private void WriteSettingsToFile(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(_testFilePath, json);
        }

        [Fact]
        public void Get_ShouldReturnValidAppSettings_WhenJsonIsCorrect()
        {
            // Arrange
            var settings = new Settings
            {
                login = "user",
                password = "pass",
                url = "http://example.com",
                pauseMinutes = 10,
                cacheMinutes = 60,
                signingIntervalSeconds = 30,
                ignoreDocTypes = new List<string> { "INV", "ACT" }
            };
            WriteSettingsToFile(settings);

            var provider = new JsonConfigurationProvider(_loggerMock.Object, _testFilePath);

            // Act
            var result = provider.Get();

            // Assert
            result.login.Should().Be("user");
            result.password.Should().Be("pass");
            result.url.Should().Be("http://example.com");
            result.pauseMinutes.Should().Be(10);
            result.cacheMinutes.Should().Be(60);
            result.signingIntervalSeconds.Should().Be(30);
            result.ignoreDocTypesDict.Should().ContainKeys("INV", "ACT");
            result.ignoreDocTypesDict["INV"].Should().Be(1);
        }

        [Theory]
        [InlineData(null, "pass", "url", "login пользователя не задан")]
        [InlineData("login", null, "url", "password пользователя не задан")]
        [InlineData("login", "pass", null, "url сайта ЕЦП не задан")]
        public void Get_ShouldThrowException_WhenRequiredFieldIsMissing(string login, string password, string url, string expectedMessage)
        {
            // Arrange
            var settings = new Settings
            {
                login = login,
                password = password,
                url = url,
                pauseMinutes = 10,
                cacheMinutes = 60,
                signingIntervalSeconds = 30,
                ignoreDocTypes = new List<string>()
            };
            WriteSettingsToFile(settings);

            var provider = new JsonConfigurationProvider(_loggerMock.Object, _testFilePath);

            // Act
            Action act = () => provider.Get();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void Get_ShouldLogWarningAndFixInvalidValues()
        {
            // Arrange
            var settings = new Settings
            {
                login = "user",
                password = "pass",
                url = "http://example.com",
                pauseMinutes = 0,           // invalid
                cacheMinutes = 0,           // invalid
                signingIntervalSeconds = 100, // invalid
                ignoreDocTypes = new List<string>()
            };
            WriteSettingsToFile(settings);

            var provider = new JsonConfigurationProvider(_loggerMock.Object, _testFilePath);

            // Act
            var result = provider.Get();

            // Assert
            result.pauseMinutes.Should().Be(15);
            result.cacheMinutes.Should().Be(360);
            result.signingIntervalSeconds.Should().Be(1);

            _loggerMock.Verify(l => l.Warn("pauseMinutes задан некорректно. Установлено pauseMinutes=15"), Times.Once);
            _loggerMock.Verify(l => l.Warn("cacheMinutes задан некорректно. Установлено cacheMinutes=360"), Times.Once);
            _loggerMock.Verify(l => l.Warn("signingIntervalSeconds задан некорректно. Установлено signingIntervalSeconds=1"), Times.Once);
        }

        [Fact]
        public void Get_ShouldCacheSettings_OnSubsequentCalls()
        {
            // Arrange
            var settings = new Settings
            {
                login = "user",
                password = "pass",
                url = "http://example.com",
                pauseMinutes = 10,
                cacheMinutes = 60,
                signingIntervalSeconds = 30,
                ignoreDocTypes = new List<string>()
            };
            WriteSettingsToFile(settings);

            var provider = new JsonConfigurationProvider(_loggerMock.Object, _testFilePath);

            // Act
            var first = provider.Get();
            File.Delete(_testFilePath); // чтобы проверить, что повторный вызов не читает файл
            var second = provider.Get();

            // Assert
            second.Should().BeSameAs(first);
        }

        [Fact]
        public void Get_ShouldThrowException_WhenJsonFileIsMalformed()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "невалидный json");

            var provider = new JsonConfigurationProvider(_loggerMock.Object, _testFilePath);

            // Act
            Action act = () => provider.Get();

            // Assert
            act.Should().Throw<JsonReaderException>();
        }
        [Fact]
        public void Get_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            var provider = new JsonConfigurationProvider(_loggerMock.Object, nonExistentFilePath);

            // Act
            Action act = () => provider.Get();

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }
    }
}
