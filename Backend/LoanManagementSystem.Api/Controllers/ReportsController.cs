using LoanManagementSystem.Api.DTOs.Reports;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "LoanOfficer, Admin")] 
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Retrieves statistical breakdown of loans grouped by their current status.
        [HttpGet("loans-by-status")]
        public async Task<IActionResult> LoansByStatus()
        {
            return Ok(await _reportService.GetLoansByStatusAsync());
        }

        // Calculates the total outstanding balance for a specific loan.
        [HttpGet("outstanding/{loanId}")]
        public async Task<IActionResult> OutstandingByLoan(int loanId)
        {
            return Ok(await _reportService.GetOutstandingAmountAsync(loanId));
        }

        // Generates a monthly EMI collection report for a specific period.
        [HttpGet("monthly-emi")]
        public async Task<IActionResult> MonthlyEmiReport(
            [FromQuery] MonthlyEmiReportRequestDto dto)
        {
            return Ok(await _reportService.GetMonthlyEmiReportAsync(dto.Month, dto.Year));
        }

        // Retrieves a report of all overdue EMIs.
        [HttpGet("emi-overdue")]
        public async Task<IActionResult> GetEmiOverdueReport()
        {
            var report = await _reportService.GetEmiOverdueReportAsync();
            return Ok(report);
        }

        // Retrieves a statistical summary for each customer using LINQ aggregation.
        [HttpGet("customer-summary")]
        public async Task<IActionResult> GetCustomerLoanSummaries()
        {
            var report = await _reportService.GetCustomerLoanSummariesAsync();
            return Ok(report);
        }
    }
}
