using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Exceptions
{
    public class DocumentSigningTests
    {
        [Fact]
        public void IsNotLoggedInException_Should_Store_Message()
        {
            var message = "Token is invalid";
            var exception = new IsNotLoggedInException(message);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void BreakWorkException_Should_Store_Message()
        {
            var message = "Critical failure";
            var exception = new BreakWorkException(message);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void StopWorkException_Should_Have_Default_Message()
        {
            var exception = new StopWorkException();
            exception.Message.Should().Be("Exception of type 'EcpSigner.Domain.Exceptions.StopWorkException' was thrown.");
        }

        [Fact]
        public void ContinueException_Should_Store_Message()
        {
            var message = "No work to do";
            var exception = new ContinueException(message);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void ContinueExceptionWithError_Should_Store_Message()
        {
            var message = "Handled error occurred";
            var exception = new ContinueExceptionWithError(message);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void DocumentSigningException_Should_Store_Message()
        {
            var message = "Signing failed";
            var exception = new DocumentSigningException(message);
            exception.Message.Should().Be(message);
        }
    }
}
