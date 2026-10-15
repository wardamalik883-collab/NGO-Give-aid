# Give-AID — Task 13: Simulated Donation Workflow and History

**Document ID:** `DOC-21`  
**Task Reference:** `TASK-13` — Simulated Donation Workflow and History  
**Dependencies:** `TASK-07` (Identity and Roles), `TASK-09` (Causes and Donation UI)  
**Status:** Completed & Verified  
**Date:** 15 October 2026  

---

## 1. Executive Summary

Task 13 delivers a comprehensive, safe, and testable end-to-end donation workflow for Give-AID. The implementation adheres strictly to the fundamental security rule: **real credit card numbers, CVVs, PINs, or banking credentials are never collected, stored, or transmitted**.

The module provides:
1. **Authenticated Donation Initiation (`/Donate`):** Requires sign-in to contribute, binds the donor identity from authenticated claims, validates campaign eligibility against active SQL Server records, and enforces boundary limits (`PKR 1` to `PKR 10,000,000`).
2. **Simulated Demo Payment Gateway (`/Donate/Payment` & `/Donate/ProcessPayment`):** A clearly labelled simulation environment featuring explicit deterministic test outcomes (*Simulate Success* vs. *Simulate Failure*) and anti-duplicate idempotency protection.
3. **Digital Confirmation & Receipts (`/Donate/Receipt/{id}`):** Generates unique non-sensitive transaction references (`GIVEAID-DON-YYYYMMDD-XXXXXX`), updates campaign raised amounts dynamically upon success, triggers simulated email notifications, and strictly forbids unauthorized access by third parties.
4. **Donor History Ledger (`/Donate/History`):** An isolated, authenticated personal ledger displaying a user's donations, status badges, KPI summary metrics, and direct links to digital receipts.
5. **Administrative Auditing Portal (`/Admin/Donations`):** Role-protected management console for platform administrators featuring live KPI summaries, status filtering (*All, Success, Failed*), campaign dropdown filters, search functionality, transaction details inspection, and reconciliation status adjustments.

---

## 2. Technical Architecture & Data Model

### 2.1 Model & Relationships (`Donation.cs`)

The `Donation` entity in `GiveAid.Web/Models/Donation.cs` models both financial audit records and demo simulation metadata:

| Field | Type | Description |
| :--- | :--- | :--- |
| `Id` | `int` | Primary Key, Identity. |
| `TransactionReference` | `nvarchar(60)` | Unique, non-sensitive transaction identifier (`GIVEAID-DON-YYYYMMDD-XXXXXX`). |
| `Amount` | `decimal(18,2)` | Donated sum in PKR. Validated between `1` and `10,000,000`. |
| `DonationDate` | `datetime2` | UTC timestamp of transaction creation. |
| `DonorName` | `nvarchar(100)` | Full name of donor (pre-filled from `ApplicationUser.FullName`). |
| `DonorEmail` | `nvarchar(100)` | Contact email address for simulated digital receipt delivery. |
| `DonorPhone` | `nvarchar(30)` | Optional telephone contact. |
| `PaymentStatus` | `nvarchar(25)` | Transaction status: `Completed`, `Failed`, or `Pending`. |
| `IsSimulated` | `bit` | Explicit demo flag (`true`). |
| `CauseId` | `int` | Foreign Key referencing target `Cause` (`OnDelete(DeleteBehavior.Restrict)`). |
| `UserId` | `nvarchar(450)` | Foreign Key referencing `AspNetUsers.Id` (`OnDelete(DeleteBehavior.SetNull)`). |
| `PaymentNotes` | `nvarchar(500)` | Simulated payment network reason / admin reconciliation audit trail. |
| `UpdatedAt` | `datetime2` | Optional UTC timestamp of administrative status modifications. |

### 2.2 EF Core Migration (`AddDonationUserAndAuditFields`)

Migration `20261015083642_AddDonationUserAndAuditFields.cs` applied the following modifications to SQL Server database `GiveAidDb`:
- Added column `PaymentNotes` (`nvarchar(500)`, nullable)
- Added column `UpdatedAt` (`datetime2`, nullable)
- Created index `IX_Donations_UserId` on `Donations(UserId)`
- Added foreign key constraint `FK_Donations_AspNetUsers_UserId` with `ON DELETE SET NULL` to preserve financial audit trails if a user account is removed.

---

## 3. Simulated Payment Workflow

```
[ Visitor ]
    │
    ▼
[ GET /Donate ] ── (Not Authenticated?) ──► [ Redirect: /Account/Login ]
    │ (Authenticated)
    ▼
[ 1. Select Active Cause & Amount ]
    │ (Server-side validation: 1 <= Amount <= 10,000,000; Active Cause)
    ▼
[ 2. POST /Donate/Index ] ──► [ 3. Render Simulated Payment Step ]
                                     │
                    ┌────────────────┴────────────────┐
                    ▼                                 ▼
           [ Simulate Success ]              [ Simulate Failure ]
                    │                                 │
                    ▼                                 ▼
          Status = "Completed"               Status = "Failed"
       Cause.CurrentAmount += Amount       Cause.CurrentAmount Unchanged
       Email Receipt Dispatched            Failure Notes Recorded
                    │                                 │
                    └────────────────┬────────────────┘
                                     ▼
                        [ Digital Receipt / Confirmation ]
                           /Donate/Receipt/{id}
                                     │
                    ┌────────────────┴────────────────┐
                    ▼                                 ▼
           [ My Donation History ]           [ Admin Audit Dashboard ]
             /Donate/History                    /Admin/Donations
```

---

## 4. Security & Privacy Guarantees

1. **Authentication Enforcement:** `DonateController` is decorated with `[Authorize]`. Anonymous visitors are immediately redirected to `/Account/Login?ReturnUrl=%2FDonate`.
2. **Server-Side Identity Binding:** The donor's `UserId` is retrieved solely from server claims (`User.FindFirstValue(ClaimTypes.NameIdentifier)`). Form tampering or query string injection cannot assign donations to another user.
3. **Receipt Ownership Authorization:** When `/Donate/Receipt/{id}` is requested, the system verifies `donation.UserId == currentUserId || User.IsInRole("Admin")`. Attempts by other registered users return `HTTP 403 Forbidden`.
4. **Isolated Personal History:** `/Donate/History` queries only records matching `d.UserId == currentUserId`.
5. **Zero Sensitive Card Credentials:** No credit card numbers, CVVs, expiration dates, ATM PINs, or banking passwords are ever requested, processed, or persisted.
6. **Idempotency Protection:** Payment requests carry an `IdempotencyToken` cached via `IMemoryCache` to prevent duplicate charges or multiple records resulting from rapid double-clicks.
7. **Role-Based Admin Protection:** `/Admin/Donations` requires `[Authorize(Roles = "Admin")]`. Regular users attempting direct access are blocked with `HTTP 403 Forbidden` / `AccessDenied`.

---

## 5. Verification & Test Execution Results

The automated integration suite in `scratch/test-task13.ps1` ran against `http://127.0.0.1:5113` and SQL Server database `GiveAidDb`:

```
==========================================================
 STARTING TASK 13 TEST SUITE: SIMULATED DONATION WORKFLOW
==========================================================
--- GROUP 1: Anonymous Access Control ---
  PASS [Test 1]: Anonymous GET /Donate redirects to /Account/Login
  PASS [Test 2]: Anonymous GET /Donate/History redirects to /Account/Login
  PASS [Test 3]: Anonymous GET /Admin/Donations redirects to /Account/Login

--- GROUP 2: Authenticated User Form & Validations ---
  PASS [Test 4]: User 1 (Zainab Tariq) login successful
  PASS [Test 5]: User 1 GET /Donate renders form with causes and pre-filled profile
  PASS [Test 6]: GET /Donate?causeId=1 pre-selects cause with contextual banner
  PASS [Test 7]: Zero amount rejected on server-side
  PASS [Test 8]: Negative amount rejected on server-side
  PASS [Test 9]: Amount > 10,000,000 rejected on server-side
  PASS [Test 10]: Invalid CauseId rejected on server-side

--- GROUP 3: Simulated Demo Payment Execution ---
  PASS [Test 11]: Valid donation details advance to Simulated Payment view
  PASS [Test 12]: POST ProcessPayment (Success) redirects to Receipt
  PASS [Test 13]: Receipt displays success badge, 5,000 PKR, unique reference, and demo notice
  PASS [Test 14]: Database record: Status=Completed, Cause CurrentAmount incremented by 5,000
  PASS [Test 15]: POST ProcessPayment (Failure) records failed attempt and redirects to Receipt
  PASS [Test 16]: Failed Receipt displays clear failure message and no false success
  PASS [Test 17]: Database check: Failed donation recorded as 'Failed', Cause amount untouched
  PASS [Test 18]: Idempotency prevents duplicate donation creation

--- GROUP 4: User Ownership & History Privacy ---
  PASS [Test 19]: User 1 /Donate/History displays their completed and failed donations with KPIs
  PASS [Test 20]: User 2 login successful
  PASS [Test 21]: User 2 cannot view User 1's receipt (HTTP 403 Forbidden / Access Denied)
  PASS [Test 22]: User 2's history is isolated and does not expose User 1's records

--- GROUP 5: Admin Donation Management ---
  PASS [Test 23]: Regular user blocked from /Admin/Donations (RBAC enforced)
  PASS [Test 24]: Administrator login successful
  PASS [Test 25]: Admin GET /Admin/Donations renders all records, KPIs, and filters
  PASS [Test 26]: Admin filter pills correctly filter Completed vs Failed donations
  PASS [Test 27]: Admin Details view renders complete transaction audit trail
  PASS [Test 28]: Admin can update donation status with audit notes in SQL Server

--- GROUP 6: Security & Navigation Integrity ---
  PASS [Test 29]: Security: Zero sensitive card columns (CardNumber/CVV/Pin) in SQL Server
  PASS [Test 30]: Navbar includes 'My Donations' link and Admin Dashboard includes Donations card

==========================================================
 TEST RUN COMPLETE: 30 Passed, 0 Failed (100% Pass Rate)
==========================================================
```

---

## 6. How to Test Demo Payments Manually

1. **Test a Successful Contribution:**
   - Sign in as a regular user (e.g. `testuser_task12@giveaid.org` / `User@12345`).
   - Navigate to `/Donate` or click **Donate Now** in the top navigation bar.
   - Select an active campaign (e.g. *Clean Drinking Water*) and choose an amount preset (e.g. `PKR 5,000`).
   - Click **Proceed to Demo Payment**.
   - Review the summary and leave **Simulate Success** selected.
   - Click **Execute Demo Payment (Success)**.
   - Observe the confirmation receipt with green success badge, unique reference `GIVEAID-DON-...`, and demo notice.
   - Click **View My Donation History** to see the entry and updated KPI totals.

2. **Test a Failed Transaction Flow:**
   - Initiate a donation for `PKR 3,000`.
   - On the payment simulation step, select **Simulate Failure**.
   - (Optional) Enter a test simulation reason: *"Simulated test decline"*.
   - Click **Execute Failed Payment Simulation**.
   - Observe the receipt displaying a red failure banner with explicit test decline explanation. No money is added to the campaign.
   - Open **My Donation History** to verify that the failed attempt is recorded as `Failed`.

3. **Test Administrative Auditing:**
   - Sign in as administrator (`admin@giveaid.org` / `Admin@123`).
   - Go to `/Admin` and click **Manage Donations**.
   - Review live KPIs (*Total Funds Raised, Total Records, Successful, Failed*).
   - Filter by status pills (*Success*, *Failed*) or search by donor / reference.
   - Click **Details** on any record to inspect audit timestamps and adjust statuses with notes.
