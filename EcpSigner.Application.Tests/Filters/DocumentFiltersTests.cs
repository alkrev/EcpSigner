using FluentAssertions;
using EcpSigner.Domain.Models;

namespace EcpSigner.Application.Filters
{
    public class DocumentFiltersTests
    {
        [Fact]
        public void NeedsSigning_ShouldReturnTrue_WhenStatusIs2_AndErrorIsNull()
        {
            // Arrange
            var doc = new Document { SignStatus = "2", Error = null };

            // Act
            var result = DocumentFilters.NeedsSigning(doc);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void NeedsSigning_ShouldReturnFalse_WhenStatusIsNot2()
        {
            var doc = new Document { SignStatus = "1", Error = null };

            var result = DocumentFilters.NeedsSigning(doc);

            result.Should().BeFalse();
        }

        [Fact]
        public void NeedsSigning_ShouldReturnFalse_WhenErrorIsNotNullOrEmpty()
        {
            var doc = new Document { SignStatus = "2", Error = "Some error" };

            var result = DocumentFilters.NeedsSigning(doc);

            result.Should().BeFalse();
        }

        [Fact]
        public void WithError_ShouldReturnTrue_WhenStatusIs2_AndErrorIsNotEmpty()
        {
            var doc = new Document { SignStatus = "2", Error = "Error text" };

            var result = DocumentFilters.WithError(doc);

            result.Should().BeTrue();
        }

        [Fact]
        public void WithError_ShouldReturnFalse_WhenErrorIsEmpty()
        {
            var doc = new Document { SignStatus = "2", Error = "" };

            var result = DocumentFilters.WithError(doc);

            result.Should().BeFalse();
        }

        [Fact]
        public void WithError_ShouldReturnFalse_WhenStatusIsNot2()
        {
            var doc = new Document { SignStatus = "1", Error = "Error text" };

            var result = DocumentFilters.WithError(doc);

            result.Should().BeFalse();
        }

        [Fact]
        public void IgnoredDocument_ShouldReturnTrue_WhenTypeIsInIgnoreList()
        {
            var doc = new Document { Type = "TYPE_A" };
            var ignoreList = new Dictionary<string, byte> { { "TYPE_A", 1 } };

            var result = DocumentFilters.IgnoredDocument(doc, ignoreList);

            result.Should().BeTrue();
        }

        [Fact]
        public void IgnoredDocument_ShouldReturnFalse_WhenTypeIsNotInIgnoreList()
        {
            var doc = new Document { Type = "TYPE_X" };
            var ignoreList = new Dictionary<string, byte> { { "TYPE_A", 1 } };

            var result = DocumentFilters.IgnoredDocument(doc, ignoreList);

            result.Should().BeFalse();
        }
    }
}