using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LoanManagementSystem.Api.Controllers;
using LoanManagementSystem.Api.DTOs.Documents;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class DocumentServiceTests
    {
        private readonly Mock<IDocumentRepository> _mockRepo;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<INotificationService> _mockNotifService;
        private readonly Mock<ILoanRepository> _mockLoanRepo;
        private readonly DocumentService _service;

        public DocumentServiceTests()
        {
            _mockRepo = new Mock<IDocumentRepository>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockNotifService = new Mock<INotificationService>();
            _mockLoanRepo = new Mock<ILoanRepository>();
            
            // Mock WebRootPath to a temp path
            _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _service = new DocumentService(_mockRepo.Object, _mockEnv.Object, _mockNotifService.Object, _mockLoanRepo.Object);
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldThrow_WhenFileIsNull()
        {
            var dto = new DocumentUploadDto { File = null };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadDocumentAsync(dto));
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldThrow_WhenFileIsEmpty()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var dto = new DocumentUploadDto { File = mockFile.Object };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadDocumentAsync(dto));
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldCallRepo_WhenValid()
        {
            // Arrange
            var content = "Hello World";
            var fileName = "test.txt";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(stream.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<System.Threading.CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var dto = new DocumentUploadDto 
            { 
                File = mockFile.Object, 
                LoanApplicationId = 1, 
                DocumentType = "Identity Proof" 
            };

            // ✅ Mock Loan for Notification
            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(1))
                .ReturnsAsync(new LoanApplication 
                { 
                    LoanId = 1, 
                    LoanType = new LoanType { LoanTypeName = "Personal Loan" } 
                });

            // Act
            var result = await _service.UploadDocumentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test.txt", result.OriginalFileName);
            _mockRepo.Verify(r => r.AddDocumentAsync(It.IsAny<LoanDocument>()), Times.Once);
            _mockNotifService.Verify(n => n.NotifyRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentsByLoanIdAsync_ShouldReturnList()
        {
            // Arrange
            var list = new List<LoanDocument> { new LoanDocument { DocumentId = 1 } };
            _mockRepo.Setup(r => r.GetDocumentsByLoanIdAsync(1)).ReturnsAsync(list);

            // Act
            var result = await _service.GetDocumentsByLoanIdAsync(1);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_ShouldReturnDoc()
        {
            // Arrange
            var doc = new LoanDocument { DocumentId = 1 };
            _mockRepo.Setup(r => r.GetDocumentByIdAsync(1)).ReturnsAsync(doc);

            // Act
            var result = await _service.GetDocumentByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DocumentId);
        }

        [Fact]
        public async Task GetDocumentFileAsync_ShouldReturnNull_WhenDocNotFound()
        {
            _mockRepo.Setup(r => r.GetDocumentByIdAsync(1)).ReturnsAsync((LoanDocument?)null);
            var result = await _service.GetDocumentFileAsync(1);
            Assert.Null(result);
        }
    }
}
