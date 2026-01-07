using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Implementations;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class LoanTypeService : ILoanTypeService
    {
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly ILoanRepository _loanRepository;

        public LoanTypeService(ILoanTypeRepository loanTypeRepository,ILoanRepository loanRepository)
        {
            _loanTypeRepository = loanTypeRepository;
            _loanRepository = loanRepository;
        }

        // Creates a new loan type configuration.
        public async Task<LoanType> CreateLoanTypeAsync(LoanTypeCreateDto dto, int adminId)
        {
            // Unique name validation
            if (await _loanTypeRepository.ExistsByNameAsync(dto.LoanTypeName))
                throw new ApplicationException("Loan type already exists.");

            // Interest rate validation
            if (dto.InterestRate <= 0 || dto.InterestRate > 30)
                throw new ApplicationException("Interest rate must be between 0 and 30%.");

            // Amount validation
            if (dto.MinAmount <= 0 || dto.MaxAmount <= 0 || dto.MinAmount > dto.MaxAmount)
                throw new ApplicationException("Invalid loan amount limits.");

            // Tenure validation
            if (dto.MaxTenureMonths <= 0)
                throw new ApplicationException("Max tenure must be greater than zero.");

            // Moratorium validation

            var loanType = new LoanType
            {
                LoanTypeName = dto.LoanTypeName,
                InterestRate = dto.InterestRate,
                MinAmount = dto.MinAmount,
                MaxAmount = dto.MaxAmount,
                MaxTenureMonths = dto.MaxTenureMonths,
                IsActive = dto.IsActive,
                HasMoratorium = dto.HasMoratorium,
                CreatedBy = adminId,
                CreatedOn = DateTime.UtcNow
            };

            await _loanTypeRepository.AddAsync(loanType);
            await _loanTypeRepository.SaveAsync();

            return loanType;
        }

        // Updates an existing loan type. Prevents deactivation if active loans exist.
        public async Task UpdateLoanTypeAsync(int id, LoanTypeUpdateDto dto, int adminUserId)
        {
            var loanType = await _loanTypeRepository.GetByIdAsync(id);
            if (loanType == null)
                throw new ApplicationException("Loan type not found.");

            if (dto.InterestRate <= 0 || dto.InterestRate > 30)
                throw new ApplicationException("Interest rate must be between 0 and 30%.");

            if (dto.MinAmount <= 0 || dto.MaxAmount <= 0 || dto.MinAmount > dto.MaxAmount)
                throw new ApplicationException("Invalid loan amount limits.");

            if (dto.MaxTenureMonths <= 0)
                throw new ApplicationException("Max tenure must be greater than zero.");




            // Do not allow deactivation if active loans exist
            if (!dto.IsActive && loanType.IsActive)
            {
                var hasActiveLoans =
                    await _loanRepository.AnyActiveLoansForLoanTypeAsync(id);

                if (hasActiveLoans)
                    throw new ApplicationException(
                        "Cannot deactivate loan type because active loans exist."
                    );
            }

            loanType.InterestRate = dto.InterestRate;
            loanType.MinAmount = dto.MinAmount;
            loanType.MaxAmount = dto.MaxAmount;
            loanType.MaxTenureMonths = dto.MaxTenureMonths;
            loanType.IsActive = dto.IsActive;
            loanType.HasMoratorium = dto.HasMoratorium;
            loanType.UpdatedBy = adminUserId;
            loanType.UpdatedOn = DateTime.UtcNow;

            await _loanTypeRepository.SaveAsync();
        }
        public async Task<IEnumerable<LoanType>> GetActiveLoanTypesAsync()
        {
            return await _loanTypeRepository.GetActiveLoanTypesAsync();
        }
        public async Task<IEnumerable<LoanType>> GetAllLoanTypesAsync()
        {
            return await _loanTypeRepository.GetAllAsync();
        }

    }
}
