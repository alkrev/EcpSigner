using ConsoleTools;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace EcpSigner.Infrastructure.Shared.ConsoleTools
{
    public class ConsoleWrapperTests
    {
        [Fact]
        public void AddingAndRemovingHandler_ShouldNotThrow()
        {
            // Arrange
            var wrapper = new ConsoleWrapper();
            ConsoleCancelEventHandler? handler = null;

            // Act
            Action act = () =>
            {
                wrapper.CancelKeyPress += handler;
                wrapper.CancelKeyPress -= handler;
            };

            // Assert
            act.Should().NotThrow("добавление и удаление обработчика через обёртку должно быть безопасным");
        }
    }
}
