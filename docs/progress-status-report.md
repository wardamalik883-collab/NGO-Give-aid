# Give-AID NGO — Academic Progress and Status Report

**Document Reference:** `docs/progress-status-report.md`  
**Application Title:** Give-AID — Non-Governmental Organization Web Portal  
**Document Nature:** Institutional Progress & Status Evaluation [Draft — Subject to Centre Confirmation]  
**Project Track:** Advanced Web Application Development (ASP.NET Core MVC / EF Core)  
**Reporting Period:** Project Inception (Task 01) through Quality Assurance & Documentation (Task 19)  
**Evaluation Date:** 2026-10-16  

---

## 1. Candidate & Project Identification

> [!NOTE]
> The institutional fields below represent standard eProject submission metadata. Values marked with `[Requires Confirmation]` should be updated according to your specific educational centre guidelines.

| Field | Institutional Parameter | Status / Value |
| :--- | :--- | :--- |
| **Project Title** | Give-AID — Humanitarian NGO Web Platform | Confirmed |
| **Target Technology** | ASP.NET Core MVC (.NET 10), C# 13, EF Core, SQL Server Express | Confirmed |
| **Candidate Name** | *[Student / Developer Name]* | *[Requires Confirmation]* |
| **Roll Number / Student ID** | *[Student Registration Number]* | *[Requires Confirmation]* |
| **Centre Name / Campus** | *[Educational Centre / Campus Name]* | *[Requires Confirmation]* |
| **Batch / Term** | *[Academic Term, e.g., Fall 2026]* | *[Requires Confirmation]* |
| **Faculty Supervisor** | *[Academic Supervisor / Evaluator Name]* | *[Requires Confirmation]* |
| **Current Task Milestone** | Task 19: Documentation, Diagrams and Review Reports | In Progress (95% Complete) |

---

## 2. Project Milestone & Progress Overview

The Give-AID platform development lifecycle is divided into five overarching phases. All functional, administrative, security, and testing requirements across Phases 1 through 4 and Phase 5 QA have been fully realized.

```
[Phase 1: Foundation]    ████████████████████  100% Completed (Tasks 01–03)
[Phase 2: Architecture]  ████████████████████  100% Completed (Tasks 04–07)
[Phase 3: Public UI]     ████████████████████  100% Completed (Tasks 08–12)
[Phase 4: Workflows]     ████████████████████  100% Completed (Tasks 13–16)
[Phase 5: QA & Delivery] ██████████████████░░   95% Complete  (Tasks 17–19 done; Task 20 pending)
```

### 2.1 Milestone Breakdown

| Milestone / Phase | Scope & Key Objectives | Planned Completion | Actual Status |
| :--- | :--- | :---: | :---: |
| **Phase 1: Inception & Setup** | Project synopsis, requirements specification, environment verification (.NET 10, SQL Server), Git repository initialization. | Week 1 | **Completed** |
| **Phase 2: Core Architecture** | MVC solution scaffold, Design System tokens, EF Core `ApplicationDbContext`, ASP.NET Core Identity & RBAC roles. | Week 2 | **Completed** |
| **Phase 3: Public Portals & CMS** | Home page, About story, Causes catalog, NGO & Partner directories, Outreach programmes, and Field photo gallery. | Week 3 | **Completed** |
| **Phase 4: Complex Workflows** | Simulated donation engine, idempotency, transaction receipts, volunteer interest intake, invitations, and Admin Dashboard KPIs. | Week 4 | **Completed** |
| **Phase 5: Quality Assurance & Docs** | Security hardening (magic bytes, anti-CSRF), end-to-end regression suite (38 tests), comprehensive documentation suite. | Week 5 | **In Finalization** |

---

## 3. Work Completed During Current Evaluation Cycle

During the current reporting cycle, the development team focused on system integration, security validation, regression testing, and technical documentation:

### 3.1 Task 16: Admin Dashboard Integration
- Unified all administrative modules into a single, cohesive dashboard at `/Admin`.
- Implemented real-time Entity Framework Core queries computing aggregate metrics directly from SQL Server: Total Raised, Active Causes, Affiliated NGOs, Corporate Partners, Scheduled Programmes, Gallery Photos, Unanswered Queries, and Pending Volunteer Applications.
- Established defensive role verification (`[Authorize(Roles = "Admin")]`).

### 3.2 Task 17: Validation, Security and Error Handling
- Hardened file uploads in [`ImageUploadService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/ImageUploadService.cs) with magic byte validation for JPEG, PNG, and WebP, rejecting spoofed extensions.
- Configured global anti-CSRF token verification (`AutoValidateAntiforgeryTokenAttribute`).
- Injected HTTP security headers (`X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`, `X-XSS-Protection`).
- Implemented user-friendly error routing with status code re-execution (`/Home/Error`) to conceal technical stack traces.

### 3.3 Task 18: End-to-End Regression Testing
- Authored and executed an automated 38-assertion regression suite ([`test_task18_regression.ps1`](file:///d:/Give-AID%20NGO/test_task18_regression.ps1)) targeting the live application and database.
- Achieved a **100% test pass rate (38/38)** across build diagnostics, public pages, authentication, administrative CRUD, simulated donations, volunteer intake, and privacy guards.
- Identified and resolved 5 functional defects documented in [`docs/bug-log.md`](file:///d:/Give-AID%20NGO/docs/bug-log.md).

### 3.4 Task 19: Documentation and Technical Deliverables
- Prepared full eProject documentation suite:
  - [`docs/project-synopsis.md`](file:///d:/Give-AID%20NGO/docs/project-synopsis.md)
  - [`docs/customer-requirement-specification.md`](file:///d:/Give-AID%20NGO/docs/customer-requirement-specification.md)
  - [`docs/project-plan.md`](file:///d:/Give-AID%20NGO/docs/project-plan.md)
  - [`docs/er-diagram.mmd`](file:///d:/Give-AID%20NGO/docs/er-diagram.mmd) & [`docs/database-design.md`](file:///d:/Give-AID%20NGO/docs/database-design.md)
  - [`docs/algorithms-and-program-logic.md`](file:///d:/Give-AID%20NGO/docs/algorithms-and-program-logic.md)
  - [`docs/gui-standards.md`](file:///d:/Give-AID%20NGO/docs/gui-standards.md) & [`docs/interface-design.md`](file:///d:/Give-AID%20NGO/docs/interface-design.md)
  - [`docs/project-review-monitoring-report.md`](file:///d:/Give-AID%20NGO/docs/project-review-monitoring-report.md)
  - [`docs/unit-testing-checklist.md`](file:///d:/Give-AID%20NGO/docs/unit-testing-checklist.md) & [`docs/final-project-checklist.md`](file:///d:/Give-AID%20NGO/docs/final-project-checklist.md)
  - [`docs/setup-and-demo-guide.md`](file:///d:/Give-AID%20NGO/docs/setup-and-demo-guide.md)

---

## 4. Summary of Quality & Testing Metrics

- **Compilation Status:** `dotnet build` executes with **0 Errors** and **0 Warnings**.
- **Automated Test Cases:** 38 executed, 38 passed (**100% pass rate**).
- **Relational Tables:** 11 database entities fully mapped and verified in SQL Server.
- **Security Protections:** Anti-CSRF on all POST endpoints, strict RBAC authorization, magic byte upload checks, parameterization via EF Core preventing SQL injection.
- **Regressions Resolved:** 5 defects diagnosed and permanently fixed in regression test suite.

---

## 5. Challenges Encountered & Solutions Implemented

1. **PowerShell Automation Collision:** Script variable `$pId` collided with PowerShell's read-only `$PID` variable during partner creation tests.  
   *Solution:* Renamed variable to `$partnerId` across test scripts.
2. **Contact Query Workflow Route Alignment:** Administrator reply route was originally targeted at a non-existent GET `/Admin/Queries/Reply/{id}`.  
   *Solution:* Unified query review and reply submission on `GET /Admin/Queries/Details/{id}` and `POST /Admin/Queries/Reply/{id}`.
3. **Receipt Financial Privacy Verification:** Automated assertions on cross-user receipt blocking triggered exception handling in PowerShell 5.1 on 302 redirects.  
   *Solution:* Handled response redirection headers with `-ErrorAction SilentlyContinue` and verified redirection to `/Account/AccessDenied`.
4. **File Upload Header Spoofing:** Standard file extension checks could allow disguised executables to be saved.  
   *Solution:* Integrated binary magic byte verification in `ImageUploadService.cs`.

---

## 6. Remaining Work to Project Submission (Task 20)

1. Final cross-documentation consistency review (Task 19 Step 11).
2. Final Task 19 completion sign-off and roadmap update.
3. Execution of **Task 20 (Final Backup and Submission)**:
   - Perform full clean solution rebuild.
   - Generate full SQL Server database backup (`GiveAidDb.bak`).
   - Create clean ZIP distribution package excluding build artifacts (`bin/`, `obj/`, `.vs/`).
   - Complete final centre submission checklist.

---

## 7. Supervisory Review and Comments

> [!NOTE]
> The section below is reserved for formal evaluative comments and feedback by the assigned academic supervisor or centre evaluator.

**Supervisor Observations / Remarks:**  
*[To be completed by Academic Evaluator during project defense / evaluation]*

\
\
\
**Evaluation Outcome:**  
[  ] Approved for Final Submission (Proceed to Task 20)  
[  ] Minor Revisions Requested  
[  ] Major Revisions Required  

**Evaluator Signature:** ___________________________ &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **Date:** _______________
