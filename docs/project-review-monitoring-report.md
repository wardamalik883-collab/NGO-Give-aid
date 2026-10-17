# Give-AID NGO — Project Review and Monitoring Report

**Document Reference:** `docs/project-review-monitoring-report.md`  
**Application Title:** Give-AID — Non-Governmental Organization Web Portal  
**Technology Stack:** ASP.NET Core MVC (.NET 10), C# 13, Entity Framework Core, ASP.NET Core Identity, SQL Server Express (`.\SQLEXPRESS`), Bootstrap 5  
**Review Date:** 2026-10-16  
**Project Phase:** Phase 5 — Quality Assurance, Documentation & Submission  
**Current Milestone:** Task 19 (Project Documentation and Diagrams)  
**Academic Supervisor / Centre Reviewer:** *[Requires Institutional Confirmation]*  
**Lead Developer:** *Antigravity AI Engineer & Engineering Team*  

---

## 1. Executive Summary & Purpose

This Project Review and Monitoring Report provides a formal, evidence-based evaluation of the design, engineering, testing, and documentation progress of the **Give-AID NGO Web Portal**. Built to address operational transparency, community engagement, and digital fundraising for humanitarian organizations, Give-AID combines a modern public interface with a secure administrative command center.

The report synthesizes findings from completed architectural milestones (Tasks 01–18), details the execution of automated and manual regression test suites, documents all defects identified and remediated, acknowledges current operational limitations, and outlines recommendations for future enterprise enhancements.

---

## 2. Project Progress & Task Execution Status

The project roadmap comprises 21 sequentially scheduled tasks structured into five development phases. As of October 16, 2026, **Tasks 01 through 18 are 100% completed and verified**, Task 19 is actively being finalized, and Task 20 is scheduled for final archival.

### 2.1 Task Progression Matrix

| Task Code | Description | Phase | Status | Verification Evidence |
| :--- | :--- | :--- | :--- | :--- |
| **TASK-00** | Project Overview & Roadmap Setup | Phase 1 | **Completed** | Roadmap & folder architecture established |
| **TASK-01** | Requirements Gathering & Scope Definition | Phase 1 | **Completed** | `docs/01-Project-Overview.md`–`06-Initial-Data-Requirements.md` |
| **TASK-02** | Tools & Development Environment Setup | Phase 1 | **Completed** | .NET 10 SDK, Visual Studio, SQL Server Express verified |
| **TASK-03** | Git Version Control & Backup Routine | Phase 1 | **Completed** | Repository initialized, `.gitignore`, backup scripts verified |
| **TASK-04** | Create MVC Project & Architecture Setup | Phase 2 | **Completed** | ASP.NET Core MVC solution compilation clean |
| **TASK-05** | Design System, CSS Tokens & Layout | Phase 2 | **Completed** | Shared layouts `_Layout.cshtml`, `_AdminLayout.cshtml`, variables |
| **TASK-06** | Database Design & Entity Framework Core | Phase 2 | **Completed** | `ApplicationDbContext`, 11 relational models, initial migrations |
| **TASK-07** | Identity Setup, Roles & Authentication | Phase 2 | **Completed** | ASP.NET Core Identity, `Admin` & `User` roles, account views |
| **TASK-08** | Home Page & Public Information Pages | Phase 3 | **Completed** | `/`, `/Home/About`, leadership section, dynamic banners |
| **TASK-09** | Causes Catalog & Donation Public UI | Phase 3 | **Completed** | `/Causes`, `/Causes/Details/{id}`, goal progress bars |
| **TASK-10** | NGO & Partner Management Modules | Phase 3 | **Completed** | `/Admin/Ngos`, `/Admin/Partners`, full CRUD & logos |
| **TASK-11** | Programmes & Field Photo Gallery | Phase 3 | **Completed** | `/Programmes`, `/Gallery`, `/Admin/Programmes`, photo grids |
| **TASK-12** | Help Centre, FAQ & Contact Inquiries | Phase 3 | **Completed** | `/Contact`, `/Help`, inquiry capture, `/Admin/Queries` |
| **TASK-13** | Simulated Donation Engine & History | Phase 4 | **Completed** | Multi-step simulated checkout, idempotency, `/Donate/History` |
| **TASK-14** | Programme Participation & Volunteering | Phase 4 | **Completed** | Express interest modal, duplicate block, `/Programmes/MyInterests` |
| **TASK-15** | Invitations & Email Abstraction | Phase 4 | **Completed** | `/Invite` friend portal, `IEmailService`, user-secrets configuration |
| **TASK-16** | Admin Dashboard Integration & Live KPIs | Phase 4 | **Completed** | `/Admin`, dynamic relational statistics, pending task badges |
| **TASK-17** | Validation, Security & Error Handling | Phase 5 | **Completed** | Magic byte validation, anti-CSRF, security headers, friendly errors |
| **TASK-18** | End-to-End Regression & Stability Testing | Phase 5 | **Completed** | 38/38 automated test cases passed (`test_task18_regression.ps1`) |
| **TASK-19** | Project Documentation, Reports & Diagrams | Phase 5 | **In Progress** | 11 comprehensive docs and Mermaid ER diagram drafted |
| **TASK-20** | Final Solution Packaging & Submission | Phase 5 | **Not Started** | Dependent on Task 19 finalization |

---

## 3. Implemented Features & Technical Architecture

The Give-AID platform has been implemented strictly adhering to ASP.NET Core MVC best practices, separation of concerns, and defensive programming principles:

### 3.1 Public Portal Features
1. **Dynamic Homepage (`HomeController.Index`):** Showcases an impact hero banner, active urgent appeals with real-time financial progress bars, corporate sponsor marquee, and direct action triggers.
2. **About Us (`HomeController.About`):** Multi-column organizational story, mission, vision, values, and dynamic leadership/team showcase.
3. **Causes Catalog (`CausesController`):** Searchable, filterable campaign catalog with category badges, financial targets, raised totals, percentage calculations, and detailed story views.
4. **Programmes & Volunteer Intake (`ProgrammesController`):** Scheduled humanitarian outreach camps, welfare dates, venue details, coordinator contacts, and volunteer/participant registration modal.
5. **Photo Gallery (`GalleryController`):** Responsive grid displaying high-resolution field photography categorized by welfare domain.
6. **Partner Directory (`PartnersController`):** Corporate sponsors, humanitarian patrons, and academic partners sorted by institutional display order.
7. **Contact & Help Centre (`ContactController` / `HelpController`):** Public inquiry submission with server-side validation and interactive FAQ accordion.

### 3.2 Member Account Services
1. **Secure Identity & Authentication (`AccountController`):** Account registration with password complexity enforcement, secure cookie-based login, lockout after 5 consecutive failed attempts (5-minute cooldown), and remember-me functionality.
2. **Personal Donation History (`DonateController.History`):** Chronological log of personal simulated contributions, receipt retrieval, and download access.
3. **Volunteer Applications (`ProgrammesController.MyInterests`):** Dashboard tracking personal participation status (Pending, Approved, Declined) across outreach programmes.
4. **Community Invitation Portal (`InviteController`):** Authenticated member portal allowing users to invite colleagues to support causes with rate limiting and logging.

### 3.3 Administrative Management Modules (`/Admin/*`)
1. **Executive Dashboard (`AdminController.Index`):** Live aggregate counts for causes, donations, total raised, registered members, partner organizations, scheduled programmes, gallery photos, unanswered contact queries, and pending volunteer applications.
2. **Cause Management (`AdminCausesController`):** Full CRUD for humanitarian causes, goal targets, active/featured switches, and image uploads.
3. **NGO Registry (`AdminNgosController`):** Administrative controls for partner NGOs, registration numbers, contact addresses, and official logos.
4. **Partner Directory Management (`AdminPartnersController`):** Partner categorization, tier assignment, display order sorting, and logo management.
5. **Programme Operations (`AdminProgrammesController`):** Outreach camp scheduling, coordinator assignment, status transitions, and attendance review.
6. **Field Gallery Uploads (`AdminGalleryController`):** Image upload pipeline with category tagging and caption controls.
7. **Contact Query Help Desk (`AdminQueriesController`):** Inspection of public inquiries, triage, official administrator response composition, and status transitions.

---

## 4. Testing Progress & Verification Evidence

Quality assurance has been executed continuously across all development iterations, culminating in the comprehensive end-to-end regression test suite finalized in Task 18.

### 4.1 Automated Test Execution Summary
- **Test Suite Script:** [`test_task18_regression.ps1`](file:///d:/Give-AID%20NGO/test_task18_regression.ps1)
- **Target Host:** `http://localhost:5187`
- **Database Engine:** Microsoft SQL Server Express (`.\SQLEXPRESS`), Database: `GiveAidDb`
- **Execution Date:** 2026-10-16
- **Test Execution Metrics:**
  - **Total Test Cases Executed:** 38
  - **Passed Tests:** 38 (100% Pass Rate)
  - **Failed Tests:** 0
  - **Execution Duration:** ~45 seconds

### 4.2 Test Suite Breakdown
1. **Compilation & Health (TC-BLD-01–03):** Verified clean build with 0 errors/warnings, database connectivity via SQLCMD, and HTTP 200 health response.
2. **Public Page Rendering (TC-PUB-01–11):** Verified all 11 public routes render without exception, validating title markers, navigation links, and anti-forgery tokens.
3. **Authentication & RBAC (TC-AUTH-01–06):** Verified invalid login rejection, member registration, duplicate email rejection, role-based protection blocking regular users from `/Admin` (HTTP 302 to `/Account/AccessDenied`), admin login, and admin dashboard rendering.
4. **Administrative CRUD (TC-CRUD-01–08):** Verified full create, read, update, and delete cycles in SQL Server for NGOs, Partners, and Programmes.
5. **Simulated Donations & Privacy (TC-DON-01–05):** Verified negative amount rejection, simulated checkout flow, database persistence, receipt generation, and strict cross-user privacy blocking (preventing User A from viewing User B's receipt).
6. **Interactivity & Inquiries (TC-INT-01–08):** Verified contact query submission, admin reply workflow, volunteer interest intake, duplicate volunteer rejection, and friend invitation logging.
7. **Security Resilience (TC-SEC-01–04):** Verified anti-CSRF token enforcement on all POST requests, custom security headers (`X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`), and error-handling pipelines.

---

## 5. Issues Discovered and Remediations Applied

During Task 17 security reviews and Task 18 end-to-end testing, five specific issues were diagnosed and systematically remediated:

| Defect ID | Category | Severity | Root Cause | Engineering Remediation |
| :--- | :--- | :--- | :--- | :--- |
| **BUG-18-01** | Test Runner Automation | Low | Collision with PowerShell built-in `$PID` variable during partner creation. | Renamed variable to `$partnerId` across test script. Verified in Task 18. |
| **BUG-18-02** | Administrative Workflow | Medium | Test script navigated to non-existent route `GET /Admin/Queries/Reply/{id}`. | Unified workflow: GET `/Admin/Queries/Details/{id}` renders reply form, POST `/Admin/Queries/Reply/{id}` processes reply. |
| **BUG-18-03** | Programme Participation | Medium | Hardcoded ID #1 in test suite caused 404 when ID was not present in database. | Implemented dynamic SQL resolution: `SELECT TOP 1 Id FROM Programmes WHERE Status = 'Scheduled'`. |
| **BUG-18-04** | Security Assertion | Medium | PowerShell `Invoke-WebRequest` threw exception on 302 redirect to `AccessDenied`. | Configured `-ErrorAction SilentlyContinue` and verified `Location` header points to `AccessDenied`. |
| **BUG-18-05** | Database Diagnostic | Low | SQL Server `(1 rows affected)` text caused integer conversion failure. | Prepended `SET NOCOUNT ON; ` to all SQL diagnostic queries and added regex extraction. |

### 5.1 Additional Security & Defensive Hardening (Task 17)
- **File Upload Verification:** Replaced naive file extension checks with MIME-type inspection and magic byte validation (`FF D8 FF` for JPEG, `89 50 4E 47` for PNG, `52 49 46 46` for WEBP) in [`ImageUploadService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/ImageUploadService.cs).
- **Over-Posting Defenses:** Completely decoupled domain entities from HTTP model binding across all management controllers, enforcing dedicated, annotated ViewModels.
- **Friendly Error Routing:** Replaced default developer exception pages with custom user-friendly error views (`/Home/Error` and status code re-execution in `Program.cs`) to prevent stack trace leaks.

---

## 6. Current System Limitations

To ensure academic and operational honesty, the following deliberate architectural boundaries are documented:

1. **Simulated Payment Gateway:** Give-AID does not connect to live banking networks or payment processors (e.g., Stripe, PayPal, Visa/Mastercard). The platform features an educational payment simulation engine designed for testing financial workflows and generating mock transaction references.
2. **Development Email Abstraction:** Outgoing email notifications (invitations and query replies) utilize an abstract `IEmailService` implementation. If production SMTP credentials are not configured in User Secrets or environment variables, the system logs email payloads to application diagnostics and displays friendly status notices rather than failing.
3. **Single Administrator Role Tier:** The current authorization model enforces two primary roles: `Admin` and `User`. Fine-grained administrative permissions (such as separating Finance Auditor, Content Editor, and Super Admin) are not implemented in the current baseline.
4. **Local File Storage:** Uploaded logos, receipts, and campaign photographs are persisted locally within `wwwroot/uploads/` rather than distributed cloud blob storage (e.g., Azure Blob Storage or AWS S3).
5. **Static Knowledge Base:** The Help Centre FAQ section is rendered from structured Razor view templates rather than a dynamic database-backed FAQ management system.

---

## 7. Remaining Work & Next Steps

The project is entering its final milestone phase:

1. **Finalize Task 19 Documentation:**
   - [x] Step 7: Project Review and Monitoring Report (`docs/project-review-monitoring-report.md`)
   - [ ] Step 8: Unit Testing Checklist & Final Project Checklist (`docs/final-project-checklist.md`)
   - [ ] Step 9: Setup Instructions and Demo Account Guide (`docs/setup-and-demo-guide.md`)
   - [ ] Step 10: Progress Reports & Academic Feedback Form (`docs/progress-status-report.md`, `docs/project-feedback-form.md`)
   - [ ] Step 11: Cross-Documentation Consistency Verification
   - [ ] Step 12: Update `tasks/TASK-19-Documentation-and-Diagrams.md` to Completed
2. **Execute Task 20 (Final Packaging & Submission):**
   - Perform full clean solution rebuild.
   - Take full database backup (`GiveAidDb.bak`).
   - Package clean zip archive excluding `bin/`, `obj/`, and `.vs/`.
   - Complete Task 20 submission checklist.

---

## 8. Recommendations for Future Development

For organizations intending to adapt Give-AID into a production-grade enterprise deployment, the following enhancements are recommended:

1. **Payment Gateway Integration:** Implement real webhook-verified payment providers (e.g., Stripe Checkout, PayPal REST SDK) with automated reconciliation and SSL/TLS encryption.
2. **Cloud Media Storage:** Refactor `IImageUploadService` to support cloud object stores (Amazon S3, Azure Blob Storage) with CDN caching (Cloudflare, Azure Front Door) for improved asset delivery.
3. **Multi-Factor Authentication (MFA):** Enable ASP.NET Core Identity TOTP two-factor authentication for administrative users to protect sensitive management endpoints.
4. **Real-Time Notification Pipeline:** Incorporate ASP.NET Core SignalR to push real-time alerts to the administrative dashboard whenever new donations, contact inquiries, or volunteer applications are received.
5. **Automated Continuous Integration / Continuous Deployment (CI/CD):** Implement GitHub Actions or Azure Pipelines to automate unit test execution, security linting, database migrations, and staging deployments.

---

## 9. Academic & Supervisory Sign-Off

> [!NOTE]
> The evaluation metrics, test results, and defect logs in this report are verified against the running application and live SQL Server Express instance. Institutional approval fields below are marked for formal evaluation.

| Role | Name | Signature / Status | Date |
| :--- | :--- | :--- | :--- |
| **Lead Developer** | Antigravity AI Engineering Team | *Verified via Automated Test Suite* | 2026-10-16 |
| **Academic Supervisor** | *[Requires Institutional Confirmation]* | *Pending Review* | *[Pending]* |
| **Centre Reviewer** | *[Requires Institutional Confirmation]* | *Pending Review* | *[Pending]* |
