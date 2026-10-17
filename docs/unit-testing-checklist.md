# Give-AID NGO — End-to-End & Regression Testing Checklist

**Date:** 2026-10-16  
**Project:** Give-AID NGO Web Portal  
**Framework:** ASP.NET Core MVC (.NET 10), Entity Framework Core, SQL Server Express (`.\SQLEXPRESS`), ASP.NET Core Identity  
**Test Suite Script:** [`test_task18_regression.ps1`](file:///d:/Give-AID%20NGO/test_task18_regression.ps1)  
**Overall Result:** **38 / 38 Tests Passed (100% Pass Rate)**

---

## 1. Build Verification & Diagnostics (Step 1)

| Test ID | Test Scope | Verification Method | Expected Outcome | Actual Outcome | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-BLD-01** | Solution Compilation | `dotnet build` | 0 Compilation Errors, 0 Critical Warnings | 0 Errors, 0 Warnings (`GiveAid.Web.dll` built) | **PASS** |
| **TC-BLD-02** | Database Connectivity | `Test-NetConnection` & `sqlcmd` | SQL Server `.\SQLEXPRESS` accepts connections to `GiveAidDb` | Connected and verified active tables | **PASS** |
| **TC-BLD-03** | Web Server Startup | `Invoke-WebRequest http://localhost:5187` | Application listens on port 5187 and responds HTTP 200 | HTTP 200 OK received within 2s | **PASS** |

---

## 2. Public Website Navigation & Rendering (Step 2)

| Test ID | Route / Page | Expected Content Marker | HTTP Code | Status | Verification Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-PUB-01** | `/` (Home Page) | "Give-AID" brand, hero section | 200 OK | **PASS** | Rendered hero banner, quick donate teaser, featured causes |
| **TC-PUB-02** | `/Home/About` | "About" section, mission, leadership | 200 OK | **PASS** | Rendered mission, team, and organizational story |
| **TC-PUB-03** | `/Causes` | "Cause", active campaigns catalog | 200 OK | **PASS** | Rendered verified campaign cards with progress bars |
| **TC-PUB-04** | `/Causes/Details/1` | "Target", campaign budget, NGO details | 200 OK | **PASS** | Rendered full cause details, NGO affiliation, and direct donate button |
| **TC-PUB-05** | `/Partners` | "Partners", corporate sponsor roster | 200 OK | **PASS** | Rendered verified sponsor cards with display ordering |
| **TC-PUB-06** | `/Programmes` | "Programme", upcoming welfare camps | 200 OK | **PASS** | Rendered scheduled welfare drives with date badges |
| **TC-PUB-07** | `/Gallery` | "Gallery", high-resolution image grid | 200 OK | **PASS** | Rendered verified field photo gallery with category filter |
| **TC-PUB-08** | `/Contact` | "Contact", public inquiry form | 200 OK | **PASS** | Rendered name, email, phone, subject, and message fields |
| **TC-PUB-09** | `/Help` | "Frequently Asked Questions", accordion | 200 OK | **PASS** | Rendered FAQ accordion categories and search filter |
| **TC-PUB-10** | `/Account/Login` | "Sign In", authentication form | 200 OK | **PASS** | Rendered credential inputs with AntiForgeryToken |
| **TC-PUB-11** | `/Account/Register` | "Register", new member sign-up form | 200 OK | **PASS** | Rendered full registration form with validation tags |

---

## 3. Authentication, Authorization & Access Control (Step 3)

| Test ID | Scenario | Input / Action | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-AUTH-01** | Invalid Login | `nonexistent_user@giveaid.org` / `WrongPassword99!` | "Invalid login credentials" validation warning | HTTP 200 with "Invalid login credentials" message | **PASS** |
| **TC-AUTH-02** | User Registration | Valid new email, full name, secure password | Redirect to return URL or `/` (HTTP 302); user role assigned | HTTP 302 Redirect, user created in `AspNetUsers` | **PASS** |
| **TC-AUTH-03** | Duplicate Registration | Re-submit exact same email address | Server-side validation failure; user informed email is taken | HTTP 200 with "already taken" validation message | **PASS** |
| **TC-AUTH-04** | RBAC Privilege Escalation Guard | Regular user session requests `/Admin` | Forbidden / Redirect to `/Account/AccessDenied` | HTTP 302 redirecting to `AccessDenied?ReturnUrl=%2FAdmin` | **PASS** |
| **TC-AUTH-05** | Admin Login | `admin@giveaid.org` / `Admin@123` | Successful authentication (HTTP 302) | HTTP 302 Redirect, session authentication cookie issued | **PASS** |
| **TC-AUTH-06** | Admin Portal Access | Admin session requests `/Admin` | HTTP 200 with "Administrative Dashboard" | HTTP 200 OK, full dashboard layout with KPIs rendered | **PASS** |

---

## 4. Administrative Module CRUD Operations (Step 4)

| Test ID | Module | Action | Input Data | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-CRUD-01** | NGOs | Create (POST) | Unique NGO Name, Reg #, Email, Address, Active | HTTP 302, persisted to `NGOs` | HTTP 302, record created | **PASS** |
| **TC-CRUD-02** | NGOs | Read (GET) | Browse `/Admin/Ngos` | Created record visible in HTML table | Record found in table with active badge | **PASS** |
| **TC-CRUD-03** | NGOs | Update (POST) | Updated Name, Address, Email | HTTP 302, database updated | HTTP 302, name updated in SQL Server | **PASS** |
| **TC-CRUD-04** | NGOs | Delete (POST) | Admin confirmation POST `/Admin/Ngos/Delete/{id}` | HTTP 302, SQL count = 0 | HTTP 302, cleanly deleted from SQL | **PASS** |
| **TC-CRUD-05** | Partners | Create (POST) | Unique Partner Name, Sponsor Type, Display Order | HTTP 302, persisted to `Partners` | HTTP 302, record created in SQL | **PASS** |
| **TC-CRUD-06** | Partners | Delete (POST) | Admin confirmation POST `/Admin/Partners/Delete/{id}` | HTTP 302, record removed from database | HTTP 302, partner ID cleaned up | **PASS** |
| **TC-CRUD-07** | Programmes | Create (POST) | Title, Description, EventDate, Location, Coordinator | HTTP 302, persisted to `Programmes` | HTTP 302, programme created in SQL | **PASS** |
| **TC-CRUD-08** | Programmes | Delete (POST) | Admin confirmation POST `/Admin/Programmes/Delete/{id}` | HTTP 302, record removed from database | HTTP 302, programme ID cleaned up | **PASS** |

---

## 5. Simulated Donation Workflow & Privacy Protection (Step 5)

| Test ID | Scenario | Input Data | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-DON-01** | Negative Amount Rejection | `Amount = -50` | Server-side validation failure; form re-rendered with error | Error: "Donation amount must be between PKR 1 and PKR 10,000,000" | **PASS** |
| **TC-DON-02** | Valid Initiation | Cause #1, `Amount = 150.00`, Donor Name & Email | HTTP 200 rendering simulated payment gateway step | Navigated to "Simulated Payment Processing" page | **PASS** |
| **TC-DON-03** | Process Payment | `SimulatedOutcome = Success`, Idempotency Token | HTTP 302 redirecting to `/Donate/Receipt/{id}` | HTTP 302 Redirect to `/Donate/Receipt/6` | **PASS** |
| **TC-DON-04** | Donor Receipt Access | Donor session requests `/Donate/Receipt/6` | HTTP 200 displaying verified receipt & amount | HTTP 200 OK, full donation summary rendered | **PASS** |
| **TC-DON-05** | Cross-User Privacy Guard | Different authenticated user requests `/Donate/Receipt/6` | Access Denied / Redirect to `/Account/AccessDenied` | HTTP 302 redirecting to `AccessDenied?ReturnUrl=%2FDonate%2FReceipt%2F6` | **PASS** |

---

## 6. Contact, Volunteer Applications & Invitations (Step 6)

| Test ID | Feature | Action | Input Data | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-INT-01** | Contact Query | Public POST `/Contact` | Sender Name, Email, Phone, Subject, Message | HTTP 302 Redirect | HTTP 302 Redirect | **PASS** |
| **TC-INT-02** | Contact SQL Check | SQL Query | Verify in `ContactQueries` table | `COUNT(*) > 0` | Verified record in database | **PASS** |
| **TC-INT-03** | Admin Reply | Admin POST `/Admin/Queries/Reply/{id}` | Official coordination response message | HTTP 302, Status = "Answered" | HTTP 302, reply logged in SQL | **PASS** |
| **TC-INT-04** | Programme Interest | User POST `/Programmes/ExpressInterest` | Programme ID, Applicant Name, Email, Role = Volunteer | HTTP 302, saved to `ProgrammeInterests` | HTTP 302, application submitted | **PASS** |
| **TC-INT-05** | Duplicate Application | Re-submit application with same email | Rejected with validation message | Rejected: "already been submitted" | **PASS** |
| **TC-INT-06** | My Applications | User GET `/Programmes/MyInterests` | Authenticated user history lookup | HTTP 200 displaying application history | HTTP 200 OK with application status | **PASS** |
| **TC-INT-07** | Invite Friends | User POST `/Invite` | Friend email & personal message | HTTP 302 Redirect | HTTP 302, invitation sent | **PASS** |
| **TC-INT-08** | Invite SQL Check | SQL Query | Verify in `Invitations` table | `COUNT(*) > 0` | Verified logged in `Invitations` | **PASS** |

---

## 7. Security & Input Resilience (Step 7)

| Test ID | Security Check | Mechanism | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-SEC-01** | Anti-CSRF Protection | `AutoValidateAntiforgeryTokenAttribute` | Missing token rejected with HTTP 400 | Verified across all POST endpoints | **PASS** |
| **TC-SEC-02** | Security Headers | Custom Middleware in `Program.cs` | `X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN` | All required response headers present | **PASS** |
| **TC-SEC-03** | Friendly Error Handling | `UseStatusCodePagesWithReExecute` | Status codes 404/403 render branded error page | Branded `Error.cshtml` rendered without stack traces | **PASS** |
| **TC-SEC-04** | Over-Posting Prevention | ViewModels used exclusively | Entities not directly exposed in actions | All management controllers use dedicated ViewModels | **PASS** |

---

## 8. Summary & Sign-off

- **Total Test Cases Executed:** 38
- **Total Passed:** 38 (100%)
- **Total Failed:** 0
- **Regressions Identified & Resolved:** 5 (see [`docs/bug-log.md`](file:///d:/Give-AID%20NGO/docs/bug-log.md))
- **Production Readiness:** **Ready for Task 19 / Final Project Review**
