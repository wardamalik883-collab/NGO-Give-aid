# 22 — Programme Interest and Participation

**Document ID:** `DOC-22`  
**Task:** `TASK-14` — Programme Interest / Participation  
**Status:** ✅ Complete  
**Completed:** 15 October 2026  
**Author:** Give-AID Development Team  

---

## 1. Overview

Task 14 implements a full volunteer and community participation workflow that allows both authenticated members and anonymous visitors to express interest in Give-AID welfare programmes. Administrators can review, approve, or reject applications through a dedicated back-office moderation panel.

---

## 2. Requirements Satisfied

| ID | Requirement | Status |
|----|-------------|--------|
| FR-040 | Expression of interest / volunteer submission | ✅ Done |
| FR-041 | Prevention of duplicate programme interest submissions | ✅ Done |
| FR-043 | Admin moderation of volunteer/participation requests | ✅ Done |

---

## 3. Database Model

### `ProgrammeInterest`

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` PK | Auto-increment |
| `ProgrammeId` | `int` FK | → `Programmes.Id` CASCADE DELETE |
| `UserId` | `nvarchar(450)` FK nullable | → `AspNetUsers.Id` SET NULL on delete |
| `ApplicantName` | `nvarchar(100)` | Required |
| `ApplicantEmail` | `nvarchar(100)` | Required, indexed with ProgrammeId |
| `ApplicantPhone` | `nvarchar(30)` | Required |
| `InterestType` | `nvarchar(50)` | `"Volunteer"` or `"Participant"` |
| `Message` | `nvarchar(1000)` | Optional motivation statement |
| `VolunteerNote` | `nvarchar(1000)` | Legacy alias for Message |
| `Status` | `nvarchar(25)` | Default `"Pending"` → `"Approved"` / `"Rejected"` |
| `SubmissionDate` | `datetime2` | UTC server timestamp on submission |
| `AdminNotes` | `nvarchar(max)` | Append-log of admin review notes |
| `ReviewedAt` | `datetime2?` | UTC timestamp set on Approve or Reject |

**Indexes:** `IX_ProgrammeInterests_UserId`, `IX_ProgrammeInterests_ProgrammeId_ApplicantEmail` (unique constraint for duplicate prevention)

---

## 4. Migration

Migration: `20261015085841_AddProgrammeInterestFieldsAndUserRel`

Applied to `GiveAidDb` on `.\SQLEXPRESS`.

---

## 5. ViewModels

| Class | Purpose |
|-------|---------|
| `ProgrammeInterestFormViewModel` | Public participation form (guest + authenticated) |
| `UserProgrammeInterestViewModel` | Authenticated user's personal application history |
| `AdminInterestListViewModel` | Admin moderation list with KPIs and filters |

---

## 6. Controllers & Actions

### `ProgrammesController` (public)

| Route | Method | Auth | Description |
|-------|--------|------|-------------|
| `GET /Programmes/ExpressInterest?programmeId={id}` | GET | Anonymous | Render participation form; pre-fills email for signed-in users |
| `POST /Programmes/ExpressInterest` | POST + AntiForgery | Anonymous | Submit application; duplicate check; email confirmation |
| `GET /Programmes/MyInterests` | GET | `[Authorize]` | Personal application history + KPI cards |

### `AdminInterestsController` (`[Authorize(Roles = "Admin")]`)

| Route | Method | Description |
|-------|--------|-------------|
| `GET /Admin/Interests` | GET | List all applications with status/programme filters + KPI cards |
| `GET /Admin/Interests/Details/{id}` | GET | Applicant details + moderation forms |
| `POST /Admin/Interests/Approve/{id}` | POST | Approve and notify applicant via email |
| `POST /Admin/Interests/Reject/{id}` | POST | Reject and notify applicant via email |

---

## 7. Views

| View File | Description |
|-----------|-------------|
| `Views/Programmes/ExpressInterest.cshtml` | Public volunteer/participant application form |
| `Views/Programmes/MyInterests.cshtml` | Authenticated user's application history with KPI cards |
| `Views/Programmes/Details.cshtml` | Updated — added TempData alert rendering + CTA button |
| `Views/Admin/Interests/Index.cshtml` | Admin list with KPI counters, status/programme filters, search |
| `Views/Admin/Interests/Details.cshtml` | Admin review: applicant info, programme reference, approve/reject forms |

---

## 8. Navigation Integration

- **Navbar user dropdown:** "My Applications" → `/Programmes/MyInterests` (all authenticated users)
- **Navbar admin dropdown:** "Volunteer Requests" → `/Admin/Interests` (Admin role only)
- **Admin Dashboard (`/Admin/Index`):** New "Volunteers — Task 14" management card
- **Programme Details page:** "Volunteer / Express Interest" CTA button links to the form

---

## 9. Business Rules

1. **Status lifecycle:** All submissions start as `"Pending"`. Only admins can transition to `"Approved"` or `"Rejected"`.
2. **Duplicate prevention (application layer):**
   - Authenticated users: blocked by `(UserId, ProgrammeId)` uniqueness check.
   - Guests: blocked by `(ApplicantEmail, ProgrammeId)` uniqueness check.
   - Database-level: unique index `IX_ProgrammeInterests_ProgrammeId_ApplicantEmail` as secondary guard.
3. **Email notifications:**
   - On submission: confirmation email to applicant via `IEmailService` (simulated).
   - On approval: acceptance email with programme date and location.
   - On rejection: polite capacity-limit notification.
4. **UserId binding:** Pulled server-side from `User.FindFirstValue(ClaimTypes.NameIdentifier)` — never accepted from the form body.
5. **Anti-forgery tokens:** All POST endpoints protected by `[ValidateAntiForgeryToken]`.
6. **Role isolation:** `/Admin/Interests` fully protected by `[Authorize(Roles = "Admin")]`; standard users receive 302→AccessDenied or 403.

---

## 10. Test Coverage

**Test file:** `scratch/test-task14.ps1`  
**Result:** **20/20 PASS** (exit code 0)

| Group | Tests | Result |
|-------|-------|--------|
| Group 1: Public discovery & CTA | 3 | ✅ All Pass |
| Group 2: Anonymous participation workflow | 4 | ✅ All Pass |
| Group 3: Authenticated user history | 5 | ✅ All Pass |
| Group 4: Role-based access control | 2 | ✅ All Pass |
| Group 5: Admin moderation workflow | 6 | ✅ All Pass |
