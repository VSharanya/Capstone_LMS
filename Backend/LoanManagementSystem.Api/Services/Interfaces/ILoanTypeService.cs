using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Models;

public interface ILoanTypeService
{
    Task<LoanType> CreateLoanTypeAsync(LoanTypeCreateDto dto, int adminId);
    Task UpdateLoanTypeAsync(int id, LoanTypeUpdateDto dto, int adminUserId);
    Task<IEnumerable<LoanType>> GetActiveLoanTypesAsync();
    Task<IEnumerable<LoanType>> GetAllLoanTypesAsync();

}