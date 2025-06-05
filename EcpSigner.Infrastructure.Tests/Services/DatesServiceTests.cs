using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace EcpSigner.Infrastructure.Services
{
    public class DatesServiceTests
    {
        [Fact]
        public void GetDates_WithOneArgument_ShouldReturnSameStartAndEndDate()
        {
            // Arrange
            var args = new[] { "01.06.2024" };
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            var service = new DatesService(args, dateTimeProviderMock.Object);

            // Act
            var (startDate, endDate) = service.GetDates();

            // Assert
            startDate.Should().Be("01.06.2024");
            endDate.Should().Be("01.06.2024");
        }

        [Fact]
        public void GetDates_WithTwoArguments_ShouldReturnCorrectStartAndEndDates()
        {
            // Arrange
            var args = new[] { "01.05.2024", "31.05.2024" };
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            var service = new DatesService(args, dateTimeProviderMock.Object);

            // Act
            var (startDate, endDate) = service.GetDates();

            // Assert
            startDate.Should().Be("01.05.2024");
            endDate.Should().Be("31.05.2024");
        }

        [Fact]
        public void GetDates_WithNoArguments_ShouldCalculatePreviousMonthSameDay()
        {
            // Arrange
            var currentDate = new DateTime(2024, 05, 15);
            var expectedStartDate = new DateTime(2024, 04, 15).ToString("dd.MM.yyyy");
            var expectedEndDate = currentDate.ToString("dd.MM.yyyy");

            var args = Array.Empty<string>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(p => p.Now).Returns(currentDate);

            var service = new DatesService(args, dateTimeProviderMock.Object);

            // Act
            var (startDate, endDate) = service.GetDates();

            // Assert
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
        }

        [Fact]
        public void GetDates_WithNoArguments_ShouldHandleInvalidPreviousMonthDay_AndFallbackToLastDayPreviousMonth()
        {
            // Arrange
            var currentDate = new DateTime(2024, 03, 31); // В феврале нет 31 дня
            var expectedEndDate = "31.03.2024";
            var expectedStartDate = "29.02.2024"; // Високосный год

            var args = Array.Empty<string>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(p => p.Now).Returns(currentDate);

            var service = new DatesService(args, dateTimeProviderMock.Object);

            // Act
            var (startDate, endDate) = service.GetDates();

            // Assert
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
        }
        [Fact]
        public void GetDates_WithNoArguments_OnJanuary1_ShouldReturnDecember1PreviousYear()
        {
            // Arrange
            var currentDate = new DateTime(2024, 01, 01);
            var expectedEndDate = "01.01.2024";
            var expectedStartDate = "01.12.2023";

            var args = Array.Empty<string>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(p => p.Now).Returns(currentDate);

            var service = new DatesService(args, dateTimeProviderMock.Object);

            // Act
            var (startDate, endDate) = service.GetDates();

            // Assert
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
        }
    }
}
