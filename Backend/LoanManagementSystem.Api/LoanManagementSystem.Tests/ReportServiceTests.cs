using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanManagementSystem.Api.DTOs.Reports;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _mockReportRepo;
        private readonly Mock<IEmiRepository> _mockEmiRepo;
        private readonly Mock<ILoanRepository> _mockLoanRepo;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _mockReportRepo = new Mock<IReportRepository>();
            _mockEmiRepo = new Mock<IEmiRepository>();
            _mockLoanRepo = new Mock<ILoanRepository>();
            _service = new ReportService(_mockReportRepo.Object, _mockEmiRepo.Object, _mockLoanRepo.Object);
        }

        [Fact]
        public async Task GetLoansByStatusAsync_ShouldReturnList_WhenCalled()
        {
            // Arrange
            var list = new List<LoansByStatusDto> { new LoansByStatusDto { Status = "Active", Count = 5 } };
            _mockReportRepo.Setup(r => r.GetLoansByStatusAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetLoansByStatusAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result.First().Status);
        }

        [Fact]
        public async Task GetOutstandingAmountAsync_ShouldReturnDto_WhenCalled()
        {
            // Arrange
            var loanId = 1;
            var expected = new OutstandingAmountDto { OutstandingAmount = 5000 };
            _mockReportRepo.Setup(r => r.GetOutstandingAmountAsync(loanId)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetOutstandingAmountAsync(loanId);

            // Assert
            Assert.Equal(5000, result.OutstandingAmount);
        }

        [Fact]
        public async Task GetMonthlyEmiReportAsync_ShouldMapEntitiesToDto()
        {
            // Arrange
            var month = 1;
            var year = 2025;
            var emis = new List<EMI>
            {
                new EMI
                {
                    LoanId = 1,
                    DueDate = new DateOnly(2025, 1, 5),
                    EMIAmount = 2000,
                    IsPaid = false,
                    LoanApplication = new LoanApplication
                    {
                        Customer = new User { FullName = "John Doe" },
                        LoanType = new LoanType { LoanTypeName = "Home" }
                    }
                }
            };

            _mockEmiRepo.Setup(r => r.GetMonthlyEmiReportAsync(month, year))
                        .ReturnsAsync(emis);

            // Act
            var result = await _service.GetMonthlyEmiReportAsync(month, year);

            // Assert
            var dto = result.First();
            Assert.Equal("John Doe", dto.CustomerName);
            Assert.Equal("Home", dto.LoanType);
            Assert.Equal(2000, dto.EmiAmount);
        }

        [Fact]
        public async Task GetEmiOverdueReportAsync_ShouldReturnValues_WhenCalled()
        {
            // Arrange
            var list = new List<EmiOverdueReportDto> { new EmiOverdueReportDto { DaysOverdue = 5 } };
            _mockReportRepo.Setup(r => r.GetEmiOverdueReportAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetEmiOverdueReportAsync();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCustomerLoanSummariesAsync_ShouldReturnList_WhenCalled()
        {
            // Arrange
            var list = new List<CustomerLoanSummaryDto>
            {
                new CustomerLoanSummaryDto
                {
                    CustomerId = 1,
                    CustomerName = "Test User",
                    ActiveLoans = 1,
                    TotalOutstanding = 10000
                }
            };
            _mockLoanRepo.Setup(r => r.GetCustomerLoanSummariesAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetCustomerLoanSummariesAsync();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Test User", item.CustomerName);
            Assert.Equal(10000, item.TotalOutstanding);
        }
    }
}
