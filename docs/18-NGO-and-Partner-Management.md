# Documentation: NGO and Partner Management (Task 10)

**Document ID:** `DOC-18`  
**Project Name:** Give-AID - NGO Management & Donation Website  
**Task Reference:** `TASK-10-NGO-and-Partner-Management`  
**Status:** Completed & Fully Verified  
**Date:** October 2026  

---

## 1. Feature Overview

The NGO and Partner Management module delivers full administrative control and public discovery for accredited partner NGOs and corporate sponsors:

1. **Models & Schema Alignment:**
   - `NGO.cs`: Accredited non-governmental organization entities (`Id`, `Name`, `RegistrationNumber`, `Description`, `Address`, `Email`, `Phone`, `WebsiteUrl`, `LogoPath`, `IsActive`, `CreatedAt`, and `Causes` relationship).
   - `Partner.cs`: Corporate partners and sponsors (`Id`, `Name`, `PartnerType`, `Description`, `WebsiteUrl`, `LogoPath`, `DisplayOrder`, `IsActive`, `CreatedAt`).
   - Integrated with EF Core `ApplicationDbContext` and applied to SQL Server Express database `GiveAidDb`.

2. **Image Upload & File Sanitization (`ImageUploadService`):**
   - Restricts uploads to permitted extensions: `.jpg`, `.jpeg`, `.png`, `.webp` (FR-037).
   - Magic byte header inspection validates genuine image binary structures (rejection of spoofed files like `.exe` disguised as images).
   - Server-side maximum file size cap: **2 MB**.
   - Generates unique GUID-based filenames (`Guid.NewGuid():N + extension`) storing relative URLs (`/uploads/ngos/...`, `/uploads/partners/...`).
   - Directory traversal guard prevents unauthorized file removal (`..` path sanitation).
   - Edit operations preserve existing logos when no new file is uploaded.
   - Deleting or replacing a record safely cleans up the old file from disk.

3. **Admin-Only CRUD Operations (`[Authorize(Roles = "Admin")]`):**
   - **NGOs:**
     - `GET /Admin/Ngos`: List view with logos, statuses, metadata, and empty states.
     - `GET /Admin/Ngos/Details/{id}`: Detailed profile review.
     - `GET /Admin/Ngos/Create` & `POST /Admin/Ngos/Create`: Multipart form with antiforgery protection and validation.
     - `GET /Admin/Ngos/Edit/{id}` & `POST /Admin/Ngos/Edit/{id}`: Edit form with logo preservation.
     - `GET /Admin/Ngos/Delete/{id}` & `POST /Admin/Ngos/Delete/{id}`: Explicit confirmation with associated cause deletion safeguard.
   - **Partners:**
     - `GET /Admin/Partners`: List view ordered by `DisplayOrder` with category badges.
     - `GET /Admin/Partners/Details/{id}`: Metadata and partnership synopsis.
     - `GET /Admin/Partners/Create` & `POST /Admin/Partners/Create`: Creation form with partner type selection.
     - `GET /Admin/Partners/Edit/{id}` & `POST /Admin/Partners/Edit/{id}`: Modification form with logo preservation.
     - `GET /Admin/Partners/Delete/{id}` & `POST /Admin/Partners/Delete/{id}`: Confirmation modal and file removal.

4. **Public Discovery Pages (`PartnersController`):**
   - `GET /Partners`: Responsive unified showcase of affiliated NGOs and corporate partners with fallbacks for missing logos and empty states.
   - `GET /Partners/NgoDetails/{id}`: Individual NGO profile with contact information, mission statement, and active campaigns.
   - `GET /Partners/PartnerDetails/{id}`: Corporate sponsor profile with tier badge and outbound website reference.
   - Graceful 404 NotFound handling for invalid or nonexistent IDs.

5. **Security & Role-Based Access Control:**
   - Anonymous visitors attempting to access `/Admin/Ngos` or `/Admin/Partners` are redirected to `/Account/Login`.
   - Regular authenticated members attempting to access admin routes are redirected to `/Account/AccessDenied`.

---

## 2. Test Execution & Verification

All 31 integration test assertions executed and passed against the running ASP.NET Core web server connected to SQL Server Express `GiveAidDb`:

| # | Test Name | Target Route / Operation | Result | Details |
|---|---|---|---|---|
| 1 | Public Partners List | `GET /Partners` | **PASS** | HTTP 200, dynamic listing rendered |
| 2 | Public NGO Details | `GET /Partners/NgoDetails/1` | **PASS** | HTTP 200, contact card rendered |
| 3 | Public NGO Nonexistent ID | `GET /Partners/NgoDetails/99999` | **PASS** | HTTP 404 NotFound handled gracefully |
| 4 | Public Partner Nonexistent ID | `GET /Partners/PartnerDetails/99999` | **PASS** | HTTP 404 NotFound handled gracefully |
| 5 | Anonymous Access to NGOs | `GET /Admin/Ngos` | **PASS** | HTTP 302 -> Redirected to `/Account/Login` |
| 6 | Anonymous Access to Partners | `GET /Admin/Partners` | **PASS** | HTTP 302 -> Redirected to `/Account/Login` |
| 7 | Ordinary User Access to NGOs | `GET /Admin/Ngos` | **PASS** | HTTP 302 -> Redirected to `/Account/AccessDenied` |
| 8 | Ordinary User Access to Partners | `GET /Admin/Partners` | **PASS** | HTTP 302 -> Redirected to `/Account/AccessDenied` |
| 9 | Admin Dashboard Access | `GET /Admin` | **PASS** | HTTP 200, Admin Authorized Area verified |
| 10 | Admin Access to NGOs List | `GET /Admin/Ngos` | **PASS** | HTTP 200 OK |
| 11 | Admin Access to Partners List | `GET /Admin/Partners` | **PASS** | HTTP 200 OK |
| 12 | Invalid Extension Rejection | `POST /Admin/Ngos/Create` with `.exe` | **PASS** | Form re-rendered with validation error |
| 13 | Oversized Upload Rejection | `POST /Admin/Ngos/Create` > 2 MB | **PASS** | Form re-rendered with 2 MB limit error |
| 14 | Valid NGO Creation | `POST /Admin/Ngos/Create` with PNG | **PASS** | HTTP 302 Redirect, record persisted |
| 15 | NGO Database & Disk Persistence | SQL Query & File Check | **PASS** | DB record verified, PNG saved to disk |
| 16 | Admin NGO Details View | `GET /Admin/Ngos/Details/{id}` | **PASS** | HTTP 200 OK |
| 17 | Logo Preservation on Edit | `POST /Admin/Ngos/Edit/{id}` without file | **PASS** | Updated name saved, existing logo preserved |
| 18 | Dynamic Public Display | `GET /Partners` | **PASS** | Updated NGO rendered in public list |
| 19 | Public NGO Details Reflection | `GET /Partners/NgoDetails/{id}` | **PASS** | HTTP 200 OK, updated bio displayed |
| 20 | NGO Deletion & Disk Cleanup | `POST /Admin/Ngos/Delete/{id}` | **PASS** | Record removed from DB, file deleted |
| 21 | Valid Partner Creation | `POST /Admin/Partners/Create` with JPEG | **PASS** | HTTP 302 Redirect, record persisted |
| 22 | Partner Database & Disk Persistence | SQL Query & File Check | **PASS** | DB record verified, JPEG saved to disk |
| 23 | Admin Partner Details View | `GET /Admin/Partners/Details/{id}` | **PASS** | HTTP 200 OK |
| 24 | Partner Logo Preservation | `POST /Admin/Partners/Edit/{id}` without file | **PASS** | Name updated, existing logo preserved |
| 25 | Public Partner Display | `GET /Partners` | **PASS** | Partner rendered in public list |
| 26 | Public Partner Details Reflection | `GET /Partners/PartnerDetails/{id}` | **PASS** | HTTP 200 OK, details rendered |
| 27 | Partner Deletion & Disk Cleanup | `POST /Admin/Partners/Delete/{id}` | **PASS** | Record removed from DB, file deleted |
| 28 | Regression: Home Page | `GET /` | **PASS** | HTTP 200 OK |
| 29 | Regression: About Us | `GET /Home/About` | **PASS** | HTTP 200 OK |
| 30 | Regression: Causes Catalog | `GET /Causes` | **PASS** | HTTP 200 OK |
| 31 | Regression: Donation Form | `GET /Donate` | **PASS** | HTTP 200 OK |
