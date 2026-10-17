# Give-AID — Non-Governmental Organization Web Portal

Give-AID is a full-featured, secure web platform engineered for non-governmental organizations to streamline digital fundraising, volunteer engagement, outreach camp coordination, and stakeholder communications. Built with **ASP.NET Core MVC (.NET 10)**, **C# 13**, **Entity Framework Core**, and **SQL Server**, the application provides a public-facing portal for benefactors and an administrative command center for organizational leadership.

---

## 1. Project Overview & Architecture

Give-AID bridges the gap between charitable benefactors, partner welfare organizations, and corporate sponsors through transparent campaign tracking and verified impact reports:

- **Public Benefactor Portal:** High-impact appeal showcases, live fundraising progress bars, searchable cause directory, welfare camp schedules, field photo galleries, and corporate sponsor directories.
- **Member Self-Service:** Secure registration, authentication, personal donation receipts, volunteer application tracking, and colleague invitation tools.
- **Administrative Command Center:** Executive dashboard with live SQL Server aggregate KPI metrics, fundraising campaign CRUD, partner NGO onboarding, corporate partner tiers, event scheduling, gallery uploads with binary header validation, and public inquiry help desk triage.

---

## 2. Main Features

- **Fundraising Campaigns & Real-Time Tracking:** Create, edit, and feature appeals with goal targets, funds raised counters, and percentage progress indicators.
- **Simulated Donation Gateway:** Multi-step educational payment processing with input validation, duplicate charge prevention (idempotency tokens), transaction logging, and branded PDF-ready receipts.
- **Welfare Outreach & Volunteering:** Scheduling of medical camps, food distribution drives, and literacy seminars with public volunteer interest intake and automated duplicate application prevention.
- **Accredited NGO & Partner Registry:** Multi-tier directory classifying humanitarian partners, emergency welfare patrons, and corporate sponsors with display order priority.
- **Inquiry Help Desk & Messaging:** Visitor contact forms with server-side validation, administrative response composition, status tracking, and friend invitation tools.
- **Defensive Engineering & Security:** Anti-CSRF protection across all HTTP POST actions, binary magic byte image inspection (`FF D8 FF`, `89 50 4E 47`, `52 49 46 46`), strict cross-user financial receipt protection, and friendly error views concealing internal stack traces.

---

## 3. Technology Stack & Prerequisites

### 3.1 Technology Stack
- **Framework:** ASP.NET Core MVC (.NET 10.0)
- **Language:** C# 13
- **ORM:** Entity Framework Core 10.0.12 (Code-First)
- **Authentication & Security:** ASP.NET Core Identity with PBKDF2 password hashing & Role-Based Access Control (RBAC)
- **Database Engine:** Microsoft SQL Server 2019/2022 or Microsoft SQL Server Express (`.\SQLEXPRESS`)
- **Front-End Styling:** Bootstrap 5.3.3, Give-AID Custom CSS Design System, Vanilla JavaScript

### 3.2 Prerequisites
- **.NET 10 SDK:** Version `10.0.401` or newer ([Download .NET 10 SDK](https://dotnet.microsoft.com/))
- **SQL Server:** SQL Server 2019/2022 or SQL Server Express (`.\SQLEXPRESS`)
- **Development Tool:** Visual Studio 2022 (v17.12+) or Visual Studio Code with the *C# Dev Kit*

---

## 4. Database Setup & Initialization

Give-AID supports three verified database setup methods. Choose the option best suited for your evaluation environment:

### Option A: Entity Framework Core Migrations (Recommended)
1. Verify the connection string in [`GiveAid.Web/appsettings.json`](file:///d:/Give-AID%20NGO/GiveAid.Web/appsettings.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GiveAidDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
2. Open PowerShell in the solution root and apply the migrations:
   ```powershell
   dotnet ef database update --project GiveAid.Web
   ```
   *Baseline accredited NGOs and sample urgent causes are automatically seeded on first web launch.*

### Option B: Restoring from SQL Server Backup (.bak)
A full pre-populated database backup is included in `database/GiveAidDb.bak`:
```powershell
sqlcmd -S ".\SQLEXPRESS" -d master -C -Q "RESTORE DATABASE [GiveAidDb] FROM DISK = N'd:\Give-AID NGO\database\GiveAidDb.bak' WITH REPLACE, RECOVERY;"
```

### Option C: Executing the Idempotent DDL Script
Execute the included schema creation script:
```powershell
sqlcmd -S ".\SQLEXPRESS" -d master -C -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'GiveAidDb') CREATE DATABASE [GiveAidDb];"
sqlcmd -S ".\SQLEXPRESS" -d GiveAidDb -C -i "database\GiveAidDb_Schema.sql"
```

*For comprehensive database instructions, refer to [`docs/database-setup-and-restore.md`](file:///d:/Give-AID%20NGO/docs/database-setup-and-restore.md).*

---

## 5. Configuring Administrator & Development Secrets

In accordance with secure coding best practices, **no administrator passwords or API keys are committed in source code**. Initial credentials are configured locally via ASP.NET Core User Secrets:

```powershell
# 1. Initialize secrets in the web project
dotnet user-secrets init --project GiveAid.Web

# 2. Configure initial demonstration administrator credentials
dotnet user-secrets set "AdminSeed:Email" "admin@giveaid.org" --project GiveAid.Web
dotnet user-secrets set "AdminSeed:Password" "Admin@123" --project GiveAid.Web
dotnet user-secrets set "AdminSeed:FullName" "Give-AID Administrator" --project GiveAid.Web

# 3. (Optional) Configure SMTP development sandbox (e.g. Mailtrap)
dotnet user-secrets set "Email:SmtpServer" "smtp.mailtrap.io" --project GiveAid.Web
dotnet user-secrets set "Email:Port" "2525" --project GiveAid.Web
dotnet user-secrets set "Email:Username" "your-sandbox-username" --project GiveAid.Web
dotnet user-secrets set "Email:Password" "your-sandbox-password" --project GiveAid.Web
```
*If SMTP credentials are not configured, Give-AID gracefully simulates email delivery and logs message payloads to application diagnostics without failing.*

---

## 6. Running the Application

### 6.1 Using .NET CLI
```powershell
dotnet run --project GiveAid.Web
```
The application will build, execute initial data seed checks, and start listening on:
- **HTTP:** `http://localhost:5187`
- **HTTPS:** `https://localhost:7187`

### 6.2 Using Visual Studio
1. Open `GiveAid.sln` in Visual Studio 2022.
2. Ensure `GiveAid.Web` is set as the startup project.
3. Press **F5** (Debug) or **Ctrl + F5** (Run without debugging).

---

## 7. User Roles & Access Information

| Account Type | Email | Password | Role | Permissions |
| :--- | :--- | :--- | :--- | :--- |
| **System Admin** | `admin@giveaid.org` | *Configured via User Secrets* (Default: `Admin@123`) | `Admin` | Full management access to `/Admin`, Causes, NGOs, Partners, Programmes, Gallery, and Queries. |
| **Public Visitor** | *Anonymous* | N/A | None | Browse appeals, NGO stories, partners, programmes, FAQs, and submit inquiries. |
| **Registered Member**| *Register at `/Account/Register`* | User defined | `User` | Donate to causes, view personal donation history/receipts, track volunteer applications, invite friends. |

---

## 8. Running Automated Tests

Give-AID includes an automated end-to-end regression test suite covering build diagnostics, public routing, authentication, RBAC guards, CRUD operations, simulated donations, and volunteer applications:

```powershell
powershell -ExecutionPolicy Bypass -File "d:\Give-AID NGO\test_task18_regression.ps1"
```

**Verified Test Results:**
- **Total Tests Executed:** 38
- **Tests Passed:** 38 (100% Pass Rate)
- **Tests Failed:** 0

*Refer to [`docs/final-verification-report.md`](file:///d:/Give-AID%20NGO/docs/final-verification-report.md) and [`docs/unit-testing-checklist.md`](file:///d:/Give-AID%20NGO/docs/unit-testing-checklist.md) for full test logs.*

---

## 9. Known Limitations

1. **Simulated Payment Gateway:** Give-AID uses an educational payment simulation engine. It does not connect to live commercial banking networks (Visa/Mastercard/Stripe) and does not handle real credit cards.
2. **Email Delivery Fallback:** When external SMTP credentials are not provided via User Secrets, emails (invitations and query replies) are logged to diagnostics and simulated in memory.
3. **Local Storage:** Uploaded logos and photos are stored locally in `wwwroot/uploads/` rather than cloud object stores (e.g. AWS S3 or Azure Blob Storage).

---

## 10. Troubleshooting Common Issues

- **Database Connection Failure:** Verify SQL Server Express is running: `Get-Service -Name "MSSQL$SQLEXPRESS"`. Start with `Start-Service -Name "MSSQL$SQLEXPRESS"`.
- **Port Conflict (5187 in use):** Free the port or modify `applicationUrl` in `GiveAid.Web/Properties/launchSettings.json`.
- **Access Denied on /Admin:** Ensure you have logged in with an account assigned the `Admin` role. Standard users are strictly redirected to `/Account/AccessDenied`.
- **Upload Rejected:** Ensure uploaded images are genuine JPEG, PNG, or WebP files under 5 MB. Renamed binary executables are rejected by magic byte inspection.

---

## 11. Complete Project Documentation Index

All technical documents, architectural specifications, and evaluation forms are available in the [`docs/`](file:///d:/Give-AID%20NGO/docs) folder:

- [`docs/project-synopsis.md`](file:///d:/Give-AID%20NGO/docs/project-synopsis.md) — Academic synopsis, problem definition, objectives, and scope.
- [`docs/customer-requirement-specification.md`](file:///d:/Give-AID%20NGO/docs/customer-requirement-specification.md) — Comprehensive requirements and FR matrix (FR-001–FR-056).
- [`docs/project-plan.md`](file:///d:/Give-AID%20NGO/docs/project-plan.md) — 5-phase project schedule, milestone roadmap, and testing approach.
- [`docs/er-diagram.mmd`](file:///d:/Give-AID%20NGO/docs/er-diagram.mmd) & [`docs/database-design.md`](file:///d:/Give-AID%20NGO/docs/database-design.md) — Relational ER diagram and table specifications.
- [`docs/algorithms-and-program-logic.md`](file:///d:/Give-AID%20NGO/docs/algorithms-and-program-logic.md) — Logic algorithms for authentication, donations, and file validation.
- [`docs/gui-standards.md`](file:///d:/Give-AID%20NGO/docs/gui-standards.md) & [`docs/interface-design.md`](file:///d:/Give-AID%20NGO/docs/interface-design.md) — GUI guidelines and 18-page screenshot inventory.
- [`docs/project-review-monitoring-report.md`](file:///d:/Give-AID%20NGO/docs/project-review-monitoring-report.md) — Project review, bug resolution summary, and recommendations.
- [`docs/final-verification-report.md`](file:///d:/Give-AID%20NGO/docs/final-verification-report.md) — Clean build and 38-test regression verification report.
- [`docs/database-setup-and-restore.md`](file:///d:/Give-AID%20NGO/docs/database-setup-and-restore.md) — Step-by-step database setup and restoration guide.
- [`docs/environment-and-versions.md`](file:///d:/Give-AID%20NGO/docs/environment-and-versions.md) — SDK, package, and environment specification.
- [`docs/setup-and-demo-guide.md`](file:///d:/Give-AID%20NGO/docs/setup-and-demo-guide.md) — Local workstation setup and demo walkthrough.
- [`docs/progress-status-report.md`](file:///d:/Give-AID%20NGO/docs/progress-status-report.md) — Academic evaluation progress report.
- [`docs/project-feedback-form.md`](file:///d:/Give-AID%20NGO/docs/project-feedback-form.md) — Evaluator rubric and scoring instrument.
- [`docs/final-project-checklist.md`](file:///d:/Give-AID%20NGO/docs/final-project-checklist.md) — 47-point acceptance verification checklist.
- [`docs/bug-log.md`](file:///d:/Give-AID%20NGO/docs/bug-log.md) — Defect analysis and regression remediation log.
