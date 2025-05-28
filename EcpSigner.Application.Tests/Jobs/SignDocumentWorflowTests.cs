using EcpSigner.Application.Interfaces;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EcpSigner.Application.Jobs
{
    public class SignDocumentWorkflowTests
    {
        private readonly Mock<IPortalService> _repositoryMock;
        private readonly Mock<ISignatureService> _signatureServiceMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SignDocumentWorflow _workflow;

        public SignDocumentWorkflowTests()
        {
            _repositoryMock = new Mock<IPortalService>();
            _signatureServiceMock = new Mock<ISignatureService>();
            _loggerMock = new Mock<ILogger>();

            _workflow = new SignDocumentWorflow(
                _repositoryMock.Object,
                _signatureServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_ShouldCallAllMethodsInOrder_WhenAllConditionsAreMet()
        {
            // Arrange
            var doc = new Document { Name = "TestDoc", Num = "123", VersionNumber = 1 };
            var ecpCert = new EcpCertificate();
            var userCert = Mock.Of<ICertificate>(cert => cert.ValidToDate == DateTime.Now.AddDays(1));
            var certs = new List<(EcpCertificate, ICertificate)> { (ecpCert, userCert) };
            var cancellationToken = CancellationToken.None;

            _repositoryMock
                .Setup(r => r.CheckBeforeSign(doc, ecpCert, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.GetSignData(doc, ecpCert, It.IsAny<string>()))
                .ReturnsAsync(("docBase64", "hashBase64"));

            _signatureServiceMock
                .Setup(s => s.Sign(userCert, "docBase64", It.IsAny<string>()))
                .Returns("signature");

            _repositoryMock
                .Setup(r => r.SaveSignature(doc, "hashBase64", "signature", ecpCert, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _workflow.RunAsync(doc, certs, cancellationToken);

            // Assert
            _repositoryMock.Verify(r => r.CheckBeforeSign(doc, ecpCert, It.IsAny<string>()), Times.Once);
            _repositoryMock.Verify(r => r.GetSignData(doc, ecpCert, It.IsAny<string>()), Times.Once);
            _signatureServiceMock.Verify(s => s.Sign(userCert, "docBase64", It.IsAny<string>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveSignature(doc, "hashBase64", "signature", ecpCert, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SelectCertificate_ShouldThrowException_WhenNoValidCertificates()
        {
            // Arrange
            var certs = new List<(EcpCertificate, ICertificate)>
            {
                (new EcpCertificate(), Mock.Of<ICertificate>(cert => cert.ValidToDate == DateTime.Now.AddSeconds(-1)))
            };

            var cancellationToken = CancellationToken.None;

            // Act
            Action action = () => _workflow.GetType()
                .GetMethod("SelectCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_workflow, new object[] { certs, cancellationToken });

            // Assert
            action.Should().Throw<TargetInvocationException>().WithInnerException<Exception>()
                .WithMessage("подходящие сертификаты не найдены");
        }

        [Fact]
        public void SaveSignature_ShouldThrowStopWorkException_WhenCancellationIsRequested()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            var method = typeof(SignDocumentWorflow)
                .GetMethod("SaveSignature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Func<Task> act = async () => await (Task)method.Invoke(_workflow, [Mock.Of<Document>(), new EcpCertificate(), "signature", "hashBase64", "docName", cancellationToken]);

            // Assert
            act.Should().ThrowAsync<StopWorkException>();
        }

        [Fact]
        public void Sign_ShouldThrowStopWorkException_WhenCancellationIsRequested()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            var method = typeof(SignDocumentWorflow)
                .GetMethod("Sign", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => method.Invoke(_workflow, ["docBase64", Mock.Of<ICertificate>(), "docName", cancellationToken]);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<StopWorkException>();
        }

        [Fact]
        public void GetSignData_ShouldThrowStopWorkException_WhenCancellationIsRequested()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            var method = typeof(SignDocumentWorflow)
                .GetMethod("GetSignData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            Func<Task> act = async () => await (Task)method.Invoke(_workflow, [Mock.Of<Document>(), new EcpCertificate(), "docName", cancellationToken]);

            // Assert
            act.Should().ThrowAsync<StopWorkException>();
        }
        [Fact]
        public void SelectCertificate_ShouldThrowStopWorkException_WhenCancellationIsRequested()
        {
            // Arrange
            var certs = new List<(EcpCertificate, ICertificate)>
            {
                (new EcpCertificate(), Mock.Of<ICertificate>(cert => cert.ValidToDate == DateTime.Now.AddSeconds(-1)))
            };
            var cancellationToken = new CancellationToken(canceled: true);

            var method = typeof(SignDocumentWorflow)
                .GetMethod("SelectCertificate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => method.Invoke(_workflow, [certs, cancellationToken]);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<StopWorkException>();
        }

        [Fact]
        public void CheckBeforeSign_ShouldThrowStopWorkException_WhenCancellationIsRequested()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            var method = typeof(SignDocumentWorflow)
                .GetMethod("CheckBeforeSign", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Func<Task> act = async () => await (Task)method.Invoke(_workflow, [Mock.Of<Document>(), new EcpCertificate(), "docName", cancellationToken]);

            // Assert
            act.Should().ThrowAsync<StopWorkException>();
        }
    }
}