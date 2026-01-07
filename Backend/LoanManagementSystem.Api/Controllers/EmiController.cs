using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/emis")]
    [Authorize]
    public class EmiController : ControllerBase
    {
        private readonly IEmiService _emiService;

        public EmiController(IEmiService emiService)
        {
            _emiService = emiService;
        }

        // Retrieves the EMI payment schedule for a specific loan.
        [HttpGet("loan/{loanId}")]
        [Authorize(Roles = "Customer,LoanOfficer")]
        public async Task<IActionResult> GetEmisByLoan(int loanId)
        {
            return Ok(await _emiService.GetEmisByLoanIdAsync(loanId));
        }

        // Processes a payment for a specific EMI installment.
        [HttpPut("{emiId}/pay")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PayEmi(int emiId, [FromBody] LoanManagementSystem.Api.DTOs.EMI.PaymentRequestDto dto)
        {
            await _emiService.MarkEmiAsPaidAsync(emiId, dto.PaymentMode);
            return Ok(new { message = "EMI paid." });
        }

        // Pays off the entire remaining loan balance (Foreclosure).
        [HttpPut("loan/{loanId}/pay-full")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PayFullOutstanding(int loanId, [FromBody] LoanManagementSystem.Api.DTOs.EMI.PaymentRequestDto dto)
        {
            await _emiService.PayFullOutstandingAsync(loanId, dto.PaymentMode);
            return Ok(new { message = "Loan fully paid and closed successfully." });
        }

        [HttpGet("loan/{loanId}/foreclosure")]
        [Authorize(Roles = "Customer,LoanOfficer")]
        public async Task<IActionResult> GetForeclosureAmount(int loanId)
        {
            var amount = await _emiService.GetForeclosureAmountAsync(loanId);
            return Ok(new { foreclosureAmount = amount });
        }

        [HttpGet("loan/{loanId}/payments")]
        [Authorize(Roles = "Customer,LoanOfficer")]
        public async Task<IActionResult> GetPaymentHistory(int loanId)
        {
            return Ok(await _emiService.GetPaymentsHistoryAsync(loanId));
        }

    }
}
