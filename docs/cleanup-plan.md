# Give-AID NGO — Cleanup Plan

**Date:** 2026-10-16  
**Project Directory:** `d:\Give-AID NGO`  
**Current Total Size:** 247.72 MB  
**Target Size:** ≤ 2 GB (already met; this cleanup reduces to ~19 MB)

---

## Category A — Safe to Remove (Confirmed Regenerable / Temporary)

| # | Path | Size (MB) | Reason for Removal | Regeneration Method | Risk Level |
|:---:|:---|---:|:---|:---|:---:|
| A1 | `GiveAid.Web/bin/` | 217.08 | Build output; contains Debug + Release compiled DLLs, native runtimes, and platform-specific binaries. | `dotnet restore && dotnet build` | **None** |
| A2 | `GiveAid.Web/obj/` | 11.69 | Intermediate build cache; compressed static assets, assembly references, restore markers. | `dotnet restore && dotnet build` | **None** |
| A3 | `scratch/` | 0.12 | Temporary development test scripts (`test-task13.ps1`, `test-task14.ps1`, `list-programmes.ps1`) and server debug logs (`server_task13.log`, `server_task14.log`, etc.) from previous task iterations. Not referenced by the application. Already covered in `.gitignore`. | N/A (disposable) | **None** |

**Subtotal Category A: 228.89 MB (potential savings)**

---

## Category B — Requires Verification (Reviewed and Resolved)

No items remain in Category B. All files were inspected and classified into A or C.

---

## Category C — Must Keep (Required for Build, Run, Submission, or Documentation)

| # | Path | Size (MB) | Reason |
|:---:|:---|---:|:---|
| C1 | `GiveAid.Web/Controllers/` | 0.14 | Application source code |
| C2 | `GiveAid.Web/Models/` | 0.02 | Entity model classes |
| C3 | `GiveAid.Web/Views/` | 0.38 | Razor view templates |
| C4 | `GiveAid.Web/ViewModels/` | 0.03 | View model classes |
| C5 | `GiveAid.Web/Services/` | 0.03 | Service layer (ImageUpload, RoleSeed, DataSeed, Email) |
| C6 | `GiveAid.Web/Data/` | 0.01 | ApplicationDbContext |
| C7 | `GiveAid.Web/Migrations/` | 0.24 | EF Core migration files — required for database recreation |
| C8 | `GiveAid.Web/wwwroot/` | 9.36 | Static assets: Bootstrap, jQuery, CSS, JS, favicon, uploads dir |
| C9 | `GiveAid.Web/Properties/` | < 0.01 | launchSettings.json |
| C10 | `GiveAid.Web/Program.cs` | < 0.01 | Application entry point |
| C11 | `GiveAid.Web/appsettings*.json` | < 0.01 | Configuration files |
| C12 | `GiveAid.Web/GiveAid.Web.csproj` | < 0.01 | Project file — NuGet package references |
| C13 | `database/GiveAidDb.bak` | 5.34 | SQL Server backup — required for submission |
| C14 | `database/GiveAidDb_Schema.sql` | 0.02 | Idempotent DDL script — required for submission |
| C15 | `.git/` | 2.80 | Git repository — history, commits, branches |
| C16 | `docs/` (all files) | 0.35 | All academic documentation — required by roadmap |
| C17 | `tasks/` | 0.05 | Project roadmap task files — required reference |
| C18 | `GiveAid.sln` + `GiveAid.slnx` | < 0.01 | Solution files |
| C19 | `README.md` | < 0.01 | Project README — required |
| C20 | `.gitignore` | < 0.01 | Git exclusion rules |
| C21 | `test_task18_regression.ps1` | 0.03 | Automated regression test suite |
| C22 | `test_all_tasks.ps1` | 0.01 | Task verification script |

---

## Execution Plan

1. Delete `GiveAid.Web/bin/` (217.08 MB)
2. Delete `GiveAid.Web/obj/` (11.69 MB)
3. Delete `scratch/` (0.12 MB)
4. Verify `.gitignore` has appropriate rules for all regenerable content
5. Run `dotnet restore && dotnet build` to verify project still compiles
6. Run `test_task18_regression.ps1` to verify no functionality broken
7. Measure final project size

**Expected final size: ~18.83 MB (from 247.72 MB — 92.4% reduction)**
