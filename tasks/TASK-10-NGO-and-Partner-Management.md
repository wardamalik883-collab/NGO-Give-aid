# TASK-10 — NGO and partner management

**Status:** Done  
**Dependencies:** Tasks 06 and 07  
**Completed Date:** 2026-10-13  

## Objective

Allow admins to maintain NGO and partner information and display it publicly.

## Steps

1. [x] Create NGO and Partner models with required fields and validation (`Models/NGO.cs`, `Models/Partner.cs`, `ViewModels/NgoFormViewModel.cs`, `ViewModels/PartnerFormViewModel.cs`).
2. [x] Create migrations and apply them to the development database (EF Core migrations applied to SQL Server Express database `GiveAidDb`).
3. [x] Build admin-only list/create/edit/details/delete screens for NGOs and Partners (`AdminNgosController.cs`, `AdminPartnersController.cs`, and corresponding Razor views in `Views/Admin/Ngos/` and `Views/Admin/Partners/`).
4. [x] Validate image/logo uploads for extension, size and generated filenames; store paths, not raw image bytes unless intentionally designed (`Services/ImageUploadService.cs` enforcing extensions, magic bytes, max 2MB, unique GUID filenames, safe relative paths, directory traversal prevention, and old file cleanup).
5. [x] Build public NGO and partner listing/details views (`PartnersController.cs`, `Views/Partners/Index.cshtml`, `Views/Partners/NgoDetails.cshtml`, `Views/Partners/PartnerDetails.cshtml`).
6. [x] Show empty states and friendly errors; confirm delete actions before removing records (`Delete.cshtml` with warning alerts, empty state banners in index views, graceful 404 responses for nonexistent IDs).

## Acceptance criteria

- [x] Admin-only CRUD persists correctly.
- [x] Public pages show saved records.
- [x] Invalid uploads are rejected and admin authorization is enforced.

## Expected outputs

- `NGO/Partner models and views`: `Models/NGO.cs`, `Models/Partner.cs`, `Views/Admin/Ngos/*.cshtml`, `Views/Admin/Partners/*.cshtml`, `Views/Partners/*.cshtml`
- `Migration`: Verified schema in `GiveAidDb` on SQL Server Express with `InitialCreate` and `AddIdentity`
- `Upload validation logic`: `Services/ImageUploadService.cs`

## Blocker log

None.

## Completion notes

- **Date completed:** 2026-10-13
- **Tests performed:**
  1. `dotnet build GiveAid.sln` succeeded with 0 warnings and 0 errors.
  2. Live integration test suite (31/31 assertions passed):
     - `GET /Partners`: HTTP 200 OK, dynamic rendering of NGOs and Partners.
     - `GET /Partners/NgoDetails/1`: HTTP 200 OK, contact details and active causes card.
     - `GET /Partners/NgoDetails/99999`: HTTP 404 NotFound handled gracefully.
     - `GET /Partners/PartnerDetails/99999`: HTTP 404 NotFound handled gracefully.
     - `GET /Admin/Ngos` (Anonymous): HTTP 302 redirected to `/Account/Login?ReturnUrl=/Admin/Ngos`.
     - `GET /Admin/Partners` (Anonymous): HTTP 302 redirected to `/Account/Login?ReturnUrl=/Admin/Partners`.
     - `GET /Admin/Ngos` (Ordinary User): HTTP 302 redirected to `/Account/AccessDenied?ReturnUrl=/Admin/Ngos`.
     - `GET /Admin/Partners` (Ordinary User): HTTP 302 redirected to `/Account/AccessDenied?ReturnUrl=/Admin/Partners`.
     - `POST /Account/Login` (Admin): Authenticated successfully with Admin role verified.
     - `GET /Admin/Ngos` (Admin): HTTP 200 OK.
     - `GET /Admin/Partners` (Admin): HTTP 200 OK.
     - `POST /Admin/Ngos/Create` (Invalid `.exe` extension): Rejected with validation error, record not created.
     - `POST /Admin/Ngos/Create` (Oversized > 2MB): Rejected with size validation error, record not created.
     - `POST /Admin/Ngos/Create` (Valid PNG): Created NGO, unique filename stored in DB, saved to `/uploads/ngos/`.
     - `GET /Admin/Ngos/Details/{id}`: HTTP 200 OK.
     - `POST /Admin/Ngos/Edit/{id}` (No new file): Edited record, preserved existing logo path in DB.
     - `GET /Partners` & `GET /Partners/NgoDetails/{id}`: Verified updated NGO appears dynamically on public pages.
     - `POST /Admin/Ngos/Delete/{id}`: Deleted record from DB and verified uploaded image removed from disk safely.
     - `POST /Admin/Partners/Create` (Valid JPEG): Created Partner, unique filename stored in DB, saved to `/uploads/partners/`.
     - `GET /Admin/Partners/Details/{id}`: HTTP 200 OK.
     - `POST /Admin/Partners/Edit/{id}` (No new file): Edited record, preserved existing logo path in DB.
     - `GET /Partners` & `GET /Partners/PartnerDetails/{id}`: Verified updated Partner appears dynamically on public pages.
     - `POST /Admin/Partners/Delete/{id}`: Deleted record from DB and verified uploaded image removed from disk safely.
     - Regression tests: `/` (200 OK), `/Home/About` (200 OK), `/Causes` (200 OK), `/Donate` (200 OK).
- **Evidence / screenshot path:** `docs/18-NGO-and-Partner-Management.md`
- **Git commit:** Pending user approval
- **Follow-up work, if any:** None for Task 10. Ready for Task 11 (Programmes and Gallery).
