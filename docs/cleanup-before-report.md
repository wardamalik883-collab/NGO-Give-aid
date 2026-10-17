# Give-AID NGO — Pre-Cleanup Size Report

**Date:** 2026-10-16  
**Project Directory:** `d:\Give-AID NGO`  

---

## 1. Overall Metrics

| Metric | Value |
|:---|:---|
| **Total Project Size** | **247.72 MB (0.242 GB)** |
| **Total Files** | 1,322 |
| **Total Directories** | 408 |

---

## 2. Top-Level Directory Breakdown

| Directory / File | Size (MB) | Type | Notes |
|:---|---:|:---|:---|
| `GiveAid.Web/` | 238.99 | Dir | Main application source + build artifacts |
| `database/` | 5.36 | Dir | SQL Server backup + schema script |
| `.git/` | 2.80 | Dir | Git repository history |
| `docs/` | 0.35 | Dir | Project documentation (required) |
| `scratch/` | 0.12 | Dir | Temporary test scripts & logs |
| `tasks/` | 0.05 | Dir | Project roadmap task files |
| `test_task18_regression.ps1` | 0.03 | File | Automated test suite |
| Others (README, .sln, etc.) | < 0.01 | Files | Essential project files |

---

## 3. GiveAid.Web/ Internal Breakdown

| Sub-directory | Size (MB) | Regenerable? | Notes |
|:---|---:|:---|:---|
| **`bin/`** | **217.08** | ✅ Yes | **87.6% of total project.** Debug (110.66 MB) + Release (106.42 MB). Full rebuild artifacts. |
| **`obj/`** | **11.69** | ✅ Yes | Intermediate build cache, compressed assets. |
| `wwwroot/` | 9.36 | ❌ Required | Static assets: Bootstrap 5 (8.28 MB), jQuery (0.88 MB), CSS, JS, uploads |
| `Views/` | 0.38 | ❌ Required | Razor view files |
| `Migrations/` | 0.24 | ❌ Required | Entity Framework Core migration files |
| `Controllers/` | 0.14 | ❌ Required | MVC controller source code |
| `ViewModels/` | 0.03 | ❌ Required | View model classes |
| `Services/` | 0.03 | ❌ Required | Service layer classes |
| `Models/` | 0.02 | ❌ Required | Entity model classes |
| `Data/` | 0.01 | ❌ Required | ApplicationDbContext |
| `Properties/` | < 0.01 | ❌ Required | launchSettings.json |
| Config files | < 0.01 | ❌ Required | Program.cs, appsettings.json, .csproj |

---

## 4. Top 10 Largest Individual Files

| # | File | Size (MB) | Location | Regenerable? |
|:---:|:---|---:|:---|:---:|
| 1 | `libmsalruntime.so` | 36.37 | `bin/Release/...linux-x64/` | ✅ |
| 2 | `libmsalruntime.so` | 36.37 | `bin/Debug/...linux-x64/` | ✅ |
| 3 | `msalruntime.dylib` | 7.79 | `bin/Debug/...osx-x64/` | ✅ |
| 4 | `msalruntime.dylib` | 7.79 | `bin/Release/...osx-x64/` | ✅ |
| 5 | `msalruntime_arm64.dylib` | 7.48 | `bin/Release/...osx-arm64/` | ✅ |
| 6 | `msalruntime_arm64.dylib` | 7.48 | `bin/Debug/...osx-arm64/` | ✅ |
| 7 | `Microsoft.CodeAnalysis.CSharp.dll` | 6.52 | `bin/Debug/` | ✅ |
| 8 | `Microsoft.CodeAnalysis.CSharp.dll` | 6.52 | `bin/Release/` | ✅ |
| 9 | `GiveAidDb.bak` | 5.34 | `database/` | ❌ Keep |
| 10 | `Microsoft.CodeAnalysis.Workspaces.dll` | 4.03 | `bin/Debug/` | ✅ |

**All top 8 largest files are inside `bin/` — fully regenerable build output.**

---

## 5. Other Notable Items

| Category | Path | Size | Assessment |
|:---|:---|:---|:---|
| **Git repository** | `.git/` | 2.80 MB | Small, must keep |
| **Database backup** | `database/GiveAidDb.bak` | 5.34 MB | Required for submission |
| **Schema script** | `database/GiveAidDb_Schema.sql` | 0.02 MB | Required for submission |
| **Scratch temp files** | `scratch/` | 0.12 MB | Old test scripts & server logs, safe to remove |
| **Uploads** | `wwwroot/uploads/` | < 0.01 MB | Application upload directory (nearly empty) |
| **`.vs/` IDE cache** | N/A | 0 MB | Does not exist — already clean |
| **`node_modules/`** | N/A | 0 MB | Does not exist |
| **`__pycache__/`** | N/A | 0 MB | Does not exist |
| **Duplicate ZIPs/archives** | N/A | 0 MB | None found inside project |

---

## 6. Space Distribution Summary

```
bin/ (Debug + Release)  ████████████████████████████████████████████  217.08 MB  (87.6%)
obj/                    ██                                             11.69 MB   (4.7%)
wwwroot/                █                                               9.36 MB   (3.8%)
database/               ▌                                               5.36 MB   (2.2%)
.git/                   ▌                                               2.80 MB   (1.1%)
Everything else         ▏                                               1.43 MB   (0.6%)
                                                            Total:  247.72 MB
```

**The `bin/` and `obj/` folders alone account for 228.77 MB (92.3%) of the project and are fully regenerable via `dotnet restore && dotnet build`.**
