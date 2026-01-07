using LoanManagementSystem.Api.Data;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class LoanTypeRepository : ILoanTypeRepository
{
    private readonly ApplicationDbContext _context;

    public LoanTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoanType loanType)
    {
        await _context.LoanTypes.AddAsync(loanType);
    }

    public async Task<LoanType?> GetByIdAsync(int id)
    {
        return await _context.LoanTypes.FindAsync(id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.LoanTypes
            .AnyAsync(l => l.LoanTypeName == name);
    }
    public async Task<IEnumerable<LoanType>> GetActiveLoanTypesAsync()
    {
        return await _context.LoanTypes
            .Where(l => l.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanType>> GetAllAsync()
    {
        return await _context.LoanTypes.ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
