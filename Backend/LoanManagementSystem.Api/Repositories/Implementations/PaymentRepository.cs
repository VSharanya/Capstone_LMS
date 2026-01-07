using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore; // Added for Include/ToListAsync

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByLoanIdAsync(int loanId)
    {
        return await _context.Payments
            .Include(p => p.EMI)
            .Where(p => p.EMI.LoanId == loanId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}