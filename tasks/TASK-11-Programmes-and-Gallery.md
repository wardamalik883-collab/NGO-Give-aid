# TASK-11 — Programmes and gallery management

**Status:** Done  
**Dependencies:** Tasks 06 and 07  
**Completed Date:** 2026-10-14  

## Objective

Build welfare programme/event pages and the related gallery management.

## Steps

1. [x] Create Programme and GalleryImage models with dates, locations, descriptions and safe image path fields (`Models/Programme.cs`, `Models/GalleryImage.cs`, `ViewModels/ProgrammeFormViewModel.cs`, `ViewModels/GalleryViewModels.cs`).
2. [x] Add migrations and apply them (`20261014053310_AddProgrammeDatesAndAudit` created and applied to SQL Server Express database `GiveAidDb`).
3. [x] Build public programme list and detail pages, separating upcoming and past programmes (`ProgrammesController.cs`, `Views/Programmes/Index.cshtml`, `Views/Programmes/Details.cshtml`).
4. [x] Build admin-only programme CRUD (`AdminProgrammesController.cs`, `Views/Admin/Programmes/*.cshtml`).
5. [x] Build admin-only gallery upload/list/delete screens with file type and size checks (`AdminGalleryController.cs`, `Views/Admin/Gallery/*.cshtml`).
6. [x] Associate gallery images with a programme when applicable (`ProgrammeId` dropdown in upload/edit; modal lightbox display on Programme Details and Gallery pages).
7. [x] Show a friendly empty state when no programmes or images exist (Empty state cards implemented across public and admin views for both modules).

## Acceptance criteria

- [x] Programmes can be created and edited by admin and displayed publicly.
- [x] Upcoming/past filtering uses programme dates.
- [x] Gallery upload and deletion work with safe filenames and authorization.

## Expected outputs

- `Programme/Gallery models and views`: `Models/Programme.cs`, `Models/GalleryImage.cs`, `Controllers/ProgrammesController.cs`, `Controllers/GalleryController.cs`, `Controllers/AdminProgrammesController.cs`, `Controllers/AdminGalleryController.cs`, `Views/Programmes/*.cshtml`, `Views/Gallery/*.cshtml`, `Views/Admin/Programmes/*.cshtml`, `Views/Admin/Gallery/*.cshtml`
- `Migration`: `Migrations/20261014053310_AddProgrammeDatesAndAudit.cs` applied to `GiveAidDb` on SQL Server Express.
- `Upload handling`: `Services/ImageUploadService.cs` (magic byte header inspection, configurable max size up to 5MB, GUID filenames, physical file deletion on image delete/replace).

## Blocker log

None.

## Completion notes

- **Date completed:** 2026-10-14
- **Tests performed:**
  1. `dotnet build GiveAid.sln` succeeded with 0 warnings and 0 errors.
  2. Live integration test suite (37/37 assertions passed):
     - `GET /Programmes`: HTTP 200 OK, dynamic rendering of upcoming and past programmes.
     - `GET /Programmes/Details/{id}`: HTTP 200 OK, displays programme schedule, venue, coordinator, and related gallery captures.
     - `GET /Programmes/Details/99999`: HTTP 404 NotFound handled gracefully.
     - `GET /Gallery`: HTTP 200 OK, responsive image grid with category filtering.
     - `GET /Gallery?programmeId=99999`: HTTP 200 OK, friendly empty state rendered.
     - `GET /Admin/Programmes` (Anonymous): HTTP 302 redirected to `/Account/Login`.
     - `GET /Admin/Gallery` (Anonymous): HTTP 302 redirected to `/Account/Login`.
     - `GET /Admin/Programmes` (Ordinary User): HTTP 302 redirected to `/Account/AccessDenied`.
     - `GET /Admin/Gallery` (Ordinary User): HTTP 302 redirected to `/Account/AccessDenied`.
     - `POST /Account/Login` (Admin): Authenticated successfully with Admin role verified.
     - `GET /Admin/Programmes` (Admin): HTTP 200 OK.
     - `GET /Admin/Gallery` (Admin): HTTP 200 OK.
     - `POST /Admin/Programmes/Create` (`EndDate < StartDate`): Server validation rejected invalid date range.
     - `POST /Admin/Programmes/Create` (Invalid `.exe` extension): Rejected with validation error.
     - `POST /Admin/Programmes/Create` (Oversized > 5MB): Rejected with size validation error.
     - `POST /Admin/Programmes/Create` (Valid PNG banner): Created Programme, unique GUID filename saved to `/uploads/programmes/`.
     - `GET /Admin/Programmes/Details/{id}`: HTTP 200 OK.
     - `POST /Admin/Programmes/Edit/{id}` (No new file): Edited record, preserved existing banner image path in DB.
     - `GET /Programmes` & `GET /Programmes/Details/{id}`: Verified updated Programme appears dynamically in Upcoming section.
     - `POST /Admin/Gallery/Upload` (Missing file): Rejected with validation error.
     - `POST /Admin/Gallery/Upload` (Invalid `.exe` extension): Rejected with file type error.
     - `POST /Admin/Gallery/Upload` (Valid PNG + linked Programme): Uploaded photo saved to `/uploads/gallery/` and linked to Programme.
     - `GET /Programmes/Details/{id}`: Verified linked photo renders in Programme Gallery section.
     - `GET /Gallery` & `GET /Gallery?programmeId={id}`: Verified photo renders in public gallery and filter matches.
     - `POST /Admin/Gallery/Edit/{id}`: Updated photo title and accessibility alt text.
     - `POST /Admin/Gallery/Delete/{id}`: Deleted gallery record and verified physical file removed from disk.
     - `POST /Admin/Programmes/Delete/{id}`: Deleted programme and verified banner image removed from disk.
     - Regression tests: `/` (200 OK), `/Home/About` (200 OK), `/Causes` (200 OK), `/Donate` (200 OK), `/Partners` (200 OK), `/Admin/Ngos` (200 OK), `/Admin/Partners` (200 OK).
- **Evidence / screenshot path:** `docs/19-Programmes-and-Gallery.md`
- **Git commit:** Pending user approval
- **Follow-up work, if any:** None for Task 11. Ready for Task 12 / Task 14 when scheduled.
