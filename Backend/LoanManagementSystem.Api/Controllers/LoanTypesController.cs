using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/loan-types")]
    public class LoanTypesController : ControllerBase
    {
        private readonly ILoanTypeService _loanTypeService;

        public LoanTypesController(ILoanTypeService loanTypeService)
        {
            _loanTypeService = loanTypeService;
        }

        // Retrieves all active loan types available for customers.
        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetLoanTypes()
        {
            var loanTypes = await _loanTypeService.GetActiveLoanTypesAsync();
            return Ok(loanTypes);
        }

        // Retrieves all loan types including inactive ones (Admin only).
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLoanTypes()
        {
            var loanTypes = await _loanTypeService.GetAllLoanTypesAsync();
            return Ok(loanTypes);
        }

        // Creates a new loan type configuration (Admin only).
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLoanType(LoanTypeCreateDto dto)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var loanType = await _loanTypeService.CreateLoanTypeAsync(dto, adminId);
            return Ok(loanType);
        }

        // Updates an existing loan type configuration.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLoanType(int id, LoanTypeUpdateDto dto)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            await _loanTypeService.UpdateLoanTypeAsync(id, dto, adminId);
            return Ok(new { message = "Loan type updated successfully." });
        }
    }
}
