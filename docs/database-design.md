# Give-AID NGO — Database Design & Schema Documentation

**Document ID:** `DB-01`  
**Database Engine:** Microsoft SQL Server Express (`Server=.\SQLEXPRESS;Database=GiveAidDb`)  
**ORM:** Entity Framework Core 10.0 (Code-First)  
**Date:** October 2026  

---

## 1. Architectural Overview

The Give-AID relational database is structured to balance strict financial auditability, relational referential integrity, and flexible community engagement. The schema consists of 11 core tables managed by Entity Framework Core, integrated alongside standard ASP.NET Core Identity authentication tables.

### Key Database Design Principles:
1. **Audit Preservation via Delete Restrict:** `Causes` cannot be deleted if referenced by an `NGO`. Similarly, `Causes` referenced by `Donations` enforce `DeleteBehavior.Restrict` so that financial audit histories are never orphaned or deleted.
2. **User Deletion Anonymization:** Deleting a user account (`ApplicationUser`) automatically applies `DeleteBehavior.SetNull` to associated `Donations`, `ProgrammeInterests`, and `ContactQueries`, preserving historical data without violating foreign key constraints.
3. **Decimal Precision Enforcement:** Monetary columns (`TargetAmount`, `CurrentAmount`, `Amount`) explicitly declare `decimal(18,2)` precision to prevent rounding errors.
4. **Idempotency & Uniqueness:** Unique indexes prevent duplicate donations, duplicate section keys, and duplicate volunteer submissions for the same email and programme.

---

## 2. Table-by-Table Schema Specification

### 2.1 `AspNetUsers` (`ApplicationUser`)
Extends ASP.NET Core Identity's `IdentityUser` with organizational domain properties.

| Column | Data Type | Nullable | Default / Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `nvarchar(450)` | No | Primary Key | Unique user GUID identifier. |
| `UserName` | `nvarchar(256)` | Yes | Unique Index | Normalized username (mirrors email). |
| `NormalizedUserName` | `nvarchar(256)` | Yes | Index | Uppercase username for fast lookup. |
| `Email` | `nvarchar(256)` | Yes | Index | User email address. |
| `NormalizedEmail` | `nvarchar(256)` | Yes | Index | Uppercase email for case-insensitive lookup. |
| `EmailConfirmed` | `bit` | No | `0` | Email confirmation flag. |
| `PasswordHash` | `nvarchar(max)` | Yes | | PBKDF2/HMAC-SHA256 salted password hash. |
| `SecurityStamp` | `nvarchar(max)` | Yes | | Token invalidation stamp on password change. |
| `ConcurrencyStamp` | `nvarchar(max)` | Yes | | Concurrency check token. |
| `PhoneNumber` | `nvarchar(max)` | Yes | | Contact telephone number. |
| `PhoneNumberConfirmed` | `bit` | No | `0` | Phone confirmation flag. |
| `TwoFactorEnabled` | `bit` | No | `0` | Two-factor authentication status. |
| `LockoutEnd` | `datetimeoffset` | Yes | | Account lockout expiration timestamp. |
| `LockoutEnabled` | `bit` | No | `1` | Lockout enabled on failed attempts. |
| `AccessFailedCount` | `int` | No | `0` | Counter for consecutive invalid login attempts. |
| **`FullName`** | `nvarchar(100)` | No | Custom field | User's full display name. |
| **`CreatedAt`** | `datetime2` | No | `GETUTCDATE()` | Registration UTC timestamp. |

---

### 2.2 `NGOs`
Stores accredited non-governmental partner organizations affiliated with the Give-AID platform.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique NGO identifier. |
| `Name` | `nvarchar(150)` | No | Required | Full registered legal name of the NGO. |
| `RegistrationNumber` | `nvarchar(50)` | Yes | | Official government/charity commission registration ID. |
| `Description` | `nvarchar(1000)` | Yes | | Organizational mission, focus, and history. |
| `Address` | `nvarchar(250)` | Yes | | Headquarters physical mailing address. |
| `Email` | `nvarchar(100)` | Yes | Email format | Official coordination contact email. |
| `Phone` | `nvarchar(30)` | Yes | Phone format | Official telephone contact. |
| `WebsiteUrl` | `nvarchar(250)` | Yes | URL format | External organizational website. |
| `LogoPath` | `nvarchar(250)` | Yes | | Relative URL to uploaded logo image (`/uploads/ngos/...`). |
| `IsActive` | `bit` | No | Default `1` | Operational visibility flag. |
| `CreatedAt` | `datetime2` | No | Default `GETUTCDATE()` | Record creation UTC timestamp. |

- **Relationships:** One-to-Many with `Causes` (`Ngo.Causes`).
- **Referential Action:** `OnDelete(DeleteBehavior.Restrict)` on child causes.

---

### 2.3 `Causes`
Represents verified charitable fundraising campaigns and disaster relief appeals.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique cause campaign identifier. |
| `Title` | `nvarchar(150)` | No | Required | Campaign public headline. |
| `ShortDescription` | `nvarchar(300)` | No | Required | Concise teaser summary for cards and search. |
| `FullDescription` | `nvarchar(max)` | No | Required | Detailed narrative, beneficiaries, and milestones. |
| `TargetAmount` | `decimal(18,2)` | No | `Range(1, 100M)` | Target fundraising monetary goal in PKR. |
| `CurrentAmount` | `decimal(18,2)` | No | Default `0.00` | Running total of successfully simulated contributions. |
| `ImagePath` | `nvarchar(250)` | Yes | | Relative URL to primary banner image (`/uploads/causes/...`). |
| `IsFeatured` | `bit` | No | Default `0` | Flag to pin campaign to Home page featured section. |
| `IsActive` | `bit` | No | Default `1` | Campaign status (active allows donations; inactive hides). |
| `StartDate` | `datetime2` | Yes | | Campaign formal commencement date. |
| `EndDate` | `datetime2` | Yes | | Campaign planned conclusion date. |
| `CreatedAt` | `datetime2` | No | Default `GETUTCDATE()` | Record creation UTC timestamp. |
| `NgoId` | `int` | No | FK -> `NGOs.Id` | Organizing accredited NGO ID. |

- **Relationships:** Many-to-One with `NGOs`; One-to-Many with `Donations`.
- **Referential Action:** `OnDelete(DeleteBehavior.Restrict)`.

---

### 2.4 `Donations`
Stores simulated financial contributions. Real payment cards are never stored.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique donation record identifier. |
| `TransactionReference` | `nvarchar(60)` | No | Unique Index | Human-readable tracking ID (`TXN-YYYYMMDD-XXXX`). |
| `Amount` | `decimal(18,2)` | No | `Range(1, 10M)` | Contributed amount in PKR. |
| `DonationDate` | `datetime2` | No | Default `GETUTCDATE()` | Transaction processing UTC timestamp. |
| `DonorName` | `nvarchar(100)` | No | Required | Full name of donor (provided on form). |
| `DonorEmail` | `nvarchar(100)` | No | Required | Receipt email address. |
| `DonorPhone` | `nvarchar(30)` | Yes | | Optional donor contact telephone. |
| `PaymentStatus` | `nvarchar(25)` | No | Default `Completed` | Status (`Completed`, `Failed`, `Pending`). |
| `IsSimulated` | `bit` | No | Default `1` | Architectural flag confirming demo simulation. |
| `PaymentNotes` | `nvarchar(500)` | Yes | | Diagnostic summary (e.g., demo test reason). |
| `CauseId` | `int` | No | FK -> `Causes.Id` | Supported charitable cause. |
| `UserId` | `nvarchar(450)` | Yes | FK -> `AspNetUsers.Id` | Optional link to registered donor account. |
| `UpdatedAt` | `datetime2` | Yes | | Timestamp of subsequent status modification. |

- **Indexes:** Unique index on `TransactionReference`; non-clustered index on `UserId`.
- **Referential Action:** `OnDelete(DeleteBehavior.Restrict)` on Cause; `OnDelete(DeleteBehavior.SetNull)` on User.

---

### 2.5 `Programmes`
Represents scheduled community welfare events, medical camps, and relief distributions.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique programme event identifier. |
| `Title` | `nvarchar(150)` | No | Required | Programme title / event headline. |
| `Description` | `nvarchar(max)` | No | Required | Comprehensive mission statement and objectives. |
| `EventDate` | `datetime2` | No | Required | Event start date and time (UTC). |
| `EndDate` | `datetime2` | Yes | | Event conclusion date and time (UTC). |
| `Location` | `nvarchar(200)` | No | Required | Venue name, district, or municipal facility. |
| `CoordinatorName` | `nvarchar(100)` | Yes | | Designated field manager or lead volunteer coordinator. |
| `CoordinatorContact` | `nvarchar(50)` | Yes | | Coordinator hotline or mobile contact. |
| `BannerImagePath` | `nvarchar(250)` | Yes | | Header promotional image (`/uploads/programmes/...`). |
| `Status` | `nvarchar(25)` | No | Default `Scheduled` | Event status (`Scheduled`, `Completed`, `Cancelled`). |
| `CreatedAt` | `datetime2` | No | Default `GETUTCDATE()` | Creation timestamp. |
| `UpdatedAt` | `datetime2` | Yes | | Modification timestamp. |

- **Relationships:** One-to-Many with `ProgrammeInterests`; One-to-Many with `GalleryImages`.

---

### 2.6 `ProgrammeInterests`
Records volunteer and community participant applications for scheduled programmes.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique application ID. |
| `ProgrammeId` | `int` | No | FK -> `Programmes.Id` | Target welfare programme. |
| `UserId` | `nvarchar(450)` | Yes | FK -> `AspNetUsers.Id` | Registered user ID (null for guests). |
| `ApplicantName` | `nvarchar(100)` | No | Required | Full name of volunteer / participant. |
| `ApplicantEmail` | `nvarchar(100)` | No | Required | Contact email address. |
| `ApplicantPhone` | `nvarchar(30)` | No | Required | Contact telephone number. |
| `InterestType` | `nvarchar(50)` | No | `Volunteer`/`Participant`| Role applied for. |
| `Message` | `nvarchar(1000)` | Yes | | Skills, motivation, or logistical needs. |
| `VolunteerNote` | `nvarchar(500)` | Yes | | Field specialization notes. |
| `Status` | `nvarchar(25)` | No | Default `Pending` | Status (`Pending`, `Approved`, `Rejected`). |
| `SubmissionDate` | `datetime2` | No | Default `GETUTCDATE()` | Application submission timestamp. |
| `AdminNotes` | `nvarchar(500)` | Yes | | Internal reviewer notes. |
| `ReviewedAt` | `datetime2` | Yes | | Timestamp of administrative review. |

- **Indexes:** Composite unique index on `(ProgrammeId, ApplicantEmail)` preventing duplicate applications; index on `UserId`.
- **Referential Action:** `OnDelete(DeleteBehavior.Cascade)` on Programme; `OnDelete(DeleteBehavior.SetNull)` on User.

---

### 2.7 `GalleryImages`
Stores verified field photo assets and impact documentation.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique gallery image ID. |
| `Title` | `nvarchar(100)` | No | Required | Image caption title. |
| `Caption` | `nvarchar(250)` | Yes | | Contextual narrative describing field activity. |
| `AltText` | `nvarchar(150)` | No | WCAG AA Required | Accessible screen-reader description. |
| `FilePath` | `nvarchar(250)` | No | Required | Server path (`/uploads/gallery/...`). |
| `UploadedAt` | `datetime2` | No | Default `GETUTCDATE()` | Upload timestamp. |
| `ProgrammeId` | `int` | Yes | FK -> `Programmes.Id` | Optional link to documented programme. |

- **Referential Action:** `OnDelete(DeleteBehavior.SetNull)` on Programme.

---

### 2.8 `ContactQueries`
Captures public and member inquiries, resolution tracking, and administrative replies.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique inquiry ticket ID. |
| `SenderName` | `nvarchar(100)` | No | Required | Inquirer's full name. |
| `SenderEmail` | `nvarchar(100)` | No | Required | Inquirer's email address. |
| `SenderPhone` | `nvarchar(30)` | Yes | | Contact phone number. |
| `Subject` | `nvarchar(150)` | No | Required | Subject line. |
| `Message` | `nvarchar(2000)` | No | Required | Inquiry content. |
| `SubmittedAt` | `datetime2` | No | Default `GETUTCDATE()` | Submission timestamp. |
| `Status` | `nvarchar(25)` | No | Default `New` | Resolution status (`New`, `In Progress`, `Answered`, `Closed`). |
| `AdminNotes` | `nvarchar(1000)` | Yes | | Internal staff coordination notes. |
| `AdminReply` | `nvarchar(2000)` | Yes | | Official response dispatched to inquirer. |
| `RepliedAt` | `datetime2` | Yes | | Timestamp of official reply. |
| `UserId` | `nvarchar(450)` | Yes | FK -> `AspNetUsers.Id` | Optional user link if submitted while logged in. |

- **Referential Action:** `OnDelete(DeleteBehavior.SetNull)` on User.

---

### 2.9 `Partners`
Manages corporate sponsors, academic partners, and humanitarian patrons.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique partner record ID. |
| `Name` | `nvarchar(150)` | No | Required | Partner or corporation name. |
| `PartnerType` | `nvarchar(50)` | No | Default `Corporate Sponsor`| Categorization (e.g., `Technology Sponsor`). |
| `Description` | `nvarchar(500)` | Yes | | Synopsis of partnership scope. |
| `WebsiteUrl` | `nvarchar(250)` | Yes | URL format | Corporate website link. |
| `LogoPath` | `nvarchar(250)` | Yes | | Relative URL to uploaded logo (`/uploads/partners/...`). |
| `DisplayOrder` | `int` | No | Default `0` | Numerical sorting weight for UI presentation. |
| `IsActive` | `bit` | No | Default `1` | Display visibility flag. |
| `CreatedAt` | `datetime2` | No | Default `GETUTCDATE()` | Creation timestamp. |

---

### 2.10 `Invitations`
Logs friend referral invitations dispatched by registered members.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique invitation record ID. |
| `SenderUserId` | `nvarchar(450)` | No | Required | User ID of registered member sending the invite. |
| `RecipientEmail` | `nvarchar(100)` | No | Email format | Recipient friend's email address. |
| `InviteMessage` | `nvarchar(500)` | Yes | | Optional personalized message. |
| `SentAt` | `datetime2` | No | Default `GETUTCDATE()` | Referral dispatch timestamp. |
| `DeliveryStatus` | `nvarchar(25)` | No | Default `Logged` | Status (`Logged`, `Sent`, `Failed`). |

---

### 2.11 `AboutSections`
Stores manageable content sections for the 7 About Us page areas.

| Column | Data Type | Nullable | Constraints | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `int` | No | PK, `IDENTITY(1,1)` | Unique section ID. |
| `SectionKey` | `nvarchar(50)` | No | Unique Index | Functional identifier (`WhatWeDo`, `Mission`, `Team`, etc.). |
| `Title` | `nvarchar(150)` | No | Required | Section heading displayed on page. |
| `Content` | `nvarchar(max)` | No | Required | Narrative content. |
| `ImagePath` | `nvarchar(250)` | Yes | | Optional illustrative asset path. |
| `DisplayOrder` | `int` | No | Default `0` | Order index. |
| `IsActive` | `bit` | No | Default `1` | Active flag. |
| `LastModifiedAt` | `datetime2` | No | Default `GETUTCDATE()` | Last modification timestamp. |

---

## 3. Database Integrity & Constraints Verification

During Task 18 end-to-end regression testing, the schema was validated against live SQL Server Express:
- All primary keys and identity sequences incremented properly.
- All foreign keys correctly prevented orphaned records.
- Deleting an NGO with active causes triggered a referential integrity exception, confirming `DeleteBehavior.Restrict`.
- Deleting a programme with volunteer applications correctly cascaded, while associated gallery images were preserved with `ProgrammeId = NULL`.
- Unique indexes on `TransactionReference` and `(ProgrammeId, ApplicantEmail)` prevented duplicate records.
