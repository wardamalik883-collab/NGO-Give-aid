# Task 09 — Causes Catalog & Donation Public UI

**Project:** Give-AID — NGO Management & Donation Website  
**Task Number:** 9  
**Status:** ✅ Completed  
**Completed On:** 2026-10-13

---

## 1. Overview

Task 09 implements the public-facing Causes catalog, in-depth Cause details view, and the donation initiation and validation subsystem.

Key architectural boundaries enforced:
- **Strict Scope Separation:** Task 09 is restricted to user interface, campaign discovery, and rigorous form/ModelState validation.
- **Zero Sensitive Data:** Real credit/debit card numbers, CVVs, expiration dates, or bank credentials are never requested or stored.
- **Pre-Payment Scope:** Simulated payment execution, gateway flows, receipt generation, and transaction history belong strictly to **Task 13: Simulated Donation Workflow and History**.

---

## 2. Pages & Features Implemented

### A. Causes Catalog (`/Causes` or `/Causes/Index` — `FR-015`, `FR-017`)
- Displays all active charitable campaigns in responsive cards.
- Interactive search bar filtering causes by title and short summary.
- Fundraising progress bar displaying current amount, target goal, and percentage funded.
- Direct **View Details** (`/Causes/Details/{id}`) and **Donate Now** (`/Donate?causeId={id}`) actions on every card.
- Accessible image fallback placeholder rendering the organizing NGO name when images are not uploaded.
- **Empty State (FR-017):** Friendly banner when no causes match search criteria or when the database has no records.

### B. Cause Details Page (`/Causes/Details/{id}` — `FR-016`)
- In-depth campaign narrative, objectives, and transparent audit pledge.
- Real-time fundraising tracker sidebar with goal, current raised amount, and remaining gap.
- Prominent Donate button carrying the pre-selected `causeId` directly into the donation form.
- Verified organizing NGO contact and registration card (name, registration number, address, email, phone).
- Handles nonexistent or invalid IDs gracefully by returning HTTP 404 (`NotFound()`).

### C. Donation Form (`/Donate` or `/Donate/Index` — `FR-020`, `FR-021`)
- **Cause Preselection & Context:** If user arrives from a cause details page, that cause is automatically preselected in the dropdown, and a contextual banner displays campaign details.
- **Authenticated Pre-Fill:** If donor is signed in, donor name, email, and phone are pre-populated automatically from `ApplicationUser`.
- **Preset Amount Buttons:** Quick-select buttons for PKR 1,000, PKR 2,500, PKR 5,000, PKR 10,000, and PKR 25,000 updating the input field.
- **Custom Amount Input:** Numeric entry supporting amounts from PKR 1 to PKR 10,000,000.
- **Donor Fields:** Full Name, Email, and Phone number (optional).
- **Scope Notice:** Clear notice stating that real cards are never charged and simulated payment processing belongs to Task 13.

### D. Validation & Confirmation (`/Donate/Confirmation`)
- Server-side validation via `ModelState`:
  - Selected cause must exist and have `IsActive == true` in database.
  - Donation amount must be numeric, > 0, and <= 10,000,000.
  - Donor full name is required (max 100 characters).
  - Donor email is required and validated against email syntax.
- **Validation Review View (`Confirmation.cshtml`):** Displays a verified summary of the donation details with a next-step roadmap notice.

---

## 3. ViewModels Created

1. `CausesIndexViewModel`: Encapsulates active cause cards list and search query.
2. `CauseDetailsViewModel`: Encapsulates full narrative description, targets, and organizing NGO profile.
3. `DonationFormViewModel`: Strictly typed form model with DataAnnotations validation attributes and `AvailableCauses` dropdown list.
4. `DonationConfirmationViewModel`: Encapsulates validated donation details for review.

---

## 4. Test & Verification Results

All tests were executed against live application instances:

| Test Scenario | Action | Expected Outcome | Result |
|---|---|---|---|
| Causes Listing | `GET /Causes` | HTTP 200 with active campaign cards | ✅ Passed |
| Empty State | `GET /Causes?q=xyznonexistent` | HTTP 200 with empty-state notice | ✅ Passed |
| Cause Details | `GET /Causes/Details/1` | HTTP 200 with narrative & NGO card | ✅ Passed |
| Nonexistent Cause Details | `GET /Causes/Details/9999` | HTTP 404 NotFound | ✅ Passed |
| Donation Form | `GET /Donate` | HTTP 200 with active cause dropdown | ✅ Passed |
| Cause Preselection | `GET /Donate?causeId=1` | HTTP 200 with preselected cause banner | ✅ Passed |
| Valid Submission | `POST /Donate` | HTTP 200 returning Confirmation view | ✅ Passed |
| Empty / Missing Amount | `POST /Donate` with `Amount=""` | "Please enter a donation amount." | ✅ Passed |
| Zero / Negative Amount | `POST /Donate` with `Amount="0"` | "Donation amount must be between PKR 1 and PKR 10,000,000." | ✅ Passed |
| Excessive Amount | `POST /Donate` with `Amount="15000000"` | "Donation amount must be between PKR 1 and PKR 10,000,000." | ✅ Passed |
| Missing Donor Name | `POST /Donate` with `DonorName=""` | "Please enter your full name." | ✅ Passed |
| Missing Donor Email | `POST /Donate` with `DonorEmail=""` | "Please enter your email address." | ✅ Passed |
| Invalid Email Format | `POST /Donate` with `DonorEmail="bad"` | "Please enter a valid email address." | ✅ Passed |
| Nonexistent Cause ID | `POST /Donate` with `CauseId="9999"` | "The selected charitable cause does not exist or is currently inactive." | ✅ Passed |

`dotnet build` produced **0 errors** and **0 warnings**.
