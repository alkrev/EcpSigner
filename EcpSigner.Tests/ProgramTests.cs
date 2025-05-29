using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using EcpSigner;
using EcpSigner.Domain.Interfaces;

namespace EcpSigner
{
    public class ProgramTests
    {
        [Fact]
        public void Run_ShouldCallRunner_WhenRunnerIsProvided()
        {
            // Arrange
            var args = new[] { "arg1", "arg2" };
            var runnerMock = new Mock<IProgramRunner>();
            runnerMock.Setup(r => r.RunAsync(args)).Returns(Task.CompletedTask);

            // Act
            Program.Run(args, runnerMock.Object);

            // Assert
            runnerMock.Verify(r => r.RunAsync(args), Times.Once);
        }

        [Fact]
        public void Run_ShouldNotThrow_WhenRunnerThrows()
        {
            // Arrange
            var args = new[] { "arg1" };
            var runnerMock = new Mock<IProgramRunner>();
            runnerMock.Setup(r => r.RunAsync(args))
                      .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            Action act = () => Program.Run(args, runnerMock.Object);

            // Assert
            act.Should().NotThrow(); // исключение должно быть перехвачено в Program.Run
        }

        [Fact]
        public void Run_ShouldUseDefaultRunner_WhenNullPassed()
        {
            // Arrange
            var args = new[] { "arg1" };

            // Act
            Action act = () => Program.Run(args, null);

            // Assert
            act.Should().NotThrow(); // должен создать ProgramRunner и выполнить без исключений
        }
    }
}

