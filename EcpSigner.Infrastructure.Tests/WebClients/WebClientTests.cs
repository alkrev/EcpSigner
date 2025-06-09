using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecp.Web;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Infrastructure.WebClients;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcpSigner.Infrastructure.WebClients
{
    public class WebClientTests
    {
        private readonly Mock<IClient> _mockInnerClient;
        private readonly WebClient _webClient;

        public WebClientTests()
        {
            _mockInnerClient = new Mock<IClient>();
            _webClient = new WebClient(_mockInnerClient.Object);
        }

        [Fact]
        public async Task PostJson_Should_Call_Inner_Client_And_Return_Result()
        {
            // Arrange
            var url = "https://example.com/api";
            var parameters = new Dictionary<string, string> { { "key", "value" } };
            var referer = "https://referer.com";
            var expectedResult = "ok";

            _mockInnerClient
                .Setup(c => c.PostJson<string>(url, parameters, referer))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _webClient.PostJson<string>(url, parameters, referer);

            // Assert
            result.Should().Be(expectedResult);
            _mockInnerClient.Verify(c => c.PostJson<string>(url, parameters, referer), Times.Once);
        }

        [Fact]
        public async Task PostJson_Should_Throw_ContinueExceptionWithError_When_Inner_Client_Throws()
        {
            // Arrange
            var url = "https://example.com/api";
            var parameters = new Dictionary<string, string>();
            var referer = "https://referer.com";
            var innerException = new InvalidOperationException("Something went wrong");

            _mockInnerClient
                .Setup(c => c.PostJson<object>(url, parameters, referer))
                .ThrowsAsync(innerException);

            // Act
            Func<Task> act = async () => await _webClient.PostJson<object>(url, parameters, referer);

            // Assert
            var ex = await act.Should().ThrowAsync<ContinueExceptionWithError>();
            ex.Which.Message.Should().Be(innerException.Message);
        }
    }
}
