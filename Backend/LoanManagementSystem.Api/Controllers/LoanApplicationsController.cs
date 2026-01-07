using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/loans")]
    [Authorize]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanApplicationsController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        // Submits a new loan application for the authenticated customer.
        [HttpPost("apply")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ApplyLoan(LoanApplyDto dto)
        {
            var customerId = int.Parse(User.FindFirst("UserId")!.Value);
            var loanId = await _loanService.ApplyLoanAsync(customerId, dto);
            return Ok(new { message = "Loan applied successfully", loanId });

        }

        // Retrieves all loans belonging to the authenticated customer.
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyLoans()
        {
            var customerId = int.Parse(User.FindFirst("UserId")!.Value);
            return Ok(await _loanService.GetLoansByCustomerIdAsync(customerId));
        }

        // Retrieves all loan applications in the system (Admin/Officer only).
        [HttpGet]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> GetAllLoans()
        {
            return Ok(await _loanService.GetAllLoansAsync());
        }

        // Retrieves details of a specific loan by its ID.
        [HttpGet("{loanId}")]
        public async Task<IActionResult> GetLoanById(int loanId)
        {
            var loan = await _loanService.GetLoanByIdAsync(loanId);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        [HttpGet("officer/my-dashboard")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> GetOfficerDashboard()
        {
            var officerId = int.Parse(User.FindFirst("UserId")!.Value);
            return Ok(await _loanService.GetLoansByOfficerAsync(officerId));
        }

        // Approves or rejects a loan application (Officer only).
        [HttpPut("{loanId}/decision")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> ApproveRejectLoan(int loanId, LoanStatusUpdateDto dto)
        {
            var officerId = int.Parse(User.FindFirst("UserId")!.Value);
            await _loanService.ApproveOrRejectLoanAsync(loanId, officerId, dto);
            return Ok(new { message = "Loan status updated." });
        }
    }
}
