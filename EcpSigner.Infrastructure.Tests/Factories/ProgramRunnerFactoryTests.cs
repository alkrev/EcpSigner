using EcpSigner.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class ProgramRunnerFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnProgramRunnerInstance()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var configPath = "dummyConfigPath";
            var factory = new ProgramRunnerFactory();

            // Act
            var result = factory.Create(configPath, mockLogger.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IProgramRunner>();
        }
    }
}
