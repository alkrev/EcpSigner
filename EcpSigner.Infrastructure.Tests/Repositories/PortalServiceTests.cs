using Xunit;
using Moq;
using FluentAssertions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Domain.Exceptions;
using Ecp.Portal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace EcpSigner.Infrastructure.Repositories
{
    public class PortalServiceTests
    {
        private readonly Mock<IMain> _mainMock;
        private readonly Mock<IEMD> _emdMock;
        private readonly PortalService _sut;

        public PortalServiceTests()
        {
            _mainMock = new Mock<IMain>();
            _emdMock = new Mock<IEMD>();
            _sut = new PortalService(_mainMock.Object, _emdMock.Object);
        }

        [Fact]
        public async Task Login_Should_ThrowBreakWorkException_When_LoginFails()
        {
            // Arrange
            _mainMock.Setup(m => m.Login("user", "pass"))
                     .ReturnsAsync(new loginReply { success = false, Error_Msg = "Invalid credentials" });

            // Act
            var act = async () => await _sut.Login("user", "pass");

            // Assert
            await act.Should().ThrowAsync<BreakWorkException>()
                     .WithMessage("Invalid credentials");
        }

        [Fact]
        public async Task Login_Should_NotThrow_When_LoginSucceeds()
        {
            _mainMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(new loginReply { success = true });

            await _sut.Invoking(s => s.Login("login", "pass"))
                      .Should().NotThrowAsync();
        }

        [Fact]
        public async Task SearchDocuments_Should_ReturnAllPages()
        {
            // Arrange
            var token = CancellationToken.None;
            var reply = new List<loadEMDSignBundleWindowReply>
        {
            new loadEMDSignBundleWindowReply { EMDRegistry_ObjectID = "id1", Document_Name = "Doc1" }
        };

            _emdMock.SetupSequence(e => e.loadEMDSignBundleWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(reply)
                    .ReturnsAsync(new List<loadEMDSignBundleWindowReply>());

            // Act
            var result = await _sut.SearchDocuments("2020-01-01", "2020-01-31", token);

            // Assert
            result.Should().HaveCount(1);
            result[0].ID.Should().Be("id1");
        }

        [Fact]
        public async Task SearchDocuments_Should_ThrowIsNotLoggedInException_When_NotLoggedInExceptionThrown()
        {
            _emdMock.Setup(e => e.loadEMDSignBundleWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(new NotLoggedInException("Not logged in"));

            Func<Task> act = async () => await _sut.SearchDocuments("2020-01-01", "2020-01-31", CancellationToken.None);

            await act.Should().ThrowAsync<IsNotLoggedInException>()
                     .WithMessage("Not logged in");
        }

        [Fact]
        public async Task LoadEcpCertificates_Should_ConvertCertificatesCorrectly()
        {
            var certReply = new loadEMDCertificateListReply
            {
                EMDCertificate_id = "cert1",
                EMDCertificate_SHA1 = "0xabc123"
            };

            _emdMock.Setup(e => e.loadEMDCertificateList())
                    .ReturnsAsync(new List<loadEMDCertificateListReply> { certReply });

            var result = await _sut.LoadEcpCertificates();

            result.Should().HaveCount(1);
            result[0].ID.Should().Be("cert1");
            result[0].thumbprint.Should().Be("00ABC123");
        }

        [Fact]
        public async Task CheckBeforeSign_Should_ThrowDocumentSigningException_When_ReplyFails()
        {
            _emdMock.Setup(e => e.checkBeforeSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new checkBeforeSignReply { success = false, Error_Msg = "Cannot sign" });

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.CheckBeforeSign(doc, cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>()
                     .WithMessage("Cannot sign");
        }

        [Fact]
        public async Task CheckBeforeSign_ShouldNotThrow_WhenSuccess()
        {
            _emdMock.Setup(e => e.checkBeforeSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new checkBeforeSignReply { success = true });

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.CheckBeforeSign(doc, cert, "doc");

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetSignData_Should_ReturnTuple_When_Valid()
        {
            _emdMock.Setup(e => e.getEMDVersionSignData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(new getEMDVersionSignDataReply
                    {
                        success = true,
                        toSign = [new() { docBase64 = "doc", hashBase64 = "hash" }]
                    });

            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert1" };

            var (docBase64, hashBase64) = await _sut.GetSignData(doc, cert, "doc");

            docBase64.Should().Be("doc");
            hashBase64.Should().Be("hash");
        }

        [Fact]
        public async Task GetSignData_Should_Throw_When_IsNotSuccess()
        {
            _emdMock.Setup(e => e.getEMDVersionSignData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(new getEMDVersionSignDataReply
                    {
                        success = false,
                        Error_Msg = "getEMDVersionSignData error"
                    });

            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.GetSignData(doc, cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>()
                     .WithMessage("getEMDVersionSignData error");
        }

        [Fact]
        public async Task GetSignData_Should_Throw_When_ToSignIsEmpty()
        {
            _emdMock.Setup(e => e.getEMDVersionSignData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(new getEMDVersionSignDataReply
                    {
                        success = true,
                        toSign = new Tosign[0]
                    });

            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.GetSignData(doc, cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>()
                     .WithMessage("GetSignData: toSign.Length = 0");
        }

        [Fact]
        public async Task SaveSignature_Should_Throw_When_ReplyFails()
        {
            _emdMock.Setup(e => e.saveEMDSignatures(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new saveEMDSignaturesReply { success = false, Error_Msg = "Save failed" });

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.SaveSignature(doc, "hash", "sig", cert, "doc");

            await act.Should().ThrowAsync<DocumentSigningException>()
                     .WithMessage("Save failed");
        }

        [Fact]
        public async Task SaveSignature_ShouldNotThrow_WhenSuccess()
        {
            _emdMock.Setup(e => e.saveEMDSignatures(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new saveEMDSignaturesReply { success = true, Error_Msg = "Save success" });

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.SaveSignature(doc, "hash", "sig", cert, "doc");

            await act.Should().NotThrowAsync();
        }
        [Fact]
        public async Task GetSignData_Should_ThrowIsNotLoggedInException_When_NotLoggedInExceptionThrown()
        {
            _emdMock.Setup(e => e.getEMDVersionSignData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .ThrowsAsync(new NotLoggedInException("Session expired"));

            var doc = new Document { Type = "type", ID = "id", VersionNumber = 1 };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.GetSignData(doc, cert, "doc");

            await act.Should().ThrowAsync<IsNotLoggedInException>()
                     .WithMessage("Session expired");
        }
        [Fact]
        public async Task CheckBeforeSign_Should_ThrowIsNotLoggedInException_When_NotLoggedInExceptionThrown()
        {
            _emdMock.Setup(e => e.checkBeforeSign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new NotLoggedInException("Auth error"));

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.CheckBeforeSign(doc, cert, "doc");

            await act.Should().ThrowAsync<IsNotLoggedInException>()
                     .WithMessage("Auth error");
        }
        [Fact]
        public async Task SaveSignature_Should_ThrowIsNotLoggedInException_When_NotLoggedInExceptionThrown()
        {
            _emdMock.Setup(e => e.saveEMDSignatures(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new NotLoggedInException("Token expired"));

            var doc = new Document { Type = "type", ID = "id", VersionID = "v1" };
            var cert = new EcpCertificate { ID = "cert1" };

            Func<Task> act = async () => await _sut.SaveSignature(doc, "hash", "sig", cert, "doc");

            await act.Should().ThrowAsync<IsNotLoggedInException>()
                     .WithMessage("Token expired");
        }
    }
}
