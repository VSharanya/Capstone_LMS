using LoanManagementSystem.Api.Models;

public interface ILoanTypeRepository
{
    Task AddAsync(LoanType loanType);
    Task<LoanType?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<LoanType>> GetActiveLoanTypesAsync();
    Task<IEnumerable<LoanType>> GetAllAsync();

    Task SaveAsync();
}