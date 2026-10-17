# Give-AID NGO — Environment and Dependency Versions Specification

**Document Reference:** `docs/environment-and-versions.md`  
**Application Title:** Give-AID — Non-Governmental Organization Web Portal  
**Document Purpose:** Define exact SDK, runtime, database, package, and tooling versions required for local compilation, execution, and evaluation.  
**Verification Date:** 2026-10-16  

---

## 1. Verified Runtime & SDK Specification

The table below reflects the exact runtime environment verified on the primary development and testing workstation:

| Component | Target Version | Verification Command | Notes / Workstation Output |
| :--- | :---: | :--- | :--- |
| **.NET SDK** | `10.0.401` | `dotnet --version` | Installed in `C:\Program Files\dotnet\sdk\10.0.401` |
| **Target Framework** | `net10.0` | `<TargetFramework>net10.0</TargetFramework>` in `GiveAid.Web.csproj` | Modern unified .NET runtime |
| **ASP.NET Core Web SDK** | `10.0` | `<Project Sdk="Microsoft.NET.Sdk.Web">` | Built-in ASP.NET Core MVC & Razor engine |
| **C# Language Version** | `13.0` | Compiler Default | Implicit default for `net10.0` projects |
| **Database Engine** | SQL Server 2022 / Express | `SELECT @@VERSION` via SQLCMD | Tested against instance `.\SQLEXPRESS` |
| **Operating System** | Windows 10/11 x64 | `[System.Environment]::OSVersion` | Windows PowerShell 5.1 & PowerShell Core |

---

## 2. NuGet Package Dependencies & Versions

All dependencies are defined in [`GiveAid.Web/GiveAid.Web.csproj`](file:///d:/Give-AID%20NGO/GiveAid.Web/GiveAid.Web.csproj) and resolve cleanly via NuGet:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.12" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.12" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.12">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Package Descriptions
1. **`Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.12):** ASP.NET Core Identity store provider backed by Entity Framework Core. Handles password hashing (PBKDF2), role-based security (`Admin`, `User`), login cookies, and user management.
2. **`Microsoft.EntityFrameworkCore.SqlServer` (10.0.12):** Official Microsoft SQL Server database provider for EF Core. Translates LINQ queries to optimized T-SQL statements.
3. **`Microsoft.EntityFrameworkCore.Design` (10.0.12):** Design-time logic for scaffolding and executing EF Core migrations.
4. **`Microsoft.EntityFrameworkCore.Tools` (10.0.12):** Package Manager Console PowerShell cmdlets (`Add-Migration`, `Update-Database`) in Visual Studio.

---

## 3. Database Engine & Connection Requirements

### 3.1 Supported SQL Server Editions
- Microsoft SQL Server 2019 / 2022 (Developer, Standard, or Enterprise)
- Microsoft SQL Server Express 2019 / 2022 (Default named instance: `.\SQLEXPRESS`)
- Microsoft SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`)

### 3.2 Connection Security
- **Windows Integrated Authentication:** Default configured mode (`Integrated Security=True`). No database usernames or passwords stored in application files.
- **TLS / Certificate Handling:** Configured with `TrustServerCertificate=True` for seamless local evaluation without requiring custom SSL certificate installation on the SQL Server host.
- **Multiple Active Result Sets:** Enabled via `MultipleActiveResultSets=true` to support concurrent asynchronous EF Core queries.

---

## 4. Front-End Assets & UI Frameworks

The client-side user experience is built using lightweight, standards-compliant libraries bundled directly into `wwwroot/`:

| Library / Asset | Version | Distribution Method | Usage Scope |
| :--- | :---: | :--- | :--- |
| **Bootstrap** | `5.3.3` | Bundled in `wwwroot/lib/bootstrap/` | Responsive grid, modal popups, cards, alerts, flex utilities |
| **Give-AID CSS Theme** | Custom | `wwwroot/css/site.css` | Design system variables, color tokens (`#0D9488`, `#F59E0B`), badges |
| **Vanilla JavaScript** | ES6+ | `wwwroot/js/site.js` | Client-side donation calculation, image preview, accordion logic |
| **Google Fonts** | Inter & Plus Jakarta Sans | Web-safe system font fallback | Typography and headings |

---

## 5. Development Tools & Recommended IDEs

1. **Visual Studio 2022 (v17.12 or newer):**
   - Workload: *ASP.NET and web development*
   - Workload: *.NET desktop development*
2. **Visual Studio Code:**
   - Extension: *C# Dev Kit* (`ms-dotnettools.csdevkit`)
   - Extension: *C#* (`ms-dotnettools.csharp`)
   - Extension: *SQL Server (mssql)* (`ms-mssql.mssql`)
3. **Database Tools:**
   - SQL Server Management Studio (SSMS 19 or 20)
   - Azure Data Studio
4. **Command Line Utilities:**
   - .NET CLI (`dotnet`)
   - Entity Framework Core Global Tool: `dotnet tool install --global dotnet-ef --version 10.0.12`

---

## 6. How to Verify Workstation Environment

Run the following diagnostic commands in PowerShell to verify workstation readiness before opening the solution:

```powershell
# 1. Verify .NET SDK 10 is available
dotnet --version

# 2. List all installed .NET SDKs
dotnet --list-sdks

# 3. Verify SQL Server Express is running
Get-Service -Name "MSSQL$SQLEXPRESS"

# 4. Test connectivity to local SQL Server port
Test-NetConnection -ComputerName localhost -Port 1433
```
