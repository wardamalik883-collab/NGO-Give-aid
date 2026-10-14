# Documentation: Programmes and Gallery Management (Task 11)

**Document ID:** `DOC-19`  
**Project Name:** Give-AID - NGO Management & Donation Website  
**Task Reference:** `TASK-11-Programmes-and-Gallery`  
**Status:** Completed & Fully Verified  
**Date:** October 2026  

---

## 1. Feature Overview

The Programmes and Gallery Management module delivers comprehensive administration and public discovery for community welfare initiatives, event schedules, and photo captures:

1. **Models & Schema Alignment:**
   - `Programme.cs`: Welfare programmes and humanitarian drives (`Id`, `Title`, `Description`, `EventDate` / `StartDate`, `EndDate`, `Location`, `CoordinatorName`, `CoordinatorContact`, `BannerImagePath` / `ImagePath`, `Status`, `CreatedAt`, `UpdatedAt`, and `GalleryImages` collection).
   - `GalleryImage.cs`: High-resolution photo asset records (`Id`, `Title`, `Caption`, `AltText` for WCAG AA compliance, `FilePath` / `ImagePath`, `UploadedAt`, `ProgrammeId` optional foreign key, and `Programme` navigation property).
   - Foreign key relationship configured with `OnDelete(DeleteBehavior.SetNull)`: deleting a programme safely retains associated gallery captures and physical files.
   - Database migration `20261014053310_AddProgrammeDatesAndAudit` applied cleanly to SQL Server Express database `GiveAidDb`.

2. **Asset Upload & File Sanitization (`ImageUploadService`):**
   - Configurable size cap up to **5 MB** for high-resolution event banners and gallery photos.
   - Validates file extensions (`.jpg`, `.jpeg`, `.png`, `.webp`) and verifies magic byte headers to prevent disguised executables.
   - Assigns cryptographically secure unique GUID filenames (`Guid.NewGuid():N + extension`), storing relative paths in the database.
   - Safe deletion routines remove physical files from `wwwroot/uploads/` upon record deletion or image replacement, with path traversal prevention.

3. **Admin-Only CRUD Operations (`[Authorize(Roles = "Admin")]`):**
   - **Programmes (`AdminProgrammesController`):**
     - `GET /Admin/Programmes`: Listing of all programmes with status badges, dates, venues, banner previews, and linked photo counts.
     - `GET /Admin/Programmes/Details/{id}`: Detailed review with coordinator contact and linked gallery thumbnails.
     - `GET /Admin/Programmes/Create` & `POST /Admin/Programmes/Create`: Server-side validation enforcing `EndDate >= StartDate` via `IValidatableObject`, banner image upload.
     - `GET /Admin/Programmes/Edit/{id}` & `POST /Admin/Programmes/Edit/{id}`: Edit details, preserve existing banner when no new file is uploaded, replace banner cleanly.
     - `GET /Admin/Programmes/Delete/{id}` & `POST /Admin/Programmes/Delete/{id}`: Delete confirmation, removes banner from disk, sets `ProgrammeId = NULL` on linked gallery images.
   - **Gallery (`AdminGalleryController`):**
     - `GET /Admin/Gallery`: Photo list with thumbnails, narrative captions, WCAG AA alt text tags, linked programme badges, and upload timestamps.
     - `GET /Admin/Gallery/Details/{id}`: High-res inspection with storage path and event linkage.
     - `GET /Admin/Gallery/Upload` & `POST /Admin/Gallery/Upload`: Photo file upload with programme dropdown selection and required accessibility alternative text.
     - `GET /Admin/Gallery/Edit/{id}` & `POST /Admin/Gallery/Edit/{id}`: Update captions, alt text, linked programme, or replace image file.
     - `GET /Admin/Gallery/Delete/{id}` & `POST /Admin/Gallery/Delete/{id}`: Confirmation dialog, permanently deletes database entity and physical file from disk.

4. **Public Programmes & Photo Gallery Discovery:**
   - `GET /Programmes`: Categorizes welfare programmes into **Upcoming Programmes** (`(EndDate ?? EventDate) >= today`, sorted nearest first) and **Past Programmes** (`(EndDate ?? EventDate) < today`, sorted most recent first). Displays friendly empty state when no programmes are scheduled.
   - `GET /Programmes/Details/{id}`: Displays full agenda, start/end dates, location, coordinator details, and dedicated **Programme Gallery** section showcasing related field photos with interactive modal lightbox inspection.
   - `GET /Gallery`: Responsive image grid with filter pills for all photos and individual programmes. Click-to-enlarge modal lightbox with accessible image rendering, title, caption, and programme badge.
   - Graceful 404 NotFound responses for nonexistent IDs.

5. **Security & Role-Based Access Control:**
   - Anonymous requests to `/Admin/Programmes` or `/Admin/Gallery` are redirected to `/Account/Login`.
   - Regular user requests to `/Admin/Programmes` or `/Admin/Gallery` are rejected and redirected to `/Account/AccessDenied`.

---

## 2. Test Execution & Verification

All 37 automated integration tests executed against local SQL Server Express and the running ASP.NET Core server:

| # | Test Name | Target Route / Operation | Result | Details |
|---|---|---|---|---|
| 1 | Public Programmes Directory | `GET /Programmes` | **PASS** | HTTP 200, renders upcoming/past or empty directory |
| 2 | Public Programme Nonexistent ID | `GET /Programmes/Details/99999` | **PASS** | HTTP 404 NotFound handled gracefully |
| 3 | Public Photo Gallery | `GET /Gallery` | **PASS** | HTTP 200, renders community gallery header |
| 4 | Public Gallery Filter Nonexistent Programme | `GET /Gallery?programmeId=99999` | **PASS** | HTTP 200, renders friendly empty state |
| 5 | Anonymous Access to /Admin/Programmes | `GET /Admin/Programmes` | **PASS** | HTTP 302 redirected to `/Account/Login` |
| 6 | Anonymous Access to /Admin/Gallery | `GET /Admin/Gallery` | **PASS** | HTTP 302 redirected to `/Account/Login` |
| 7 | Ordinary User Access to /Admin/Programmes | `GET /Admin/Programmes` | **PASS** | HTTP 302 redirected to `/Account/AccessDenied` |
| 8 | Ordinary User Access to /Admin/Gallery | `GET /Admin/Gallery` | **PASS** | HTTP 302 redirected to `/Account/AccessDenied` |
| 9 | Admin Authentication | `POST /Account/Login` | **PASS** | HTTP 200, Admin role confirmed |
| 10 | Admin Programmes Dashboard | `GET /Admin/Programmes` | **PASS** | HTTP 200, authorized admin access |
| 11 | Admin Gallery Dashboard | `GET /Admin/Gallery` | **PASS** | HTTP 200, authorized admin access |
| 12 | Date Validation: Reject EndDate < StartDate | `POST /Admin/Programmes/Create` | **PASS** | Rejected with server-side validation error |
| 13 | Upload Rejection: Invalid Extension (.exe) | `POST /Admin/Programmes/Create` | **PASS** | Rejected with file type validation error |
| 14 | Upload Rejection: Oversized File (> 5MB) | `POST /Admin/Programmes/Create` | **PASS** | Rejected with file size validation error |
| 15 | Programme Create with Valid Banner | `POST /Admin/Programmes/Create` | **PASS** | HTTP 302 redirected to `/Admin/Programmes` |
| 16 | Programme DB Persistence & Banner Storage | SQL query & disk verification | **PASS** | Record created, unique banner saved to `/uploads/programmes/` |
| 17 | Admin Programme Details | `GET /Admin/Programmes/Details/{id}` | **PASS** | HTTP 200, verified coordinator and metadata |
| 18 | Programme Edit & Image Preservation | `POST /Admin/Programmes/Edit/{id}` | **PASS** | Title updated, existing banner image preserved |
| 19 | Public Directory Displays Saved Programme | `GET /Programmes` | **PASS** | Programme rendered in Upcoming section |
| 20 | Public Details Displays Programme Info | `GET /Programmes/Details/{id}` | **PASS** | HTTP 200, venue, coordinator, and agenda rendered |
| 21 | Gallery Validation: File Required | `POST /Admin/Gallery/Upload` | **PASS** | Re-rendered with missing file error |
| 22 | Gallery Upload Rejection: Invalid Extension | `POST /Admin/Gallery/Upload` | **PASS** | Rejected with file type validation error |
| 23 | Gallery Upload with Programme Association | `POST /Admin/Gallery/Upload` | **PASS** | HTTP 302 redirected to `/Admin/Gallery` |
| 24 | Gallery DB Persistence & Disk Storage | SQL query & disk verification | **PASS** | Record created, file saved to `/uploads/gallery/`, linked to programme |
| 25 | Programme Details Embeds Linked Photo | `GET /Programmes/Details/{id}` | **PASS** | Photo rendered in Programme Gallery section |
| 26 | Public Gallery Displays Photo | `GET /Gallery` | **PASS** | Photo rendered in public gallery grid |
| 27 | Public Gallery Programme Filtering | `GET /Gallery?programmeId={id}` | **PASS** | Filter matched photo correctly |
| 28 | Gallery Photo Edit | `POST /Admin/Gallery/Edit/{id}` | **PASS** | HTTP 302, photo metadata updated |
| 29 | Gallery Deletion & Physical File Cleanup | `POST /Admin/Gallery/Delete/{id}` | **PASS** | DB record deleted and file removed from disk |
| 30 | Programme Deletion & Banner Cleanup | `POST /Admin/Programmes/Delete/{id}` | **PASS** | DB record deleted and banner removed from disk |
| 31 | Regression: Home Page | `GET /` | **PASS** | HTTP 200 OK |
| 32 | Regression: About Us | `GET /Home/About` | **PASS** | HTTP 200 OK |
| 33 | Regression: Causes Directory | `GET /Causes` | **PASS** | HTTP 200 OK |
| 34 | Regression: Donation Portal | `GET /Donate` | **PASS** | HTTP 200 OK |
| 35 | Regression: Partners & NGOs | `GET /Partners` | **PASS** | HTTP 200 OK |
| 36 | Regression: Admin NGOs | `GET /Admin/Ngos` | **PASS** | HTTP 200 OK |
| 37 | Regression: Admin Partners | `GET /Admin/Partners` | **PASS** | HTTP 200 OK |

**Test Result:** `37 / 37 PASSED (100%)`.

---

## 3. Requirements Checklist Update

The following functional requirements are fully satisfied:
- **FR-038**: Public Welfare Programme Directory (`/Programmes` with Upcoming vs. Past segregation).
- **FR-039**: Programme Detail View with related gallery imagery (`/Programmes/Details/{id}`).
- **FR-042**: Responsive Photo Gallery with Programme Filtering (`/Gallery`).
- **FR-044**: Modal Lightbox Image Inspection with accessibility captions (`#galleryImageModal`).
- **FR-045**: Programme Lifecycle Management (`/Admin/Programmes` full CRUD).
- **FR-046**: Gallery Image Asset Administration (`/Admin/Gallery` full CRUD & upload).
- **FR-047**: Banner Upload with strict file validation (magic bytes, extensions, 5MB cap).
- **FR-048**: Physical File Cleanup on image deletion (disk & database synchronization).
