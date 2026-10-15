# Documentation: Help Centre, Contact and Queries (Task 12)

**Document ID:** `DOC-20`  
**Project Name:** Give-AID - NGO Management & Donation Website  
**Task Reference:** `TASK-12-Help-Contact-and-Queries`  
**Status:** Completed & Fully Verified  
**Date:** October 2026  

---

## 1. Feature Overview

The Help Centre, Contact and Queries module establishes a bi-directional communication channel between website visitors and the Give-AID operations team:

1. **Entity Schema & Database Alignment (`ContactQuery`):**
   - Stores visitor messages: `Id`, `SenderName`/`Name`, `SenderEmail`/`Email`, `SenderPhone`/`Phone`, `Subject`, `Message`, `SubmittedAt`/`CreatedAt`, `Status` (New, In Progress, Answered, Closed), and `AdminNotes`.
   - Admin Reply extensions: `AdminReply` (max 2000 chars) and `RepliedAt` timestamp.
   - User account association: `UserId` foreign key linked to `AspNetUsers(Id)` with `OnDelete(DeleteBehavior.SetNull)`.
   - Migration `20261015080938_AddQueryReplyFields` applied cleanly to SQL Server Express database `GiveAidDb`.

2. **Public Help Centre & FAQ Accordion (`HelpController`):**
   - Route: `GET /Help` (and `/Help?category={cat}`).
   - Organizes common questions into 5 distinct categories:
     - About Give-AID (mission, transparency, partner vetting).
     - Causes & Missions (core humanitarian pillars).
     - Contact & Inquiries (communication channels, tracking).
     - Programmes & Volunteering (event schedules, volunteer eligibility).
     - Donations & Payments (simulated demonstration model, zero payment card storage).
   - Interactive accordion layout built on Bootstrap 5 collapse controls with accessible heading anchors.

3. **Public Contact Us Form (`ContactController`):**
   - Route: `GET /Contact` & `POST /Contact`.
   - Accessible form fields: Name, Email Address, Contact Phone, Inquiry Subject, and Message.
   - Pre-fills registered user details when signed in (`User.Identity.IsAuthenticated == true`).
   - Server-side validation enforces required fields, maximum lengths, and valid RFC email formatting.
   - Links the inquiry to the authenticated member profile via server-side claims (`User.FindFirstValue(ClaimTypes.NameIdentifier)`), eliminating mass assignment vulnerabilities.
   - Assigns initial status `New`.
   - Anti-forgery validation (`[ValidateAntiForgeryToken]`) prevents cross-site request forgery.
   - Friendly confirmation banner on success and descriptive error messages if validation fails.

4. **Configurable Email Service (`EmailService` & `IEmailService`):**
   - Dependency injection registered interface `IEmailService`.
   - Reads SMTP settings from configuration (`EmailSettings:Host`, `Port`, `SenderEmail`, `Username`, `Password`).
   - Graceful simulation fallback: when SMTP credentials are not configured in development, logs the email details safely and simulates delivery without throwing exceptions or blocking query operations.
   - Generates responsive HTML notification templates for official administrative responses.

5. **Admin-Only Query Management (`AdminQueriesController`):**
   - Protected by `[Authorize(Roles = "Admin")]`.
   - `GET /Admin/Queries`: Dashboard with real-time KPI counters (Total, New, In Progress, Answered, Closed) and quick status filter pills.
   - `GET /Admin/Queries/Details/{id}`: Full inquiry inspection showing sender identity, verified member badge, subject, original message, and status.
   - `POST /Admin/Queries/Reply/{id}`: Official response workflow. Saves `AdminReply`, marks status as `Answered`, sets `RepliedAt` timestamp, records internal operational notes, and optionally dispatches email notifications.
   - `POST /Admin/Queries/UpdateStatus/{id}`: Quick status updates (`New`, `In Progress`, `Answered`, `Closed`) with audit notes.
   - `GET /Admin/Queries/Delete/{id}` & `POST /Admin/Queries/Delete/{id}`: Permanent deletion with confirmation safeguard.

6. **Role-Based Security & Protection:**
   - Anonymous visitors attempting to access `/Admin/Queries` are redirected to `/Account/Login`.
   - Regular users attempting to access `/Admin/Queries` are blocked and redirected to `/Account/AccessDenied`.

---

## 2. Test Execution & Verification

All 30 automated integration test assertions executed and passed against the running ASP.NET Core server and SQL Server Express `GiveAidDb`:

| # | Test Case | Target Route / Operation | Result | Details |
|---|---|---|:---:|---|
| 1 | Public Help Centre & FAQ | `GET /Help` | **PASS** | HTTP 200, FAQ accordion content rendered |
| 2 | Public FAQ Category Filter | `GET /Help?category=...` | **PASS** | HTTP 200, Category questions filtered correctly |
| 3 | Public Contact Us Page | `GET /Contact` | **PASS** | HTTP 200, Antiforgery token & fields present |
| 4 | Form Validation: Required Fields | `POST /Contact` (empty) | **PASS** | HTTP 200, Name, Email, Subject, Message errors |
| 5 | Form Validation: Invalid Email | `POST /Contact` (malformed) | **PASS** | HTTP 200, Invalid email address format rejected |
| 6 | Anonymous Inquiry Submission | `POST /Contact` | **PASS** | HTTP 302, Redirected with success message |
| 7 | SQL Server Persistence (Anonymous) | SQL database verification | **PASS** | Record created, `Status = 'New'`, `UserId = NULL` |
| 8 | Authenticated User Profile Pre-Fill | `GET /Contact` (logged in) | **PASS** | Detected member, pre-filled Name and Email |
| 9 | Authenticated Query Account Link | `POST /Contact` (logged in) | **PASS** | Query saved and linked to `UserId` in database |
| 10 | Anonymous Access to Admin Queries | `GET /Admin/Queries` | **PASS** | HTTP 302 redirected to `/Account/Login` |
| 11 | Regular User Access to Admin Queries | `GET /Admin/Queries` | **PASS** | HTTP 302 redirected to `/Account/AccessDenied` |
| 12 | Admin Queries Dashboard Access | `GET /Admin/Queries` | **PASS** | HTTP 200, KPI counters and table rendered |
| 13 | Admin Status Filter | `GET /Admin/Queries?status=New` | **PASS** | HTTP 200, Filters queue accurately |
| 14 | Admin Query Details | `GET /Admin/Queries/Details/{id}` | **PASS** | HTTP 200, Full inquiry and contact details |
| 15 | Nonexistent Query ID Handled | `GET /Admin/Queries/Details/99999` | **PASS** | HTTP 302 redirected gracefully with error alert |
| 16 | Update Query Status & Admin Notes | `POST /Admin/Queries/UpdateStatus/{id}` | **PASS** | Status updated to 'In Progress', notes saved in DB |
| 17 | Admin Official Reply & Status | `POST /Admin/Queries/Reply/{id}` | **PASS** | Reply saved, status 'Answered', timestamp set |
| 18 | Safe Deletion of Inquiry | `POST /Admin/Queries/Delete/{id}` | **PASS** | Query removed from SQL Server database |
| 19 | Regression: Home Page | `GET /` | **PASS** | HTTP 200 OK |
| 20 | Regression: About Us | `GET /Home/About` | **PASS** | HTTP 200 OK |
| 21 | Regression: Causes Directory | `GET /Causes` | **PASS** | HTTP 200 OK |
| 22 | Regression: Donation Portal | `GET /Donate` | **PASS** | HTTP 200 OK |
| 23 | Regression: Partners & NGOs | `GET /Partners` | **PASS** | HTTP 200 OK |
| 24 | Regression: Welfare Programmes | `GET /Programmes` | **PASS** | HTTP 200 OK |
| 25 | Regression: Photo Gallery | `GET /Gallery` | **PASS** | HTTP 200 OK |
| 26 | Regression: Admin Dashboard | `GET /Admin` | **PASS** | HTTP 200 OK, Inquiries module card verified |
| 27 | Regression: Admin NGOs | `GET /Admin/Ngos` | **PASS** | HTTP 200 OK |
| 28 | Regression: Admin Partners | `GET /Admin/Partners` | **PASS** | HTTP 200 OK |
| 29 | Regression: Admin Programmes | `GET /Admin/Programmes` | **PASS** | HTTP 200 OK |
| 30 | Regression: Admin Gallery | `GET /Admin/Gallery` | **PASS** | HTTP 200 OK |

**Overall Result:** `30 / 30 PASSED (100%)`.

---

## 3. Requirements Checklist Update

The following functional requirements are completely satisfied:
- **FR-049**: Frequently Asked Questions (FAQ) accordion (`/Help`).
- **FR-050**: Public Contact Us inquiry form (`/Contact`).
- **FR-051**: Server-side validation for contact submissions.
- **FR-052**: Admin inquiry dashboard (`/Admin/Queries`).
- **FR-053**: Admin query status tracking, internal notes, and official replies.
- **FR-054**: Contact queries linked to logged-in user profile.
