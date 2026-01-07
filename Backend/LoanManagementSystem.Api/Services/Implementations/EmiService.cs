using AutoMapper;
using LoanManagementSystem.Api.DTOs.EMI;
using LoanManagementSystem.Api.Helpers;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class EmiService : IEmiService
    {
        private readonly IEmiRepository _emiRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly INotificationService _notificationService; 
        private readonly IMapper _mapper;

        public EmiService(
            IEmiRepository emiRepository,
            ILoanRepository loanRepository,
            IPaymentRepository paymentRepository,
            INotificationService notificationService, 
            IMapper mapper)
        {
            _emiRepository = emiRepository;
            _loanRepository = loanRepository;
            _paymentRepository = paymentRepository;
            _notificationService = notificationService; 
            _mapper = mapper;
        }

        // Generates the full EMI schedule for a loan based on principal, rate, and tenure.
        public async Task GenerateEmiScheduleAsync(int loanId)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ApplicationException("Loan not found.");

            if (loan.Status != LoanStatusConstants.Approved)
                throw new ApplicationException("Loan must be approved before generating EMI.");

            var principal = loan.LoanAmount;
            var tenure = loan.TenureMonths;
            var annualRate = loan.LoanType.InterestRate;

            var monthlyRate = annualRate / 12 / 100;
            var emiAmount = CalculateEmi(principal, monthlyRate, tenure);

            // EMI START DATE (RESPECTS MORATORIUM)
            var startDate = loan.EmiStartDate;

            var emis = new List<EMI>();

            for (int i = 1; i <= tenure; i++)
            {
                emis.Add(new EMI
                {
                    LoanId = loan.LoanId,
                    InstallmentNumber = i,
                    DueDate = startDate.AddMonths(i - 1),
                    EMIAmount = Math.Round(emiAmount, 0), // Rounded to nearest integer
                    IsPaid = false
                });
            }

            await _emiRepository.AddRangeAsync(emis);
            await _emiRepository.SaveAsync();
        }

        // Marks a single EMI installment as paid and records the payment transaction.
        public async Task MarkEmiAsPaidAsync(int emiId, string paymentMode)
        {
            var emi = await _emiRepository.GetEmiByIdAsync(emiId);
            if (emi == null)
                throw new ApplicationException("EMI not found.");

            if (emi.IsPaid)
                throw new ApplicationException("EMI already paid.");

            emi.IsPaid = true;
            emi.PaidDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _emiRepository.UpdateEmiAsync(emi);

            var payment = new Payment
            {
                EMIId = emi.EMIId,
                PaidAmount = emi.EMIAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentMode = paymentMode 
            };

            await _paymentRepository.AddAsync(payment);

            //  Fetch Loan to get CustomerId for notification
            var loan = await _loanRepository.GetLoanByIdAsync(emi.LoanId);
            if (loan == null) throw new ApplicationException("Loan not found for this EMI.");

            // NOTIFY USER OF PAYMENT
            await _notificationService.CreateNotificationAsync(
                loan.CustomerId,
                $"Payment received for Installment #{emi.InstallmentNumber} ({emi.EMIAmount:C}).",
                "Success"
            );

            // CLOSE LOAN IF ALL EMIs PAID
            if (loan.EMIs.All(e => e.IsPaid))
            {
                loan.Status = LoanStatusConstants.Closed;
                await _loanRepository.UpdateLoanAsync(loan);

                // NOTIFY CLOSURE
                await _notificationService.CreateNotificationAsync(
                    loan.CustomerId,
                    $"Congratulations! Your loan {loan.LoanType.LoanTypeName} has been fully repaid and is now CLOSED.",
                    "Success"
                );
            }
        }

        // Calculates the total amount required to foreclose (pay off) the loan today.
        public async Task<decimal> GetForeclosureAmountAsync(int loanId)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(loanId);
            if (loan == null) throw new ApplicationException("Loan not found.");
            if (loan.Status != LoanStatusConstants.Active) return 0; // Or throw exception

            // Calculate Principal Paid So Far
            var paidEmis = loan.EMIs.Where(e => e.IsPaid).OrderBy(e => e.InstallmentNumber).ToList();
            var unpaidEmis = loan.EMIs.Where(e => !e.IsPaid).ToList();

            if (!unpaidEmis.Any()) return 0;

            decimal principalBalance = loan.LoanAmount;
            decimal annualRate = loan.LoanType.InterestRate;
            decimal monthlyRate = annualRate / 12 / 100;

            // Re-run amortization for paid EMIs
            foreach (var emi in paidEmis)
            {
                decimal interestComponent = principalBalance * monthlyRate;
                decimal principalComponent = emi.EMIAmount - interestComponent;
                principalBalance -= principalComponent;
            }

            
            return Math.Round(principalBalance, 0); // Rounded to nearest integer
        }

        // Processes a full foreclosure payment, closing all remaining EMIs and the loan itself.
        public async Task PayFullOutstandingAsync(int loanId, string paymentMode)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(loanId);
            if (loan == null)
                throw new ApplicationException("Loan not found.");

            if (loan.Status != LoanStatusConstants.Active)
                throw new ApplicationException("Only active loans can be foreclosed.");

            var unpaidEmis = loan.EMIs.Where(e => !e.IsPaid).OrderBy(e => e.InstallmentNumber).ToList();

            if (!unpaidEmis.Any())
                throw new ApplicationException("No outstanding installments. Loan is already paid.");

            // 1. Get Calculated Amount
            decimal foreclosureAmount = await GetForeclosureAmountAsync(loanId);

            // 2. Identify the EMI to attach this payment to (The first unpaid one)
            var closingEmi = unpaidEmis.First();

            // 3. Mark this EMI as Paid (Foreclosed)
            closingEmi.IsPaid = true;
            closingEmi.PaidDate = DateOnly.FromDateTime(DateTime.UtcNow);
            // We do NOT change closingEmi.EMIAmount because that's the scheduled amount.
            // The actual PaidAmount in the Payment table will reflect the foreclosure value.
            
            await _emiRepository.UpdateEmiAsync(closingEmi);

            // 4. Create Final Payment Record linked to this preserved EMI
            var foreclosurePayment = new Payment
            {
                 EMIId = closingEmi.EMIId, 
                 PaidAmount = foreclosureAmount,
                 PaymentDate = DateTime.UtcNow,
                 PaymentMode = paymentMode
            };

            await _paymentRepository.AddAsync(foreclosurePayment);

            // 5. Delete REMAINING Unpaid EMIs (Installments after the closing one)
            var emisToDelete = unpaidEmis.Where(e => e.EMIId != closingEmi.EMIId).ToList();
            if (emisToDelete.Any())
            {
                await _emiRepository.RemoveRangeAsync(emisToDelete);
            }

            // 6. Close Loan
            loan.Status = LoanStatusConstants.Closed;
            await _loanRepository.UpdateLoanAsync(loan);

            // NOTIFY USER
            await _notificationService.CreateNotificationAsync(
                loan.CustomerId,
                $"Foreclosure successful! Your loan {loan.LoanType.LoanTypeName} has been fully repaid via foreclosure and is now CLOSED.",
                "Success"
            );
        }


        // Retrieves the EMI schedule for a loan.
        public async Task<IEnumerable<EmiResponseDto>> GetEmisByLoanIdAsync(int loanId)
        {
            var emis = await _emiRepository.GetEmisByLoanIdAsync(loanId);
            return _mapper.Map<IEnumerable<EmiResponseDto>>(emis);
        }

        // EMI FORMULA

        private static decimal CalculateEmi(decimal principal, decimal monthlyRate, int tenure)
        {
            if (monthlyRate == 0)
                return principal / tenure;

            var ratePower = (decimal)Math.Pow((double)(1 + monthlyRate), tenure);
            return principal * monthlyRate * ratePower / (ratePower - 1);
        }
        // Retrieves the payment history for a loan with amounts in local currency/formatting.
        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsHistoryAsync(int loanId)
        {
            var payments = await _paymentRepository.GetPaymentsByLoanIdAsync(loanId);
            
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            return payments.Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                InstallmentNumber = p.EMI.InstallmentNumber,
                PaidAmount = Math.Round(p.PaidAmount, 0),
                PaymentDate = TimeZoneInfo.ConvertTimeFromUtc(p.PaymentDate, istZone), // Convert to IST
                PaymentMode = p.PaymentMode
            });
        }

    }
}
