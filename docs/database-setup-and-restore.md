# Give-AID NGO — Database Setup, Migration & Restoration Guide

**Document Reference:** `docs/database-setup-and-restore.md`  
**Application Title:** Give-AID — Non-Governmental Organization Web Portal  
**Database Name:** `GiveAidDb`  
**Target Engine:** Microsoft SQL Server 2019/2022 or Microsoft SQL Server Express (`.\SQLEXPRESS`)  
**Revision Date:** 2026-10-16  

---

## 1. Overview of Database Setup Options

Give-AID provides three reliable, verified options for initializing and configuring the database on a fresh evaluation machine:

| Option | Method | Recommended For | Required Artifacts |
| :---: | :--- | :--- | :--- |
| **Option A** *(Recommended)* | **EF Core Code-First Migrations** | Fresh development machines, cross-platform environments, LocalDB | `GiveAid.Web/Migrations/` |
| **Option B** | **SQL Server Native Backup (.bak)** | Quick local evaluation with full pre-populated demo records | `database/GiveAidDb.bak` |
| **Option C** | **Idempotent DDL SQL Script** | Restricted SQL Server environments, Azure SQL, or DBA review | `database/GiveAidDb_Schema.sql` |

---

## 2. Option A: Migration-Based Setup (Recommended)

This is the standard, cross-platform method for ASP.NET Core applications. It requires no physical file paths and works with any supported SQL Server instance.

### Step A.1: Configure Connection String
Open [`GiveAid.Web/appsettings.json`](file:///d:/Give-AID%20NGO/GiveAid.Web/appsettings.json) and verify or update the connection string to point to your local SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GiveAidDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

*For LocalDB instances:*
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GiveAidDb;Integrated Security=True;MultipleActiveResultSets=true"
```

### Step A.2: Apply Migrations
From the project root directory, execute:
```powershell
dotnet ef database update --project GiveAid.Web
```

*Alternatively, from the Visual Studio Package Manager Console:*
```powershell
Update-Database -Project GiveAid.Web
```

### Step A.3: Automatic Seeding on Startup
When the application starts (`dotnet run --project GiveAid.Web`):
1. [`RoleSeedService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/RoleSeedService.cs) automatically ensures the `Admin` and `User` Identity roles exist.
2. If `AdminSeed:Email` and `AdminSeed:Password` are set in User Secrets, it creates the administrator account.
3. [`DataSeedService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/DataSeedService.cs) detects empty tables and automatically seeds baseline accredited NGOs and initial urgent causes.

---

## 3. Option B: Restoring from SQL Server Backup (.bak)

A full, verified SQL Server backup is included in the package at [`database/GiveAidDb.bak`](file:///d:/Give-AID%20NGO/database/GiveAidDb.bak). It contains all 11 tables, pre-seeded causes, accredited NGOs, corporate partners, scheduled welfare camps, sample gallery photos, and test accounts.

### Step B.1: Restoring via PowerShell & SQLCMD
Run the following PowerShell command (adjust destination file paths if your SQL Server data directory differs):

```powershell
# Restore GiveAidDb from the .bak file
sqlcmd -S ".\SQLEXPRESS" -d master -C -Q @"
RESTORE DATABASE [GiveAidDb] 
FROM DISK = N'd:\Give-AID NGO\database\GiveAidDb.bak' 
WITH REPLACE, RECOVERY;
"@
```

### Step B.2: Restoring via SQL Server Management Studio (SSMS)
1. Open **SQL Server Management Studio** and connect to your SQL Server instance (e.g., `.\SQLEXPRESS`).
2. In Object Explorer, right-click **Databases** and choose **Restore Database...**.
3. Under **Source**, select **Device**, click the **...** button, and click **Add**.
4. Browse to and select `d:\Give-AID NGO\database\GiveAidDb.bak`.
5. Under **Destination**, confirm Database is set to `GiveAidDb`.
6. Click **OK** to restore.

---

## 4. Option C: Executing the Idempotent DDL Script

For environments where binary restore permissions are restricted:

1. Open PowerShell in the project root:
   ```powershell
   cd "d:\Give-AID NGO"
   ```
2. Execute the included idempotent DDL script:
   ```powershell
   sqlcmd -S ".\SQLEXPRESS" -d master -C -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'GiveAidDb') CREATE DATABASE [GiveAidDb];"
   sqlcmd -S ".\SQLEXPRESS" -d GiveAidDb -C -i "database\GiveAidDb_Schema.sql"
   ```
3. Start the application to allow [`DataSeedService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/DataSeedService.cs) to populate the baseline data.

---

## 5. Verifying Database Integrity

After applying any setup method, verify the database schema and table availability:

```powershell
sqlcmd -S ".\SQLEXPRESS" -d GiveAidDb -C -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;"
```

### Expected 11 Domain Tables + Identity Tables:
- `AboutSections`
- `AspNetRoleClaims`
- `AspNetRoles`
- `AspNetUserClaims`
- `AspNetUserLogins`
- `AspNetUserRoles`
- `AspNetUsers`
- `AspNetUserTokens`
- `Causes`
- `ContactQueries`
- `Donations`
- `GalleryImages`
- `Invitations`
- `NGOs`
- `Partners`
- `ProgrammeInterests`
- `Programmes`

---

## 6. How to Take a New Database Backup

To create a new backup after conducting manual tests:
```powershell
sqlcmd -S ".\SQLEXPRESS" -d master -C -Q "BACKUP DATABASE [GiveAidDb] TO DISK = N'd:\Give-AID NGO\database\GiveAidDb.bak' WITH FORMAT, INIT;"
```

> [!NOTE]
> SQL Server Express Edition does not support native backup compression (`WITH COMPRESSION`). Backups must be performed without the `COMPRESSION` flag.
