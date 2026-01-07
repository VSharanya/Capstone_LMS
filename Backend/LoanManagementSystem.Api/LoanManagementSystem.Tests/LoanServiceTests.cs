using System;
using System.Threading.Tasks;
using AutoMapper;
using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using LoanManagementSystem.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly Mock<ILoanRepository> _mockLoanRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IEmiService> _mockEmiService;
        private readonly Mock<INotificationService> _mockNotifService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly LoanService _service;

        public LoanServiceTests()
        {
            _mockLoanRepo = new Mock<ILoanRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockEmiService = new Mock<IEmiService>();
            _mockNotifService = new Mock<INotificationService>();
            _mockMapper = new Mock<IMapper>();

            _service = new LoanService(
                _mockLoanRepo.Object,
                _mockUserRepo.Object,
                _mockEmiService.Object,
                _mockNotifService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task ApplyLoanAsync_ShouldApply_WhenValid()
        {
            // Arrange
            var customerId = 1;
            var customer = new User { UserId = customerId, Role = "Customer", AnnualIncome = 600000 }; // 50k monthly
            var loanType = new LoanType 
            { 
                LoanTypeId = 1, 
                IsActive = true, 
                MinAmount = 10000, 
                MaxAmount = 1000000, 
                MaxTenureMonths = 60,
                InterestRate = 10
            };
            var applyDto = new LoanApplyDto 
            { 
                LoanTypeId = 1, 
                LoanAmount = 100000, 
                TenureMonths = 12 
            };

            _mockUserRepo.Setup(r => r.GetUserByIdAsync(customerId)).ReturnsAsync(customer);
            _mockLoanRepo.Setup(r => r.GetLoanTypeByIdAsync(1)).ReturnsAsync(loanType);
            _mockMapper.Setup(m => m.Map<LoanApplication>(It.IsAny<LoanApplyDto>()))
                       .Returns(new LoanApplication());

            // Act
            await _service.ApplyLoanAsync(customerId, applyDto);

            // Assert
            _mockLoanRepo.Verify(r => r.AddLoanAsync(It.IsAny<LoanApplication>()), Times.Once);
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), "Info"), Times.Once);
        }

        [Fact]
        public async Task ApplyLoanAsync_ShouldThrow_WhenAmountInvalid()
        {
            // Arrange
            var customer = new User { Role = "Customer" };
            var loanType = new LoanType { MinAmount = 10000, MaxAmount = 20000, IsActive = true };
            var dto = new LoanApplyDto { LoanTypeId = 1, LoanAmount = 5000 };

            _mockUserRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(customer);
            _mockLoanRepo.Setup(r => r.GetLoanTypeByIdAsync(1)).ReturnsAsync(loanType);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.ApplyLoanAsync(1, dto));
        }

        [Fact]
        public async Task ApproveOrRejectLoanAsync_ShouldReject_WhenStatusRejected()
        {
            // Arrange
            var loanId = 1;
            var officerId = 2;
            var loan = new LoanApplication 
            { 
                LoanId = loanId, 
                Status = "Under Review", // Updated to match strict workflow
                CustomerId = 1,
                LoanType = new LoanType { LoanTypeName = "Test" },
                Customer = new User { FullName = "Test" }
            };
            var officer = new User { UserId = officerId, Role = "LoanOfficer", FullName = "Officer" };
            var statusDto = new LoanStatusUpdateDto { Status = "Rejected", Remarks = "Bad Credit" };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(officerId)).ReturnsAsync(officer);

            // Act
            await _service.ApproveOrRejectLoanAsync(loanId, officerId, statusDto);

            // Assert
            Assert.Equal("Rejected", loan.Status);
            Assert.Equal("Rejected", loan.Status);
            _mockLoanRepo.Verify(r => r.UpdateLoanAsync(loan), Times.Once);

            // ✅ Verify Notification
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.Is<string>(s => s.Contains("REJECTED")), "Error"), Times.Once);
            _mockNotifService.Verify(n => n.NotifyRoleAsync("Admin", It.Is<string>(s => s.Contains("Loan Rejected")), "Warning"), Times.Once);
        }

        [Fact]
        public async Task ApproveOrRejectLoanAsync_ShouldApproveAndGenerateEmi_WhenStatusApproved()
        {
            // Arrange
            var loanId = 1;
            var officerId = 2;
            var loan = new LoanApplication 
            { 
                LoanId = loanId, 
                Status = "Under Review", //  Updated to match strict workflow
                CustomerId = 1,
                LoanType = new LoanType { LoanTypeName = "Test" },
                Customer = new User { FullName = "Test" }
            };
            var officer = new User { UserId = officerId, Role = "LoanOfficer", FullName = "Officer" };
            var statusDto = new LoanStatusUpdateDto { Status = "Approved" };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(officerId)).ReturnsAsync(officer);

            // Act
            await _service.ApproveOrRejectLoanAsync(loanId, officerId, statusDto);

            // Assert
            Assert.Equal("Active", loan.Status); // Should become active after approval 
            Assert.Equal("Active", loan.Status); // Should become active after approval
            _mockEmiService.Verify(e => e.GenerateEmiScheduleAsync(loanId), Times.Once);

            // Verify Notification
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.Is<string>(s => s.Contains("APPROVED")), "Success"), Times.Once);
            _mockNotifService.Verify(n => n.NotifyRoleAsync("Admin", It.Is<string>(s => s.Contains("Loan Approved")), "Success"), Times.Once);
        }

        [Fact]
        public async Task ApproveOrRejectLoanAsync_ShouldThrow_WhenApprovingAppliedLoan()
        {
            // Arrange
            var loanId = 1;
            var officerId = 2;
            var loan = new LoanApplication { LoanId = loanId, Status = "Applied" };
            var officer = new User { UserId = officerId, Role = "LoanOfficer" };
            var dto = new LoanStatusUpdateDto { Status = "Approved" };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(officerId)).ReturnsAsync(officer);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _service.ApproveOrRejectLoanAsync(loanId, officerId, dto));
            Assert.Contains("must be 'Under Review' to Approve", ex.Message);
        }

        [Fact]
        public async Task ApproveOrRejectLoanAsync_ShouldThrow_WhenVerifierIsApprover()
        {
            // Arrange
            var loanId = 1;
            var officerId = 2;
            var loan = new LoanApplication 
            { 
                LoanId = loanId, 
                Status = "Under Review", 
                VerifiedBy = officerId // Same as approver
            };
            var officer = new User { UserId = officerId, Role = "LoanOfficer" };
            var dto = new LoanStatusUpdateDto { Status = "Approved" };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);
            _mockUserRepo.Setup(r => r.GetUserByIdAsync(officerId)).ReturnsAsync(officer);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _service.ApproveOrRejectLoanAsync(loanId, officerId, dto));
            Assert.Contains("Violation of segregation of duties", ex.Message);
        }

        [Fact]
        public async Task ApplyLoanAsync_ShouldThrow_WhenMoratoriumMissingForEducationLoan()
        {
            // Arrange
            var customerId = 1;
            var loanType = new LoanType 
            { 
                LoanTypeId = 1, 
                LoanTypeName = "Education Loan", 
                HasMoratorium = true,
                IsActive = true,
                MinAmount = 1000,
                MaxAmount = 100000 
            };
            var dto = new LoanApplyDto 
            { 
                LoanTypeId = 1, 
                LoanAmount = 50000, 
                TenureMonths = 24, 
                MoratoriumMonths = null 
            };

            _mockUserRepo.Setup(r => r.GetUserByIdAsync(customerId)).ReturnsAsync(new User { Role = "Customer" });
            _mockLoanRepo.Setup(r => r.GetLoanTypeByIdAsync(1)).ReturnsAsync(loanType);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _service.ApplyLoanAsync(customerId, dto));
            Assert.Contains("Moratorium months are required", ex.Message);
        }
    }
}
