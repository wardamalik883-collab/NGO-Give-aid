# Give-AID NGO — Local Setup and Demonstration Guide

**Document Reference:** `docs/setup-and-demo-guide.md`  
**Application Title:** Give-AID — Non-Governmental Organization Web Portal  
**Target Environment:** Local Development & Academic Evaluation  
**Framework:** ASP.NET Core MVC (.NET 10), C# 13, Entity Framework Core, SQL Server Express (`.\SQLEXPRESS`), ASP.NET Core Identity  
**Revision Date:** 2026-10-16  

---

## 1. System Requirements & Prerequisites

Before running the Give-AID web application locally, ensure your workstation meets the following hardware and software requirements:

### 1.1 Hardware Specifications
- **Processor:** x64 or ARM64 Dual-Core CPU (Quad-core 2.4 GHz+ recommended)
- **Memory (RAM):** Minimum 8 GB (16 GB recommended for concurrent SQL Server & Visual Studio)
- **Storage:** At least 5 GB available SSD storage for repository, dependencies, and database files

### 1.2 Required Software & SDKs
- **Operating System:** Windows 10/11 (Build 19041+) or modern Linux/macOS environment
- **.NET SDK:** Microsoft .NET 10.0 SDK (`10.0.100` or later)  
  *Verify via terminal:* `dotnet --version`
- **Database Engine:** Microsoft SQL Server 2019/2022 or Microsoft SQL Server Express (`.\SQLEXPRESS`) with LocalDB/Named Instance support
- **IDE / Editor:**
  - Visual Studio 2022 (v17.12 or newer) with *.NET desktop development* and *ASP.NET and web development* workloads, OR
  - Visual Studio Code with the *C# Dev Kit* and *SQL Server (mssql)* extensions
- **Command Line Shell:** Windows PowerShell 5.1+ or PowerShell Core 7+
- **Database Management (Optional):** SQL Server Management Studio (SSMS 19+) or Azure Data Studio

---

## 2. Opening the Existing Solution

The Give-AID codebase is structured as a standard modular ASP.NET Core solution.

1. Clone or navigate to the workspace directory:
   ```powershell
   cd "D:\Give-AID NGO"
   ```
2. Open the solution in Visual Studio:
   - Double-click `GiveAid.sln`, OR
   - Run from terminal:
     ```powershell
     start GiveAid.sln
     ```
3. Alternatively, open the directory in Visual Studio Code:
   ```powershell
   code .
   ```

---

## 3. Configuring Database Connectivity

Give-AID uses Entity Framework Core with SQL Server. The default connection string is configured in [`GiveAid.Web/appsettings.json`](file:///d:/Give-AID%20NGO/GiveAid.Web/appsettings.json).

### 3.1 Default Configuration (Windows Authentication)
For local development using SQL Server Express with Windows Integrated Security:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GiveAidDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3.2 Alternative Configuration (SQL Server Authentication)
If your SQL Server instance uses SQL Authentication (username/password), configure your connection string using local environment variables or User Secrets (do NOT commit real passwords to Git):
```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GiveAidDb;User Id=sa;Password=<YourStrongPassword>;TrustServerCertificate=True;MultipleActiveResultSets=true" --project GiveAid.Web
```

> [!IMPORTANT]
> Ensure the SQL Server service (`MSSQL$SQLEXPRESS` or `MSSQLSERVER`) is running before starting the application. You can check the service status in PowerShell:
> ```powershell
> Get-Service -Name "*SQL*"
> ```

---

## 4. Applying Entity Framework Core Database Migrations

Give-AID includes pre-configured EF Core migrations located in `GiveAid.Web/Migrations/`.

To create the `GiveAidDb` database and construct all 11 domain tables and ASP.NET Core Identity tables:

1. Open PowerShell in the project root:
   ```powershell
   cd "D:\Give-AID NGO"
   ```
2. Execute the EF Core migration command:
   ```powershell
   dotnet ef database update --project GiveAid.Web
   ```
3. *Alternative via Visual Studio Package Manager Console:*
   ```powershell
   Update-Database -Project GiveAid.Web
   ```

> [!NOTE]
> Upon database creation, the built-in [`DataSeedService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/DataSeedService.cs) automatically detects empty tables on the first web server run and populates baseline NGOs and initial urgent causes.

---

## 5. Configuring Development Secrets & Initial Admin Account

Give-AID implements safe credential management. Production passwords and administrator secrets are **never hardcoded in source control**. Instead, credentials are read from ASP.NET Core User Secrets or system environment variables.

### 5.1 Initialize User Secrets
```powershell
dotnet user-secrets init --project GiveAid.Web
```

### 5.2 Configure Initial Administrator Account
Set your desired demonstration administrator credentials using User Secrets:
```powershell
dotnet user-secrets set "AdminSeed:Email" "admin@giveaid.org" --project GiveAid.Web
dotnet user-secrets set "AdminSeed:Password" "Admin@123" --project GiveAid.Web
dotnet user-secrets set "AdminSeed:FullName" "Give-AID System Administrator" --project GiveAid.Web
```

> [!TIP]
> On the next application startup, [`RoleSeedService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/RoleSeedService.cs) will read these values, create the `admin@giveaid.org` account (if it does not exist), and assign it the `Admin` role.

### 5.3 Configure Optional SMTP Email Settings (Optional / Mailtrap)
For testing outgoing invitation emails through an SMTP sandbox such as Mailtrap:
```powershell
dotnet user-secrets set "Email:SmtpServer" "smtp.mailtrap.io" --project GiveAid.Web
dotnet user-secrets set "Email:Port" "2525" --project GiveAid.Web
dotnet user-secrets set "Email:SenderEmail" "noreply@giveaid.org" --project GiveAid.Web
dotnet user-secrets set "Email:SenderName" "Give-AID Notifications" --project GiveAid.Web
dotnet user-secrets set "Email:Username" "<your-sandbox-username>" --project GiveAid.Web
dotnet user-secrets set "Email:Password" "<your-sandbox-password>" --project GiveAid.Web
```
*If SMTP credentials are not configured, Give-AID gracefully simulates email delivery by logging message payloads to application diagnostics.*

---

## 6. Running the Application

### 6.1 Running via .NET CLI
```powershell
dotnet run --project GiveAid.Web
```
The application will build, apply seed operations, and begin listening:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5187
      Now listening on: https://localhost:7187
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 6.2 Running via Visual Studio
1. Set `GiveAid.Web` as the **Startup Project** in Solution Explorer.
2. Select the `http` or `https` launch profile from the run dropdown.
3. Press **F5** (Debug) or **Ctrl + F5** (Run without debugging).
4. The default browser will automatically navigate to `http://localhost:5187`.

---

## 7. Application Page Directory & URLs

| Area | Route | Access Level | Description |
| :--- | :--- | :--- | :--- |
| **Public** | `/` | Anonymous | Impact home banner, urgent appeals, partner showcase |
| **Public** | `/Home/About` | Anonymous | Organizational mission, leadership profiles, history |
| **Public** | `/Causes` | Anonymous | Searchable campaign catalog with financial progress |
| **Public** | `/Causes/Details/{id}` | Anonymous | Detailed campaign breakdown with direct donation CTA |
| **Public** | `/Partners` | Anonymous | Corporate sponsors & humanitarian partners directory |
| **Public** | `/Programmes` | Anonymous | Welfare outreach schedule & volunteer participation |
| **Public** | `/Gallery` | Anonymous | Categorized field photo gallery |
| **Public** | `/Contact` | Anonymous | Public inquiry form with validation |
| **Public** | `/Help` | Anonymous | Frequently Asked Questions accordion |
| **Public** | `/Account/Register` | Anonymous | Member registration form |
| **Public** | `/Account/Login` | Anonymous | Member & administrator sign-in portal |
| **Member** | `/Donate?causeId={id}` | Authenticated | Multi-step educational simulated donation engine |
| **Member** | `/Donate/History` | Authenticated | Personal donation transaction log & receipts |
| **Member** | `/Programmes/MyInterests` | Authenticated | Personal volunteer & participant applications |
| **Member** | `/Invite` | Authenticated | Invite colleagues to join Give-AID campaigns |
| **Admin** | `/Admin` | Admin Role | Administrative command center & live SQL metrics |
| **Admin** | `/Admin/Causes` | Admin Role | Fundraising campaign CRUD & featured triggers |
| **Admin** | `/Admin/Ngos` | Admin Role | Partner NGO accredited organization registry |
| **Admin** | `/Admin/Partners` | Admin Role | Corporate partner & tier directory management |
| **Admin** | `/Admin/Programmes` | Admin Role | Welfare programme scheduling & attendance |
| **Admin** | `/Admin/Gallery` | Admin Role | Photo gallery uploads with magic byte checks |
| **Admin** | `/Admin/Queries` | Admin Role | Inquiry triage, response composition & logging |

---

## 8. Creating Demonstration Accounts & Role Verification

### 8.1 Creating a Standard Member Account
1. Open your browser and navigate to `http://localhost:5187/Account/Register`.
2. Fill out the registration form:
   - **Full Name:** `Sarah Jenkins`
   - **Email Address:** `sarah.jenkins@example.com`
   - **Password:** `Member@2026!`
   - **Confirm Password:** `Member@2026!`
3. Click **Create Account**.
4. Upon successful registration, the application automatically signs in the user with the `User` role and redirects to the home page.
5. The top navigation bar updates to display "Welcome, Sarah Jenkins", a link to "My Donations", "My Interests", and "Sign Out".

### 8.2 Verifying Role-Based Access Control (RBAC) Protection
To verify that non-administrative users cannot access restricted management modules:
1. While logged in as `sarah.jenkins@example.com`, attempt to navigate directly to:
   ```
   http://localhost:5187/Admin
   ```
2. **Expected & Verified Result:** The application intercepts the request, blocks access, and redirects to:
   ```
   http://localhost:5187/Account/AccessDenied?ReturnUrl=%2FAdmin
   ```
3. A user-friendly message is displayed stating: *"Access Denied — You do not have permission to access this administrative resource."*

### 8.3 Logging In as Administrator & Verifying Dashboard
1. Click **Sign Out** or navigate to `http://localhost:5187/Account/Login`.
2. Enter the configured administrator credentials:
   - **Email:** `admin@giveaid.org` (or your configured `AdminSeed:Email`)
   - **Password:** `Admin@123` (or your configured `AdminSeed:Password`)
3. Click **Sign In**.
4. The navigation bar now displays an **Admin Portal** badge and dropdown.
5. Navigate to `http://localhost:5187/Admin`.
6. **Expected & Verified Result:** The comprehensive Administrative Dashboard renders, showing live summary cards (Total Raised, Active Causes, Affiliated NGOs, Corporate Partners, Scheduled Programmes, Gallery Photos, Unanswered Queries, Pending Volunteer Applications) and management navigation shortcuts.

---

## 9. Common Setup Issues & Troubleshooting

### Issue 1: Cannot Connect to SQL Server (`Cannot open database "GiveAidDb"`)
- **Cause:** SQL Server Express service is stopped or the database has not been initialized.
- **Solution:**
  1. Open PowerShell as Administrator and run:
     ```powershell
     Start-Service -Name "MSSQL$SQLEXPRESS"
     ```
  2. Ensure migrations have been applied:
     ```powershell
     dotnet ef database update --project GiveAid.Web
     ```

### Issue 2: Port Conflict (`Address already in use: 5187`)
- **Cause:** A previous instance of `dotnet run` or another application is occupying port 5187.
- **Solution:**
  1. Identify the occupying process:
     ```powershell
     Get-NetTCPConnection -LocalPort 5187 | Select-Object OwningProcess
     ```
  2. Terminate the process or change the port in `GiveAid.Web/Properties/launchSettings.json`.

### Issue 3: Administrator Account Not Seeded
- **Cause:** The application was started without configuring `AdminSeed:Email` and `AdminSeed:Password`.
- **Solution:**
  1. Set the User Secrets as described in Section 5.2.
  2. Restart the web application via `dotnet run --project GiveAid.Web`.
  3. Inspect the terminal output to confirm: *"Initial admin account seeded successfully."*

### Issue 4: Image Upload Fails (`Invalid file type or corrupt header`)
- **Cause:** The uploaded file is either not an image or fails magic byte verification (e.g., an executable renamed with a `.jpg` extension).
- **Solution:** Ensure you upload a genuine JPG, PNG, or WebP image under 5 MB. The server rigorously inspects binary headers to prevent malicious payloads.

---

## 10. Summary & Demonstration Verification

Following the procedures detailed in this guide guarantees an identical runtime environment to the evaluated testing baseline. All features, workflows, and administrative controls are verified functional, secure, and ready for supervisory assessment.
