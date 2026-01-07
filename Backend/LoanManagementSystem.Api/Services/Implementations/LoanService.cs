using AutoMapper;
using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.Helpers;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmiService _emiService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public LoanService(
            ILoanRepository loanRepository,
            IUserRepository userRepository,
            IEmiService emiService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _userRepository = userRepository;
            _emiService = emiService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        // APPLY LOAN (Customer)

        // Processes a new loan application for a customer, validating eligibility limits 
        // and moratorium rules before saving.
        public async Task<int> ApplyLoanAsync(int customerId, LoanApplyDto loanApplyDto)
        {
            var customer = await _userRepository.GetUserByIdAsync(customerId);
            if (customer == null || customer.Role != RoleConstants.Customer)
            {
                throw new ApplicationException("Invalid customer.");
            }

            var loanType = await _loanRepository.GetLoanTypeByIdAsync(loanApplyDto.LoanTypeId);
            if (loanType == null || !loanType.IsActive)
            {
                throw new ApplicationException("Invalid loan type.");
            }
            // Moratorium validation (BASED ON LOAN TYPE CONFIG)
            if (loanType.HasMoratorium &&
                (!loanApplyDto.MoratoriumMonths.HasValue ||
                 loanApplyDto.MoratoriumMonths <= 0))
            {
                throw new ApplicationException(
                    "Moratorium months are required for this loan type.");
            }

            if (!loanType.HasMoratorium &&
                loanApplyDto.MoratoriumMonths.HasValue)
            {
                throw new ApplicationException(
                    "Moratorium is not allowed for this loan type.");
            }

            // Loan amount validation
            if (loanApplyDto.LoanAmount < loanType.MinAmount ||
                loanApplyDto.LoanAmount > loanType.MaxAmount)
            {
                throw new ApplicationException("Loan amount is outside allowed limits.");
            }

            // Tenure validation
            if (loanApplyDto.TenureMonths > loanType.MaxTenureMonths)
            {
                throw new ApplicationException("Tenure exceeds maximum allowed tenure.");
            }
            // EMI vs Monthly Income Validation (Affordability Check)
            var annualIncome = customer.AnnualIncome;
            var monthlyIncome = annualIncome / 12;

            var monthlyRate = loanType.InterestRate / 12 / 100;

            var emi = EmiCalculator.CalculateEmi(
                loanApplyDto.LoanAmount,
                monthlyRate,
                loanApplyDto.TenureMonths
            );

            // EMI Start Date Calculation
            DateOnly emiStartDate;

            if (loanType.HasMoratorium)
            {
                emiStartDate = DateOnly.FromDateTime(
                    DateTime.UtcNow.AddMonths(loanApplyDto.MoratoriumMonths!.Value)
                );
            }
            else
            {
               // Default: Start next month
               emiStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
            }

            var loanApplication = _mapper.Map<LoanApplication>(loanApplyDto);
            loanApplication.CustomerId = customerId;
            loanApplication.Status = LoanStatusConstants.Applied;
            loanApplication.AppliedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            loanApplication.MoratoriumMonths = loanApplyDto.MoratoriumMonths;
            loanApplication.EmiStartDate = emiStartDate;

            await _loanRepository.AddLoanAsync(loanApplication);
            await _loanRepository.SaveAsync();

            // NOTIFY USER
            await _notificationService.CreateNotificationAsync(
                customerId,
                $"Your application for {loanType.LoanTypeName} of {loanApplyDto.LoanAmount:C} has been submitted successfully.",
                "Info"
            );

            // NOTIFY LOAN OFFICERS
            await _notificationService.NotifyRoleAsync(
                RoleConstants.LoanOfficer,
                $"New Loan Application: {customer.FullName} applied for {loanType.LoanTypeName} ({loanApplyDto.LoanAmount:C}).",
                "Info"
            );

            return loanApplication.LoanId;
        }

        // GET LOANS BY STATUS (LINQ)
        public async Task<IEnumerable<LoanResponseDto>> GetLoansByStatusAsync(string? status)
        {
            // Using LINQ on IQueryable for dynamic filtering
            var query = _loanRepository.GetAllQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(l => l.Status == status);
            }

            var loans = await query.ToListAsync();
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }

        public async Task<IEnumerable<LoanResponseDto>> GetLoansByOfficerAsync(int officerId)
        {
            var loans = await _loanRepository.GetLoansHandledByOfficerAsync(officerId);
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }

        // APPROVE / REJECT LOAN (Officer)
        public async Task ApproveOrRejectLoanAsync(
            int loanId,
            int officerId,
            LoanStatusUpdateDto statusDto)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(loanId);
            if (loan == null)
            {
                throw new ApplicationException("Loan not found.");
            }
            // Applied -> Under Review -> Approved/Rejected

            // 1. If trying to mark UNDER REVIEW, current must be APPLIED
            if (statusDto.Status == LoanStatusConstants.UnderReview)
            {
                 if (loan.Status != LoanStatusConstants.Applied)
                 {
                     throw new ApplicationException("Loan must be in 'Applied' status to move to 'Under Review'.");
                 }
            }
            // 2. If trying to REJECT, current can be APPLIED or UNDER REVIEW
            else if (statusDto.Status == LoanStatusConstants.Rejected)
            {
                 if (loan.Status != LoanStatusConstants.Applied && loan.Status != LoanStatusConstants.UnderReview)
                 {
                      throw new ApplicationException("Loan must be 'Applied' or 'Under Review' to Reject.");
                 }

                 // SEPARATION OF DUTIES CHECK: Verifier cannot reject once verified
                 if (loan.Status == LoanStatusConstants.UnderReview && loan.VerifiedBy.HasValue && loan.VerifiedBy.Value == officerId)
                 {
                      throw new ApplicationException("You verified this loan. You cannot reject it at this stage. Only the second officer can make the final decision.");
                 }
            }
            // 3. If trying to APPROVE, current must be UNDER REVIEW
            else if (statusDto.Status == LoanStatusConstants.Approved)
            {
                if (loan.Status != LoanStatusConstants.UnderReview)
                {
                     throw new ApplicationException("Loan must be 'Under Review' to Approve.");
                }
            }

            var officer = await _userRepository.GetUserByIdAsync(officerId);
            if (officer == null || officer.Role != RoleConstants.LoanOfficer)
            {
                throw new ApplicationException("Unauthorized action.");
            }

            // UNDER REVIEW (VERIFICATION)
            if (statusDto.Status == LoanStatusConstants.UnderReview)
            {
                 loan.Status = LoanStatusConstants.UnderReview;
                 loan.VerifiedBy = officerId; // Track who verified it
                 loan.VerifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                 
                 // NOTIFY USER
                 await _notificationService.CreateNotificationAsync(
                    loan.CustomerId, 
                    $"Your application for {loan.LoanType.LoanTypeName} has been verified and is currently Under Review by the final approval team.",
                    "Info"
                );
            }
            // REJECTED
            else if (statusDto.Status == LoanStatusConstants.Rejected)
            {
                loan.Status = LoanStatusConstants.Rejected;
                loan.Remarks = statusDto.Remarks;
                loan.ApprovedBy = officerId;
                
                // NOTIFY USER
                await _notificationService.CreateNotificationAsync(
                    loan.CustomerId, 
                    $"Your {loan.LoanType.LoanTypeName} for {loan.LoanAmount:C} has been REJECTED. Remarks: {statusDto.Remarks ?? "Criteria not met"}",
                    "Error"
                );

                // NOTIFY ADMINS
                await _notificationService.NotifyRoleAsync(
                    RoleConstants.Admin,
                    $"Loan Rejected: Officer {officer.FullName} rejected {loan.LoanType.LoanTypeName} for customer {loan.Customer.FullName}.",
                    "Warning"
                );
            }
            // APPROVED
            else if (statusDto.Status == LoanStatusConstants.Approved)
            {
                // SEPARATION OF DUTIES CHECK
                if (loan.VerifiedBy.HasValue && loan.VerifiedBy.Value == officerId)
                {
                    throw new ApplicationException("Violation of segregation of duties: You verified this loan, so you cannot approve it. Another officer is required.");
                }

                loan.Status = LoanStatusConstants.Approved;
                loan.ApprovedBy = officerId;
                loan.ApprovedDate = DateOnly.FromDateTime(DateTime.UtcNow);

                // Generate EMI schedule AFTER approval
                await _emiService.GenerateEmiScheduleAsync(loan.LoanId);

                loan.Status = LoanStatusConstants.Active;

                // NOTIFY USER
                await _notificationService.CreateNotificationAsync(
                    loan.CustomerId,
                    $"Congratulations! Your {loan.LoanType.LoanTypeName} for {loan.LoanAmount:C} has been APPROVED and is now ACTIVE.",
                    "Success"
                );

                // NOTIFY ADMINS
                await _notificationService.NotifyRoleAsync(
                    RoleConstants.Admin,
                    $"Loan Approved: Officer {officer.FullName} approved {loan.LoanType.LoanTypeName} for customer {loan.Customer.FullName}.",
                    "Success"
                );
            }
            else
            {
                throw new ApplicationException("Invalid loan status.");
            }

            await _loanRepository.UpdateLoanAsync(loan);
        }

        // GET LOAN BY ID
        public async Task<LoanResponseDto?> GetLoanByIdAsync(int loanId)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(loanId);
            return loan == null ? null : _mapper.Map<LoanResponseDto>(loan);
        }

        // GET LOANS BY CUSTOMER
        public async Task<IEnumerable<LoanResponseDto>> GetLoansByCustomerIdAsync(int customerId)
        {
            var loans = await _loanRepository.GetLoansByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }

        // GET ALL LOANS
        public async Task<IEnumerable<LoanResponseDto>> GetAllLoansAsync()
        {
            var loans = await _loanRepository.GetAllLoansAsync();
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }
    }
}
