# Give-AID NGO — Bug Log & Regression Analysis

**Date:** 2026-10-16  
**Document Purpose:** Record defects, unexpected regressions, root causes, and resolution details identified during Task 18 End-to-End Testing.  
**Tested Against:** ASP.NET Core MVC, EF Core, SQL Server `.\SQLEXPRESS`, ASP.NET Core Identity.

---

## Bug Summary Matrix

| Bug ID | Module | Severity | Symptom | Status | Verified In |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **BUG-18-01** | Test Runner / PowerShell | Low | Variable name collision on `$pId` triggering `Cannot overwrite variable PID because it is read-only or constant` | **Resolved** | `test_task18_regression.ps1` |
| **BUG-18-02** | Contact Queries Admin | Medium | Admin query reply form lookup returned HTTP 404 because GET was directed to `/Admin/Queries/Reply/{id}` instead of `/Admin/Queries/Details/{id}` | **Resolved** | `test_task18_regression.ps1` & `AdminQueriesController` |
| **BUG-18-03** | Programme Interest | Medium | Expression of interest GET route `/Programmes/Apply/1` and `/Programmes/ExpressInterest?programmeId=1` failed with HTTP 404 when ID #1 did not exist in database | **Resolved** | `test_task18_regression.ps1` & `ProgrammesController` |
| **BUG-18-04** | Donation Privacy Guard | Medium | Cross-user receipt privacy check threw `InvalidOperationException` due to PowerShell `Invoke-WebRequest -MaximumRedirection 0 -ErrorAction Stop` behavior on redirect | **Resolved** | `test_task18_regression.ps1` & `DonateController` |
| **BUG-18-05** | SQL Output Parsing | Low | `sqlcmd` appending `(1 rows affected)` caused numeric cast failure `Cannot convert value "3 (1 rows affected)" to type "System.Int32"` | **Resolved** | `Run-Sql` helper function |

---

## Detailed Defect Reports

### BUG-18-01: PowerShell Variable Name Collision with Built-In `$PID`
- **Component:** Test Automation Suite (`test_task18_regression.ps1`)
- **Severity:** Low (Script execution)
- **Description:** During Step 4 Partner CRUD execution, the script attempted to assign `$pId = Run-Sql "SELECT TOP 1 Id FROM Partners WHERE Name = '$partnerName'"`. In PowerShell 5.1, variable names are case-insensitive and `$PID` is an automatic read-only variable representing the host process ID.
- **Root Cause:** Reusing `$pId` collided with the reserved `$PID` variable in the PowerShell execution environment.
- **Resolution:** Renamed variable to `$partnerId` across Partner creation, retrieval, and deletion steps.
- **Verification:** Step 4 Partner CRUD tests passed cleanly with test partner record created and deleted.

---

### BUG-18-02: Route Mismatch for Administrator Contact Query Reply View
- **Component:** Admin Query Reply Workflow (`AdminQueriesController.cs` / `Views/Admin/Queries/Details.cshtml`)
- **Severity:** Medium
- **Description:** Test step 6.1 received an HTTP 404 Not Found error when requesting the query reply form via `GET /Admin/Queries/Reply/{id}`.
- **Root Cause:** Architectural alignment in Task 13 unified the query inspection and reply interface on a single administrative view located at `GET /Admin/Queries/Details/{id}`, with replies submitted via `POST /Admin/Queries/Reply/{id}`. There was intentionally no dedicated GET endpoint for `/Reply/{id}`.
- **Resolution:** Updated test navigation flow to retrieve the reply form and anti-forgery token from `GET /Admin/Queries/Details/{id}`, and submit the official reply payload to `POST /Admin/Queries/Reply/{id}`.
- **Verification:** Admin official reply POST succeeded with HTTP 302 redirect and query status was successfully marked as "Answered" in SQL Server.

---

### BUG-18-03: Hardcoded Non-Existent ID in Programme Interest Submission
- **Component:** Welfare Programme Public Participation (`ProgrammesController.cs`)
- **Severity:** Medium
- **Description:** Test step 6.2 received an HTTP 404 when initiating an expression of interest with `programmeId=1`.
- **Root Cause:** The database seed data and previous testing cycles resulted in active programmes having IDs 4, 5, 6, and 7, while ID 1 was not present. `ProgrammesController.ExpressInterest` strictly validates that the referenced programme exists and returns `NotFound()` if it does not.
- **Resolution:** Implemented dynamic querying in the regression test suite:
  ```powershell
  $targetProgId = Run-Sql "SELECT TOP 1 Id FROM Programmes WHERE Status = 'Scheduled'"
  ```
  The test now dynamically binds to a verified active scheduled programme (ID #4).
- **Verification:** Programme interest submitted successfully (HTTP 302), duplicate prevention verified, and application logged in user's `/Programmes/MyInterests` view.

---

### BUG-18-04: Cross-User Receipt Privacy Assertion Handling in PowerShell
- **Component:** Donation Security & Receipt Access Control (`DonateController.cs`)
- **Severity:** Medium
- **Description:** Step 5.5 failed with `Operation is not valid due to the current state of the object` when testing cross-user access to another donor's receipt.
- **Root Cause:** In ASP.NET Core Identity, calling `return Forbid()` when a non-owner/non-admin requests `/Donate/Receipt/{id}` results in a cookie challenge redirecting (HTTP 302) to `/Account/AccessDenied?ReturnUrl=%2FDonate%2FReceipt%2F{id}`. In PowerShell 5.1, executing `Invoke-WebRequest` with both `-MaximumRedirection 0` and `-ErrorAction Stop` triggers an `InvalidOperationException` before the response status and headers can be examined.
- **Resolution:** Updated the request invocation to use `-ErrorAction SilentlyContinue` and verified that the response returns either HTTP 302 with `Location` header pointing to `AccessDenied`, or HTTP 403 Forbidden:
  ```powershell
  $intruderAttempt = Invoke-WebRequest -Uri ($BaseUrl + "/Donate/Receipt/" + $donationId) -WebSession $otherUserSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
  $isDenied = ($loc -match "AccessDenied" -or $loc -match "Login")
  ```
- **Verification:** Verified that unauthorized members are blocked with HTTP 302 redirecting to `AccessDenied`, preserving donor financial privacy.

---

### BUG-18-05: SQLCMD Row Count Output Interacting with Type Conversion
- **Component:** Database Query Utility (`Run-Sql` in `test_task18_regression.ps1`)
- **Severity:** Low
- **Description:** Calls to `[int](Run-Sql "...")` failed with `Input string was not in a correct format` because `sqlcmd` returned `"3  (1 rows affected)"`.
- **Root Cause:** Although `-h -1` suppresses column headers, SQL Server still prints row count status unless `NOCOUNT ON` is explicitly set.
- **Resolution:** Prepended `SET NOCOUNT ON; ` to all SQL queries executed by `Run-Sql`, and added regex parsing to isolate the integer result:
  ```powershell
  function Run-Sql($query) {
      $out = sqlcmd -S $sqlServer -d $sqlDb -C -h -1 -Q ("SET NOCOUNT ON; " + $query)
      $text = ("" + ($out -join " ")).Trim()
      $m = [regex]::Match($text, '^\s*(-?\d+)')
      if ($m.Success) { return [int]$m.Groups[1].Value }
      return 0
  }
  ```
- **Verification:** All subsequent database queries cleanly returned integer values for counts and IDs.

---

## Quality & Stability Conclusion

All 5 identified defects were analyzed, resolved, and re-tested against the live application and database. Regression test suite execution confirmed **38 of 38 tests passing (100%)** with zero remaining defects or blocking issues.
