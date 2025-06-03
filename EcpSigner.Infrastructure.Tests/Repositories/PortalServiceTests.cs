using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using EcpSigner.Infrastructure.Repositories;
using Ecp.Portal;
using EcpSigner.Domain.Models;
using EcpSigner.Domain.Exceptions;
using System.Collections.Generic;
using System.Threading;


namespace EcpSigner.Infrastructure.Repositories
{
    public class PortalServiceTests
    {
        private readonly Mock<IMain> _mainMock;
        private readonly Mock<IEMD> _emdMock;
        private readonly PortalService _service;

        public PortalServiceTests()
        {
            _mainMock = new Mock<IMain>();
            _emdMock = new Mock<IEMD>();
            _service = new PortalService(_mainMock.Object, _emdMock.Object);
        }

        [Fact]
        public async Task Login_ShouldSucceed_WhenReplyIsSuccessful()
        {
            // Arrange
            var loginReply = new loginReply { success = true };
            _mainMock.Setup(m => m.Login("user", "pass")).ReturnsAsync(loginReply);

            // Act
            Func<Task> act = async () => await _service.Login("user", "pass");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Login_ShouldThrowBreakWorkException_WhenLoginFails()
        {
            // Arrange
            var loginReply = new loginReply { success = false, Error_Msg = "Login failed" };
            _mainMock.Setup(m => m.Login("user", "pass")).ReturnsAsync(loginReply);

            // Act
            Func<Task> act = async () => await _service.Login("user", "pass");

            // Assert
            await act.Should().ThrowAsync<BreakWorkException>()
                .WithMessage("Login failed");
        }

        [Fact]
        public async Task SearchDocuments_ShouldReturnDocuments_WhenSuccessful()
        {
            // Arrange
            var docsPage = new List<loadEMDSignBundleWindowReply>
        {
            new loadEMDSignBundleWindowReply { Document_Name = "Doc1", EMDRegistry_ObjectID = "1" }
        };
            _emdMock.SetupSequence(e => e.loadEMDSignBundleWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(docsPage)
                .ReturnsAsync(new List<loadEMDSignBundleWindowReply>());

            // Act
            var result = await _service.SearchDocuments("2024-01-01", "2024-01-31", CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].ID.Should().Be("1");
        }

        [Fact]
        public async Task SearchDocuments_ShouldThrowStopWorkException_WhenCancelled()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () => await _service.SearchDocuments("2024-01-01", "2024-01-31", cts.Token);

            // Assert
            await act.Should().ThrowAsync<StopWorkException>();
        }

        [Fact]
        public async Task LoadEcpCertificates_ShouldReturnConvertedCertificates()
        {
            // Arrange
            var certs = new List<loadEMDCertificateListReply>
        {
            new loadEMDCertificateListReply { EMDCertificate_id = "id1", EMDCertificate_SHA1 = "0xABC" }
        };
            _emdMock.Setup(e => e.loadEMDCertificateList()).ReturnsAsync(certs);

            // Act
            var result = await _service.LoadEcpCertificates();

            // Assert
            result.Should().HaveCount(1);
            result[0].ID.Should().Be("id1");
            result[0].thumbprint.Should().Be("00ABC");
        }

        [Fact]
        public async Task CheckBeforeSign_ShouldThrowDocumentSigningException_WhenReplyFails()
        {
            // Arrange
            var reply = new checkBeforeSignReply { success = false, Error_Msg = "Error" };
            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert" };
            _emdMock.Setup(e => e.checkBeforeSign("type", "id", "cert", "v1")).ReturnsAsync(reply);

            // Act
            Func<Task> act = async () => await _service.CheckBeforeSign(doc, cert, "doc");

            // Assert
            await act.Should().ThrowAsync<DocumentSigningException>().WithMessage("Error");
        }

        [Fact]
        public async Task GetSignData_ShouldReturnData_WhenSuccessful()
        {
            // Arrange
            var rep = new getEMDVersionSignDataReply
            {
                success = true,
                toSign = new[] { new Tosign { docBase64 = "doc", hashBase64 = "hash" } }
            };
            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert" };
            _emdMock.Setup(e => e.getEMDVersionSignData("type", "id", "cert", 1)).ReturnsAsync(rep);

            // Act
            var result = await _service.GetSignData(doc, cert, "doc");

            // Assert
            result.docBase64.Should().Be("doc");
            result.hashBase64.Should().Be("hash");
        }

        [Fact]
        public async Task GetSignData_ShouldThrow_WhenToSignIsEmpty()
        {
            var rep = new getEMDVersionSignDataReply { success = true, toSign = new Tosign[0] };
            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert" };
            _emdMock.Setup(e => e.getEMDVersionSignData("type", "id", "cert", 1)).ReturnsAsync(rep);

            Func<Task> act = async () => await _service.GetSignData(doc, cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>().WithMessage("GetSignData: toSign.Length = 0");
        }

        [Fact]
        public async Task SaveSignature_ShouldThrow_WhenSaveFails()
        {
            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert" };
            _emdMock.Setup(e => e.saveEMDSignatures("type", "id", "v1", "hash", "sig", "cert"))
                .ReturnsAsync(new saveEMDSignaturesReply { success = false, Error_Msg = "save failed" });

            Func<Task> act = async () => await _service.SaveSignature(doc, "hash", "sig", cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>().WithMessage("save failed");
        }
    }
}
