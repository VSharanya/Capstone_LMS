using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LoanManagementSystem.Api.DTOs.EMI;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using LoanManagementSystem.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class EmiServiceTests
    {
        private readonly Mock<IEmiRepository> _mockEmiRepo;
        private readonly Mock<ILoanRepository> _mockLoanRepo;
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<INotificationService> _mockNotifService; 
        private readonly Mock<IMapper> _mockMapper;
        private readonly EmiService _service;

        public EmiServiceTests()
        {
            _mockEmiRepo = new Mock<IEmiRepository>();
            _mockLoanRepo = new Mock<ILoanRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockNotifService = new Mock<INotificationService>(); 
            _mockMapper = new Mock<IMapper>();

            _service = new EmiService(
                _mockEmiRepo.Object,
                _mockLoanRepo.Object,
                _mockPaymentRepo.Object,
                _mockNotifService.Object, 
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GenerateEmiScheduleAsync_ShouldGenerate_WhenApproved()
        {
            // Arrange
            var loan = new LoanApplication
            {
                LoanId = 1,
                Status = "Approved",
                LoanAmount = 100000,
                TenureMonths = 12,
                LoanType = new LoanType { InterestRate = 10 },
                EmiStartDate = new DateOnly(2025, 1, 1)
            };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(1)).ReturnsAsync(loan);

            // Act
            await _service.GenerateEmiScheduleAsync(1);

            // Assert
            // Assert
            _mockEmiRepo.Verify(r => r.AddRangeAsync(It.Is<List<EMI>>(l => 
                l.Count == 12 && 
                l.All(e => e.EMIAmount % 1 == 0) 
            )), Times.Once);
            _mockEmiRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GenerateEmiScheduleAsync_ShouldThrow_WhenNotApproved()
        {
            // Arrange
            var loan = new LoanApplication { Status = "Applied" };
            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(1)).ReturnsAsync(loan);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.GenerateEmiScheduleAsync(1));
        }

        [Fact]
        public async Task MarkEmiAsPaidAsync_ShouldPayAndCloseLoan_WhenLastEmi()
        {
            // Arrange
            var emiId = 1;
            var loanId = 10;
            var emi = new EMI { EMIId = emiId, LoanId = loanId, IsPaid = false, EMIAmount = 5000 };
            var loan = new LoanApplication 
            {
                LoanId = loanId, 
                Status = "Active",
                CustomerId = 101, 
                LoanType = new LoanType { LoanTypeName = "Personal Loan" }, 
                EMIs = new List<EMI> 
                { 
                    new EMI { EMIId = emiId, IsPaid = true }, 
                    new EMI { EMIId = 2, IsPaid = true } 
                }
            };

            _mockEmiRepo.Setup(r => r.GetEmiByIdAsync(emiId)).ReturnsAsync(emi);
            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);

            // Act
            await _service.MarkEmiAsPaidAsync(emiId, "UPI"); 

            // Assert
            _mockEmiRepo.Verify(r => r.UpdateEmiAsync(emi), Times.Once);
            _mockPaymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
            
            // Check loan closure
            Assert.Equal("Closed", loan.Status);
            _mockLoanRepo.Verify(r => r.UpdateLoanAsync(loan), Times.Once);

            // ✅ Verify Notifications
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.Is<string>(s => s.Contains("Payment received")), "Success"), Times.Once);
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.Is<string>(s => s.Contains("CLOSED")), "Success"), Times.Once);
        }

         [Fact]
        public async Task GetForeclosureAmountAsync_ShouldCalculateCorrectly()
        {
            // Arrange
            var loanId = 1;
            var loan = new LoanApplication
            {
                LoanId = loanId,
                Status = "Active",
                LoanAmount = 10000,
                LoanType = new LoanType { InterestRate = 12 }, 
                EMIs = new List<EMI>
                {
                    // Case: 1 EMI paid, others unpaid
                    new EMI { InstallmentNumber = 1, IsPaid = true, EMIAmount = 888m }, 
                    new EMI { InstallmentNumber = 2, IsPaid = false, EMIAmount = 888m } 
                }
            };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);

            // Act
            var amount = await _service.GetForeclosureAmountAsync(loanId);

            // Assert
            Assert.True(amount > 0);
        }

        [Fact]
        public async Task PayFullOutstandingAsync_ShouldCloseLoan()
        {
            // Arrange
            var loanId = 1;
            var loan = new LoanApplication
            {
                LoanId = loanId,
                Status = "Active",
                LoanAmount = 1000,
                LoanType = new LoanType { InterestRate = 10 },
                EMIs = new List<EMI>
                {
                    new EMI { EMIId = 101, IsPaid = false, InstallmentNumber = 1 },
                    new EMI { EMIId = 102, IsPaid = false, InstallmentNumber = 2 }
                }
            };

            _mockLoanRepo.Setup(r => r.GetLoanByIdAsync(loanId)).ReturnsAsync(loan);

            // Act
            await _service.PayFullOutstandingAsync(loanId, "NetBanking"); // 

            // Assert
            _mockEmiRepo.Verify(r => r.RemoveRangeAsync(It.IsAny<List<EMI>>()), Times.Once);
            Assert.Equal("Closed", loan.Status);
            
            // Verify Notification
            _mockNotifService.Verify(n => n.CreateNotificationAsync(It.IsAny<int>(), It.Is<string>(s => s.Contains("Foreclosure successful")), "Success"), Times.Once);
        }
    }
}
