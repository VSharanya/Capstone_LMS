using System;
using System.Threading.Tasks;
using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class LoanTypeServiceTests
    {
        private readonly Mock<ILoanTypeRepository> _mockLoanTypeRepo;
        private readonly Mock<ILoanRepository> _mockLoanRepo;
        private readonly LoanTypeService _service;

        public LoanTypeServiceTests()
        {
            _mockLoanTypeRepo = new Mock<ILoanTypeRepository>();
            _mockLoanRepo = new Mock<ILoanRepository>();
            _service = new LoanTypeService(_mockLoanTypeRepo.Object, _mockLoanRepo.Object);
        }

        [Fact]
        public async Task CreateLoanTypeAsync_ShouldAddLoanType_WhenValid()
        {
            // Arrange
            var dto = new LoanTypeCreateDto
            {
                LoanTypeName = "Home Loan",
                InterestRate = 8.5m,
                MinAmount = 100000,
                MaxAmount = 5000000,
                MaxTenureMonths = 240,
                IsActive = true
            };

            _mockLoanTypeRepo.Setup(r => r.ExistsByNameAsync(dto.LoanTypeName))
                             .ReturnsAsync(false);

            // Act
            var result = await _service.CreateLoanTypeAsync(dto, 1);

            // Assert
            Assert.NotNull(result);
            _mockLoanTypeRepo.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Once);
        }

        [Fact]
        public async Task CreateLoanTypeAsync_ShouldThrow_WhenNameExists()
        {
            var dto = new LoanTypeCreateDto { LoanTypeName = "Existing" };
            _mockLoanTypeRepo.Setup(r => r.ExistsByNameAsync(dto.LoanTypeName))
                             .ReturnsAsync(true);

            await Assert.ThrowsAsync<ApplicationException>(() => _service.CreateLoanTypeAsync(dto, 1));
        }

        [Fact]
        public async Task CreateLoanTypeAsync_ShouldThrow_WhenLimitsInvalid()
        {
            var dto = new LoanTypeCreateDto
            {
                LoanTypeName = "Test",
                MinAmount = 5000,
                MaxAmount = 1000, // Invalid
                InterestRate = 5
            };

            await Assert.ThrowsAsync<ApplicationException>(() => _service.CreateLoanTypeAsync(dto, 1));
        }

        [Fact]
        public async Task UpdateLoanTypeAsync_ShouldUpdate_WhenValid()
        {
            // Arrange
            var loanTypeId = 1;
            var loanType = new LoanType { LoanTypeId = loanTypeId, IsActive = true };
            var dto = new LoanTypeUpdateDto { IsActive = true, InterestRate = 10, MaxTenureMonths = 12, MinAmount=100, MaxAmount=2000 };

            _mockLoanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                             .ReturnsAsync(loanType);

            // Act
            await _service.UpdateLoanTypeAsync(loanTypeId, dto, 1);

            // Assert
            _mockLoanTypeRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateLoanTypeAsync_ShouldThrow_WhenDeactivatingWithActiveLoans()
        {
            // Arrange
            var loanTypeId = 1;
            var loanType = new LoanType { LoanTypeId = loanTypeId, IsActive = true };
            var dto = new LoanTypeUpdateDto { IsActive = false, InterestRate = 10, MaxTenureMonths=12, MinAmount=100, MaxAmount=2000 };

            _mockLoanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);
            _mockLoanRepo.Setup(r => r.AnyActiveLoansForLoanTypeAsync(loanTypeId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.UpdateLoanTypeAsync(loanTypeId, dto, 1));
        }
    }
}
