# LoanSphere - Loan Management System

LoanSphere is a modern, full-stack Loan Management System designed to streamline the entire loan lifecycle, from application to closure. It provides a secure, role-based platform for Admins, Loan Officers, and Customers to manage loans, track payments, and generate insightful reports.

## 🌟 Key Features

### 👤 Customer Portal
- **Dashboard**: At-a-glance view of active loans, upcoming EMIs, and recent notifications.
- **Loan Application**: Multi-step wizard for applying for new loans with document upload support.
- **My Loans**: Detailed list of all loan applications with status filtering (Active, closed, Rejected).
- **Status Tracker**: Visual progress tracker for checking application status (Applied → Under Review → Approved → Active).
- **Reports & Tools**:
  - **Loan Statement**: Generate detailed account statements for your loans.
  - **EMI Calculator**: Estimate monthly payments.
  - **Foreclosure Calculator**: Check settlement amounts for early closure.
- **Payments**: View EMI schedules and payment history.

### 👔 Loan Officer Portal
- **Dashboard**: Metrics on total applications, pending reviews, and approvals.
- **Loan Processing**: Review customer applications, verify documents, and approve/reject loans.
- **Customer Reports**:
  - **Customer Summaries**: Detailed list of all customers with loan counts and total outstanding amounts.
- **Financial Reports**:
  - **Loans by Status**: Visual breakdown of loan distributions (Active, Closed, Overdue).
  - **Outstanding Reports**: Track total outstanding amounts across the organization.
  - **EMI Overdue**: Identify loans with missed or overdue payments.
  - **Active vs Closed**: Compare current portfolio performance.
  - **Monthly Collections**: Track EMI collections month-over-month.

### 🛡️ Admin Portal
- **User Management**: Create and manage Loan Officers and Admins.
- **Loan Products**: Configure loan types, interest rates, and tenure options.

### 🔔 Shared Features
- **Notifications**: Real-time alerts for loan updates, approvals, and reminders.
- **Security**: JWT-based authentication and role-based authorization (RBAC).

## 🛠️ Technology Stack

### Frontend
- **Framework**: [Angular 20](https://angular.io/)
- **State Management**: Angular Signals & RxJS
- **UI Components**: Angular Material
- **Styling**: CSS/Custom CSS, Responsive Grid
- **Charts**: Interactive data visualization for reports

### Backend
- **Framework**: [.NET 8/9 Web API](https://dotnet.microsoft.com/)
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)

## 🚀 Getting Started

### Prerequisites
- **Node.js**: v18 or higher
- **Angular CLI**: v19/v20
- **.NET SDK**: 8.0/9.0
- **SQL Server**: Local or hosted instance

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/VSharanya/Capstone_LMS.git
   ```

2. **Frontend Setup**
   ```bash
   cd loan-management-frontend
   npm install
   ng serve -o
   ```
   The application will open at `http://localhost:4200/`.

3. **Backend Setup**
   - Navigate to the API directory.
   - Update `appsettings.json` with your SQL Server connection string.
   - Run database migrations:
     ```bash
     dotnet ef database update
     ```
   - Start the server:
     ```bash
     dotnet run
     ```

## 🔒 Roles & Permissions

| Role | Access Level |
|------|--------------|
| **Admin** | Full system control, User Management, Loan Type Config. |
| **Loan Officer** | Loan Verification, Approval/Rejection, Detailed Reporting. |
| **Customer** | Apply for loans, View Status, Pay EMIs, Statements. |

## 📂 Project Structure

### Frontend (`src/app/`)
```
src/app/
├── components/
│   ├── admin/           # Admin dashboard & management
│   ├── loan-officer/    # Officer dashboard & reporting
│   ├── customer/        # Customer tools & loan application
│   ├── auth/            # Login & Register
│   └── shared/          # Navbar, Sidebar, Dialogs
├── services/            # API integration services
├── models/              # TypeScript interfaces
└── interceptors/        # JWT & Error handling
```

### Backend (`LoanManagementSystem.Api/`)
```
LoanManagementSystem.Api/
├── Controllers/         # API Endpoints
├── Models/              # Database Entities (EF Core)
├── DTOs/                # Data Transfer Objects
├── Data/                # DbContext & Seeders
├── Repositories/        # Data Access Layer (Repository Pattern)
├── Services/            # Business Logic Layer
├── Security/            # JWT Token Generation
├── Mappings/            # AutoMapper Profiles
├── Middlewares/         # Global Exception Handling
└── Helpers/             # Constants & Enums
```

## 💾 Data Seeding & Default Credentials

The application comes with a `DbInitializer` that automatically seeds the database with essential data and test users on the first run.

### Default Users
| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@lms.com` | `Admin@123` |
| **Loan Officer** | `officer@lms.com` | `Officer@123` |
| **Customer** | `customer1@lms.com` | `Customer@123` |
| **Customer** | `customer2@lms.com` | `Customer@123` |

### Seeded Data
- **Loan Types**: Personal, Home, Vehicle, and Education loans are pre-configured.
- **Demo Data**: The system generates **10 extra random customers** with various loan applications (Active, Closed, Rejected) to populate the charts and reports immediately.
