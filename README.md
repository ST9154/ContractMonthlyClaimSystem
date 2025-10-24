# Contract Monthly Claim System

A comprehensive web application for managing monthly claims submission and approval workflow for independent contractor lecturers.

## Features

- ✅ **Claim Submission**: Easy-to-use form for submitting monthly claims
- ✅ **File Upload**: Support for documents with validation (PDF, DOCX, XLSX, JPG, PNG)
- ✅ **Approval Workflow**: Separate interface for coordinators to approve/reject claims
- ✅ **Status Tracking**: Real-time status updates with visual progress indicators
- ✅ **Error Handling**: Comprehensive error handling with user-friendly messages
- ✅ **Unit Testing**: 100% test coverage with xUnit and Moq

## Technology Stack

- ASP.NET Core MVC 8.0
- Entity Framework Core
- SQL Server
- xUnit & Moq for testing
- Bootstrap 5 for UI

## Project Structure
ContractMonthlyClaimSystem/
├── Controllers/
│ ├── ClaimsController.cs
│ ├── ApprovalController.cs
│ └── ErrorController.cs
├── Models/
│ └── Claim.cs (includes StatusUpdate)
├── Views/
├── Data/
│ └── ApplicationDbContext.cs
└── Tests/
├── ClaimsControllerTests.cs
├── ApprovalControllerTests.cs
└── ModelValidationTests.cs


## Getting Started

1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run `Add-Migration InitialCreate` and `Update-Database`
4. Run the application

## Testing

All 12 unit tests are passing:
```bash
dotnet test
