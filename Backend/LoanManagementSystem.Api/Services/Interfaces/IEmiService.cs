using LoanManagementSystem.Api.DTOs.EMI;
using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface IEmiService
    {
        Task GenerateEmiScheduleAsync(int loanId);
        Task MarkEmiAsPaidAsync(int emiId, string paymentMode); 
        Task<IEnumerable<EmiResponseDto>> GetEmisByLoanIdAsync(int loanId);
        Task PayFullOutstandingAsync(int loanId, string paymentMode); 
        Task<decimal> GetForeclosureAmountAsync(int loanId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsHistoryAsync(int loanId); 
    }
}
