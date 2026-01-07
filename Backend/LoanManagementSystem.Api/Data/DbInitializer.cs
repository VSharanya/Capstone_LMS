#nullable disable
using LoanManagementSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Apply migrations automatically
            await context.Database.MigrateAsync();

            Console.WriteLine(" Seeding Database...");

            // 1. SEED USERS
            if (!context.Users.Any())
            {
                var passwordHasher = new PasswordHasher<User>();

                var admin = new User
                {
                    FullName = "Admin User",
                    Email = "admin@lms.com",
                    Role = "Admin",
                    IsActive = true
                };
                admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@123");

                var loanOfficer = new User
                {
                    FullName = "Loan Officer",
                    Email = "officer@lms.com",
                    Role = "LoanOfficer",
                    IsActive = true
                };
                loanOfficer.PasswordHash = passwordHasher.HashPassword(loanOfficer, "Officer@123");

                var customer1 = new User
                {
                    FullName = "Gayathri Yerraballi",
                    Email = "customer1@lms.com",
                    Role = "Customer",
                    AnnualIncome = 1200000,
                    IsActive = true
                };
                customer1.PasswordHash = passwordHasher.HashPassword(customer1, "Customer@123");

                var customer2 = new User
                {
                    FullName = "Rahul Sharma",
                    Email = "customer2@lms.com",
                    Role = "Customer",
                    AnnualIncome = 850000,
                    IsActive = true
                };
                customer2.PasswordHash = passwordHasher.HashPassword(customer2, "Customer@123");

                await context.Users.AddRangeAsync(admin, loanOfficer, customer1, customer2);
                await context.SaveChangesAsync();
                Console.WriteLine("Users Seeded");
            }

            // 2. SEED LOAN TYPES
            if (!context.LoanTypes.Any())
            {
                var loanTypes = new List<LoanType>
                {
                    new LoanType
                    {
                        LoanTypeName = "Personal Loan",
                        InterestRate = 12.0m,
                        MinAmount = 50000,
                        MaxAmount = 500000,
                        MaxTenureMonths = 60,
                        IsActive = true,
                        CreatedOn = DateTime.Now.Date
                    },
                    new LoanType
                    {
                        LoanTypeName = "Home Loan",
                        InterestRate = 8.5m,
                        MinAmount = 500000,
                        MaxAmount = 5000000,
                        MaxTenureMonths = 240,
                        IsActive = true,
                        CreatedOn = DateTime.Now.Date
                    },
                    new LoanType
                    {
                        LoanTypeName = "Vehicle Loan",
                        InterestRate = 9.5m,
                        MinAmount = 100000,
                        MaxAmount = 1500000,
                        MaxTenureMonths = 84,
                        IsActive = true,
                        CreatedOn = DateTime.Now.Date
                    },
                    new LoanType
                    {
                        LoanTypeName = "Education Loan",
                        InterestRate = 9.0m,
                        MinAmount = 100000,
                        MaxAmount = 2000000,
                        MaxTenureMonths = 120,
                        HasMoratorium = true,
                        MoratoriumMonths = 24,
                        IsActive = true,
                        CreatedOn = DateTime.Now.Date
                    }
                };

                await context.LoanTypes.AddRangeAsync(loanTypes);
                await context.SaveChangesAsync();
                Console.WriteLine("Loan Types Seeded");
            }

            // 3. SEED LOANS & EMIs
            if (!context.LoanApplications.Any())
            {
                // Fetch seeded entities
                var personalType = await context.LoanTypes.FirstOrDefaultAsync(t => t.LoanTypeName == "Personal Loan");
                var homeType = await context.LoanTypes.FirstOrDefaultAsync(t => t.LoanTypeName == "Home Loan");
                var vehicleType = await context.LoanTypes.FirstOrDefaultAsync(t => t.LoanTypeName == "Vehicle Loan");
                var educationType = await context.LoanTypes.FirstOrDefaultAsync(t => t.LoanTypeName == "Education Loan");

                var customer1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer1@lms.com");
                var customer2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer2@lms.com");
                var officer = await context.Users.FirstOrDefaultAsync(u => u.Email == "officer@lms.com");

                var loans = new List<LoanApplication>();

                // 1. Active Personal Loan (Customer 1) - With EMIs
                var activeLoan = new LoanApplication
                {
                    Customer = customer1,
                    LoanType = personalType,
                    LoanAmount = 200000,
                    TenureMonths = 24,
                    Status = "Active",
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)), //  Datetime -> DateOnly
                    ApprovedByUser = officer, //  Assigning to Navigation Property instead of int FK
                    ApprovedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)), //  Datetime -> DateOnly
                    EmiStartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)), //  Datetime -> DateOnly
                    Remarks = "Approved based on high credit score."
                };
                loans.Add(activeLoan);

                // 2. Applied Home Loan (Customer 2)
                var appliedLoan = new LoanApplication
                {
                    Customer = customer2,
                    LoanType = homeType,
                    LoanAmount = 3500000,
                    TenureMonths = 180,
                    Status = "Applied",
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), //  Datetime -> DateOnly
                    Remarks = "Documents submitted pending verification."
                };
                loans.Add(appliedLoan);

                // 3. Under Review Vehicle Loan (Customer 1)
                var reviewLoan = new LoanApplication
                {
                    Customer = customer1,
                    LoanType = vehicleType,
                    LoanAmount = 800000,
                    TenureMonths = 60,
                    Status = "Under Review",
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), //  Datetime -> DateOnly
                    Remarks = "Undergoing manual verification."
                };
                loans.Add(reviewLoan);

                // 4. Rejected Personal Loan (Customer 2)
                var rejectedLoan = new LoanApplication
                {
                    Customer = customer2,
                    LoanType = personalType,
                    LoanAmount = 1000000, 
                    TenureMonths = 36,
                    Status = "Rejected",
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)), //  Datetime -> DateOnly
                    ApprovedByUser = officer, //  Assigning to Navigation Property
                    ApprovedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)), //  Datetime -> DateOnly
                    Remarks = "Debt-to-Income ratio too high."
                };
                loans.Add(rejectedLoan);

                // 5. Closed Education Loan (Customer 1)
                var closedLoan = new LoanApplication
                {
                    Customer = customer1,
                    LoanType = educationType,
                    LoanAmount = 500000,
                    TenureMonths = 48,
                    Status = "Closed",
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)), // Datetime -> DateOnly
                    ApprovedByUser = officer, // Assigning to Navigation Property
                    ApprovedDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)), // Datetime -> DateOnly
                    EmiStartDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-2).AddMonths(24)), // Datetime -> DateOnly
                    Remarks = "Fully Repaid."
                };
                loans.Add(closedLoan);


                await context.LoanApplications.AddRangeAsync(loans);
                await context.SaveChangesAsync();
                Console.WriteLine("Loans Seeded");

                // 4. SEED EMIs (Only for Active Loan)

                // Generate EMIs for the Active Personal Loan
                decimal loanAmount = activeLoan.LoanAmount;
                decimal annualRate = activeLoan.LoanType.InterestRate;
                decimal monthlyRate = annualRate / 12 / 100;
                int months = activeLoan.TenureMonths;

                decimal emiAmount = (loanAmount * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months)) / ((decimal)Math.Pow(1 + (double)monthlyRate, months) - 1);
                
                var emiSchedule = new List<EMI>();
                DateOnly currentDate = activeLoan.EmiStartDate;

                for (int i = 1; i <= months; i++)
                {
                    var emi = new EMI
                    {
                        LoanId = activeLoan.LoanId,
                        InstallmentNumber = i,
                        DueDate = currentDate,
                        EMIAmount = Math.Round(emiAmount, 0),
                        IsPaid = false
                    };

                    // Mark first 2 months as PAID for realism
                    if (i <= 2)
                    {
                        emi.IsPaid = true;
                        emi.PaidDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-(2 - i))); 
                    }

                    emiSchedule.Add(emi);
                    currentDate = currentDate.AddMonths(1);
                }

                await context.EMIs.AddRangeAsync(emiSchedule);
                await context.SaveChangesAsync();
                Console.WriteLine("EMIs Seeded");

                // 5. SEED EXTRA DATA FOR REPORTS (10 More Customers)

                Console.WriteLine("Generating 10 extra customers for reporting...");
                var random = new Random();
                var newCustomers = new List<User>();

                for (int i = 3; i <= 12; i++)
                {
                    var extraCustomer = new User
                    {
                        FullName = $"Customer {i}",
                        Email = $"customer{i}@lms.com",
                        Role = "Customer",
                        AnnualIncome = random.Next(500000, 2500000), // Random Income 5L - 25L
                        IsActive = true
                    };
                    // Use same password for simplicity
                    extraCustomer.PasswordHash = new PasswordHasher<User>().HashPassword(extraCustomer, "Customer@123");
                    newCustomers.Add(extraCustomer);
                }

                await context.Users.AddRangeAsync(newCustomers);
                await context.SaveChangesAsync(); 

                // Add Loans for these new customers
                var extraLoans = new List<LoanApplication>();
                var extraEmis = new List<EMI>();

                foreach (var cust in newCustomers)
                {
                    // Each customer gets 1-3 loans
                    int loanCount = random.Next(1, 4); 

                    for (int j = 0; j < loanCount; j++)
                    {
                        // Random Loan Type
                        var type = random.Next(0, 4) switch
                        {
                            0 => personalType,
                            1 => homeType,
                            2 => vehicleType,
                            _ => educationType
                        };

                        // Random Status
                        string status = random.Next(0, 5) switch
                        {
                            0 => LoanManagementSystem.Api.Helpers.LoanStatusConstants.Applied,
                            1 => LoanManagementSystem.Api.Helpers.LoanStatusConstants.UnderReview,
                            2 => LoanManagementSystem.Api.Helpers.LoanStatusConstants.Rejected,
                            3 => LoanManagementSystem.Api.Helpers.LoanStatusConstants.Closed,
                            _ => LoanManagementSystem.Api.Helpers.LoanStatusConstants.Active 
                        };

                        // Adjust amount within range
                        decimal amount = type.MinAmount + (random.Next(0, 10) * 10000); 
                        if (amount > type.MaxAmount) amount = type.MaxAmount;

                        var loan = new LoanApplication
                        {
                            CustomerId = cust.UserId,
                            LoanTypeId = type.LoanTypeId,
                            LoanAmount = amount,
                            TenureMonths = type.MaxTenureMonths / 2, // Half max tenure
                            Status = status,
                            AppliedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-random.Next(2, 24))),
                            Remarks = "Seeded for reporting."
                        };

                        if (status == LoanManagementSystem.Api.Helpers.LoanStatusConstants.Active || 
                            status == LoanManagementSystem.Api.Helpers.LoanStatusConstants.Closed ||
                            status == LoanManagementSystem.Api.Helpers.LoanStatusConstants.Rejected)
                        {
                             loan.ApprovedBy = officer.UserId;
                             loan.ApprovedDate = loan.AppliedDate.AddDays(5);
                        }

                        if (status == LoanManagementSystem.Api.Helpers.LoanStatusConstants.Active)
                        {
                            loan.EmiStartDate = loan.ApprovedDate.Value.AddMonths(1);

                            // GENERATE EMIs for Active Loans
                             decimal r = type.InterestRate / 12 / 100;
                             int n = loan.TenureMonths;
                             decimal emiVal = (amount * r * (decimal)Math.Pow(1 + (double)r, n)) / ((decimal)Math.Pow(1 + (double)r, n) - 1);
                        }

                        extraLoans.Add(loan);
                    }
                }

                await context.LoanApplications.AddRangeAsync(extraLoans);
                await context.SaveChangesAsync(); // IDs generated here

                // NOW Generate EMIs for the Active Extra Loans
                foreach (var l in extraLoans.Where(x => x.Status == LoanManagementSystem.Api.Helpers.LoanStatusConstants.Active))
                {
                     decimal r = l.LoanType.InterestRate / 12 / 100;
                     int n = l.TenureMonths;
                     decimal emiVal = (l.LoanAmount * r * (decimal)Math.Pow(1 + (double)r, n)) / ((decimal)Math.Pow(1 + (double)r, n) - 1);
                     
                     DateOnly d = l.EmiStartDate;
                     for(int k=1; k<=n; k++)
                     {
                         var emi = new EMI
                         {
                             LoanId = l.LoanId,
                             InstallmentNumber = k,
                             DueDate = d,
                             EMIAmount = Math.Round(emiVal, 0),
                             IsPaid = k < 3 // First 2 paid
                         };
                         if(emi.IsPaid) emi.PaidDate = d;
                         
                         extraEmis.Add(emi);
                         d = d.AddMonths(1);
                     }
                }
                
                await context.EMIs.AddRangeAsync(extraEmis);
                await context.SaveChangesAsync();

                Console.WriteLine($" Added {newCustomers.Count} extra customers, {extraLoans.Count} loans, and {extraEmis.Count} EMIs.");
            }
        }
    }
}
