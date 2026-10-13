# TASK-09 — Causes and donation user interface

**Status:** Completed  
**Dependencies:** Task 05  
**Completed Date:** 2026-10-13  

## Objective

Build the public cause listing and donation form without implementing payment processing yet.

## Steps

1. [x] Create a cause listing/details view with cause name, description, progress, and image fallback (`CausesController`, `Views/Causes/Index.cshtml`, `Views/Causes/Details.cshtml`).
2. [x] Create a donation form with selected cause, amount presets, custom amount, and required donor information (`DonateController`, `Views/Donate/Index.cshtml`).
3. [x] Use a ViewModel and validation attributes rather than trusting raw form values (`DonationFormViewModel.cs`, `DonationConfirmationViewModel.cs`).
4. [x] Provide helpful validation messages and a clear confirmation/result design (`Views/Donate/Confirmation.cshtml`).
5. [x] Do not request real card details in this initial UI task; payment simulation is handled in Task 13 (verified zero card fields requested or stored).
6. [x] Use an empty state when no causes have been created yet (verified in `Views/Causes/Index.cshtml` with custom search query test).

## Acceptance criteria

- [x] Cause list and donation form render cleanly (HTTP 200 OK verified).
- [x] Invalid amount (empty, zero, negative, > 10M) and missing required fields (name, email, invalid email, unselected cause) show useful validation errors.
- [x] The form does not collect or persist real payment-card details.

## Expected outputs

- `Cause and donation Razor views`: `Views/Causes/Index.cshtml`, `Views/Causes/Details.cshtml`, `Views/Donate/Index.cshtml`, `Views/Donate/Confirmation.cshtml`
- `Donation ViewModel`: `DonationFormViewModel.cs`, `DonationConfirmationViewModel.cs`, `CausesIndexViewModel.cs`, `CauseDetailsViewModel.cs`

## Blocker log

None.

## Completion notes

- **Date completed:** 2026-10-13
- **Tests performed:**
  1. `dotnet build` succeeded with 0 errors and 0 warnings.
  2. Live smoke tests against port 5100:
     - `GET /Causes`: HTTP 200 OK. Verified active causes render with titles, NGO names, progress bars, Details and Donate links.
     - `GET /Causes?q=xyznonexistent`: HTTP 200 OK. Verified friendly empty-state banner renders.
     - `GET /Causes/Details/1`: HTTP 200 OK. Verified full narrative, targets, and NGO contact card.
     - `GET /Causes/Details/9999`: HTTP 404 NotFound handled correctly.
     - `GET /Donate`: HTTP 200 OK. Verified form with cause dropdown and amount presets.
     - `GET /Donate?causeId=1`: HTTP 200 OK. Verified pre-selected cause banner.
     - `POST /Donate` (Valid): Verified redirect to Confirmation view with validated summary.
     - `POST /Donate` (Invalid): Tested empty amount, zero amount, negative amount, amount > 10M, missing donor name, missing email, invalid email format, and nonexistent cause ID (9999). Verified appropriate validation errors on all fields.
- **Evidence / screenshot path:** `docs/17-Causes-and-Donation-UI.md`
- **Git commit:** Pending user approval
- **Follow-up work, if any:** None for Task 09. Await approval before Task 10.

