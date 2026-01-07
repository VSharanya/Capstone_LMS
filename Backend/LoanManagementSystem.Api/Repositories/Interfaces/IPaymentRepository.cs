using LoanManagementSystem.Api.Models;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<IEnumerable<Payment>> GetPaymentsByLoanIdAsync(int loanId); 
}