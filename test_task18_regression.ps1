param (
    [string]$BaseUrl = "http://localhost:5187"
)

$ErrorActionPreference = "Continue"
$sqlServer = '.\SQLEXPRESS'
$sqlDb = 'GiveAidDb'

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Give-AID NGO - Task 18 Comprehensive End-to-End Test Suite      " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

# 1. Start Server if not already listening
$port = 5187
$testConn = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue
$startedProcess = $null

if (-not $testConn.TcpTestSucceeded) {
    Write-Host "[STEP 1] Starting GiveAid.Web..." -ForegroundColor Yellow
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = "dotnet"
    $startInfo.Arguments = 'run --project "d:\Give-AID NGO\GiveAid.Web\GiveAid.Web.csproj" --no-launch-profile --urls ' + $BaseUrl
    $startInfo.WorkingDirectory = "d:\Give-AID NGO\GiveAid.Web"
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startedProcess = [System.Diagnostics.Process]::Start($startInfo)

    Write-Host "[INFO] Waiting for web application to start up..." -ForegroundColor Yellow
    $retries = 25
    $ready = $false
    while ($retries -gt 0 -and -not $ready) {
        Start-Sleep -Seconds 2
        try {
            $resp = Invoke-WebRequest -Uri $BaseUrl -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) { $ready = $true }
        } catch {
            $retries--
            Write-Host ("Waiting... " + $retries + " left") -ForegroundColor Gray
        }
    }

    if (-not $ready) {
        Write-Host "ERROR: Could not connect to GiveAid.Web" -ForegroundColor Red
        if ($startedProcess) { $startedProcess.Kill() }
        exit 1
    }
}

Write-Host "[OK] Server is listening and responding" -ForegroundColor Green

$testResults = @()

function Log-Test($step, $testName, $passed, $details) {
    $stat = if ($passed) { "PASS" } else { "FAIL" }
    $col = if ($passed) { "Green" } else { "Red" }
    Write-Host ("[" + $stat + "] " + $step + " - " + $testName + " : " + $details) -ForegroundColor $col
    $script:testResults += [PSCustomObject]@{
        Step     = $step
        TestName = $testName
        Status   = $stat
        Details  = $details
    }
}

function Get-Token($content) {
    $pattern = '__RequestVerificationToken"[^>]*value="([^"]+)"'
    $m = [regex]::Match($content, $pattern)
    if ($m.Success) { return $m.Groups[1].Value }
    return ""
}

function Get-InputValue($content, $name) {
    $pattern = 'name="' + $name + '"[^>]*value="([^"]*)"'
    $m = [regex]::Match($content, $pattern)
    if ($m.Success) { return $m.Groups[1].Value }
    return ""
}

function Run-Sql($query) {
    $out = sqlcmd -S $sqlServer -d $sqlDb -C -h -1 -Q ("SET NOCOUNT ON; " + $query)
    $text = ("" + ($out -join " ")).Trim()
    $m = [regex]::Match($text, '^\s*(-?\d+)')
    if ($m.Success) {
        return [int]$m.Groups[1].Value
    }
    return 0
}

# =============================================================================
# STEP 2: TEST PUBLIC WEBSITE PAGES
# =============================================================================
Write-Host ""
Write-Host "--- [STEP 2] Testing Public Website Pages ---" -ForegroundColor Cyan

$publicPages = @(
    @{ Url = "/"; Name = "Home Page"; Check = "Give-AID" },
    @{ Url = "/Home/About"; Name = "About Us Page"; Check = "About" },
    @{ Url = "/Causes"; Name = "Causes Catalog"; Check = "Cause" },
    @{ Url = "/Causes/Details/1"; Name = "Cause Details View"; Check = "Target" },
    @{ Url = "/Partners"; Name = "Corporate Partners"; Check = "Partners" },
    @{ Url = "/Programmes"; Name = "Welfare Programmes"; Check = "Programme" },
    @{ Url = "/Gallery"; Name = "Photo Gallery"; Check = "Gallery" },
    @{ Url = "/Contact"; Name = "Contact Us"; Check = "Contact" },
    @{ Url = "/Help"; Name = "Help and FAQ Centre"; Check = "Frequently Asked Questions" },
    @{ Url = "/Account/Login"; Name = "Member Login Page"; Check = "Sign In" },
    @{ Url = "/Account/Register"; Name = "Member Registration Page"; Check = "Register" }
)

foreach ($p in $publicPages) {
    try {
        $fullUrl = $BaseUrl + $p.Url
        $resp = Invoke-WebRequest -Uri $fullUrl -UseBasicParsing -TimeoutSec 10
        $passed = ($resp.StatusCode -eq 200) -and ($resp.Content -match $p.Check)
        Log-Test "Step 2" $p.Name $passed ("HTTP " + $resp.StatusCode)
    } catch {
        Log-Test "Step 2" $p.Name $false $_.Exception.Message
    }
}

# =============================================================================
# STEP 3: TEST AUTHENTICATION AND AUTHORIZATION
# =============================================================================
Write-Host ""
Write-Host "--- [STEP 3] Testing Authentication and Authorization ---" -ForegroundColor Cyan

$userSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$adminSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# 3.1 Invalid login credentials rejected
try {
    $loginPage = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Login") -WebSession $userSession -UseBasicParsing
    $token = Get-Token $loginPage.Content

    $badLoginBody = @{
        "Email" = "nonexistent_user@giveaid.org"
        "Password" = "WrongPassword99!"
        "RememberMe" = "false"
        "__RequestVerificationToken" = $token
    }
    $badLoginResp = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Login") -Method Post -Body $badLoginBody -WebSession $userSession -UseBasicParsing
    $invalidLoginRejected = ($badLoginResp.Content -match "Invalid login credentials")
    Log-Test "Step 3" "Invalid Login Rejected" $invalidLoginRejected "Returned error message as expected"
} catch {
    Log-Test "Step 3" "Invalid Login Rejected" $false $_.Exception.Message
}

# 3.2 User Registration (New Member)
$testEmail = "regression_test_" + [Guid]::NewGuid().ToString().Substring(0, 8) + "@giveaid.org"
$testPassword = "Password123"

try {
    $regPage = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -WebSession $userSession -UseBasicParsing
    $token = Get-Token $regPage.Content

    $regBody = @{
        "FullName" = "Regression Test User"
        "Email" = $testEmail
        "Password" = $testPassword
        "ConfirmPassword" = $testPassword
        "__RequestVerificationToken" = $token
    }

    $regResp = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -Method Post -Body $regBody -WebSession $userSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $registeredOk = ($regResp.StatusCode -eq 302)
    Log-Test "Step 3" "New User Registration" $registeredOk ("Created user " + $testEmail + " (HTTP " + $regResp.StatusCode + ")")
} catch {
    Log-Test "Step 3" "New User Registration" $false $_.Exception.Message
}

# 3.3 Duplicate Registration Rejected
try {
    $dupSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $regPage2 = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -WebSession $dupSession -UseBasicParsing
    $token2 = Get-Token $regPage2.Content

    $dupBody = @{
        "FullName" = "Duplicate User"
        "Email" = $testEmail
        "Password" = $testPassword
        "ConfirmPassword" = $testPassword
        "__RequestVerificationToken" = $token2
    }
    $dupResp = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -Method Post -Body $dupBody -WebSession $dupSession -UseBasicParsing
    $dupRejected = ($dupResp.Content -match "already taken" -or $dupResp.Content -match "already registered" -or $dupResp.Content -match "is already")
    Log-Test "Step 3" "Duplicate Email Registration Blocked" $dupRejected "Server rejected duplicate email registration"
} catch {
    Log-Test "Step 3" "Duplicate Email Registration Blocked" $false $_.Exception.Message
}

# 3.4 Regular User Access to Admin Area Rejected
try {
    $adminAttempt = Invoke-WebRequest -Uri ($BaseUrl + "/Admin") -WebSession $userSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    if ($adminAttempt -and ($adminAttempt.StatusCode -eq 302 -or $adminAttempt.StatusCode -eq 301)) {
        $loc = $adminAttempt.Headers["Location"]
        $isForbiddenOrDenied = ($loc -match "AccessDenied" -or $loc -match "Login")
        Log-Test "Step 3" "Regular User Denied from /Admin" $isForbiddenOrDenied ("HTTP " + $adminAttempt.StatusCode + " -> " + $loc)
    } else {
        Log-Test "Step 3" "Regular User Denied from /Admin" $false ("Status: " + $adminAttempt.StatusCode)
    }
} catch {
    if ($_.Exception.Response) {
        $code = [int]$_.Exception.Response.StatusCode
        Log-Test "Step 3" "Regular User Denied from /Admin" ($code -eq 403 -or $code -eq 302) ("HTTP " + $code)
    } else {
        Log-Test "Step 3" "Regular User Denied from /Admin" $false $_.Exception.Message
    }
}

# 3.5 Admin User Authentication
try {
    $adminLoginPage = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Login") -WebSession $adminSession -UseBasicParsing
    $adminToken = Get-Token $adminLoginPage.Content

    $adminLoginBody = @{
        "Email" = "admin@giveaid.org"
        "Password" = "Admin@123"
        "RememberMe" = "false"
        "__RequestVerificationToken" = $adminToken
    }
    $adminLoginResp = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Login") -Method Post -Body $adminLoginBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $adminAuthOk = ($adminLoginResp.StatusCode -eq 302)
    Log-Test "Step 3" "Admin Login (admin@giveaid.org)" $adminAuthOk "Admin authenticated successfully"

    # Admin access to /Admin allowed
    $adminDash = Invoke-WebRequest -Uri ($BaseUrl + "/Admin") -WebSession $adminSession -UseBasicParsing
    $dashOk = ($adminDash.StatusCode -eq 200) -and ($adminDash.Content -match "Administrative Dashboard")
    Log-Test "Step 3" "Admin Authorized Access to /Admin" $dashOk "HTTP 200 - Dashboard rendered"
} catch {
    Log-Test "Step 3" "Admin Login" $false $_.Exception.Message
}

# =============================================================================
# STEP 4: TEST CRUD FUNCTIONALITY (NGOs, Partners, Programmes)
# =============================================================================
Write-Host ""
Write-Host "--- [STEP 4] Testing CRUD Functionality (Admin) ---" -ForegroundColor Cyan

# 4.1 NGO CRUD
$createdNgoId = 0
try {
    # C - Create NGO
    $ngoCreatePage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Create") -WebSession $adminSession -UseBasicParsing
    $ngoToken = Get-Token $ngoCreatePage.Content

    $ngoUniqueName = "Automated Test NGO " + [Guid]::NewGuid().ToString().Substring(0, 6)
    $ngoBody = @{
        "Name" = $ngoUniqueName
        "RegistrationNumber" = "NGO-TEST-999"
        "Description" = "Temporary test NGO created by regression test suite."
        "Address" = "Islamabad, Pakistan"
        "Email" = "test@automatedngo.org"
        "Phone" = "+92 51 9999999"
        "WebsiteUrl" = "https://automatedngo.org"
        "IsActive" = "true"
        "__RequestVerificationToken" = $ngoToken
    }

    $ngoCreateResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Create") -Method Post -Body $ngoBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $ngoCreated = ($ngoCreateResp.StatusCode -eq 302)
    Log-Test "Step 4" "NGO Create (POST)" $ngoCreated ("Created " + $ngoUniqueName)

    # R - Read in Index
    $ngosIndex = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos") -WebSession $adminSession -UseBasicParsing
    $ngoFound = ($ngosIndex.Content -match $ngoUniqueName)
    Log-Test "Step 4" "NGO Read (List)" $ngoFound "Verified record appears in table"

    # Extract ID from SQL
    $createdNgoId = Run-Sql "SELECT TOP 1 Id FROM NGOs WHERE Name = '$ngoUniqueName'"

    if ($createdNgoId -gt 0) {
        # U - Update / Edit
        $editPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Edit/" + $createdNgoId) -WebSession $adminSession -UseBasicParsing
        $editToken = Get-Token $editPage.Content
        $updatedNgoName = $ngoUniqueName + " (Updated)"

        $editBody = @{
            "Id" = $createdNgoId
            "Name" = $updatedNgoName
            "RegistrationNumber" = "NGO-TEST-999-EDIT"
            "Description" = "Updated description."
            "Address" = "Lahore, Pakistan"
            "Email" = "updated@automatedngo.org"
            "IsActive" = "true"
            "__RequestVerificationToken" = $editToken
        }
        $editResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Edit/" + $createdNgoId) -Method Post -Body $editBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        $editOk = ($editResp.StatusCode -eq 302)
        Log-Test "Step 4" "NGO Update (POST)" $editOk ("Updated name to " + $updatedNgoName)

        # D - Delete
        $delPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Delete/" + $createdNgoId) -WebSession $adminSession -UseBasicParsing
        $delToken = Get-Token $delPage.Content
        $delBody = @{ "__RequestVerificationToken" = $delToken }

        $delResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Ngos/Delete/" + $createdNgoId) -Method Post -Body $delBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        $delOk = ($delResp.StatusCode -eq 302)

        # Verify deleted in database
        $checkCount = Run-Sql "SELECT COUNT(*) FROM NGOs WHERE Id = $createdNgoId"
        Log-Test "Step 4" "NGO Delete (POST and Verify)" ($delOk -and ($checkCount -eq 0)) "Record cleanly deleted from SQL Server"
    }
} catch {
    Log-Test "Step 4" "NGO CRUD Exception" $false $_.Exception.Message
}

# 4.2 Partner CRUD
try {
    $partnerPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Partners/Create") -WebSession $adminSession -UseBasicParsing
    $partnerToken = Get-Token $partnerPage.Content
    $partnerName = "Test Partner " + [Guid]::NewGuid().ToString().Substring(0, 6)

    $partnerBody = @{
        "Name" = $partnerName
        "PartnerType" = "Corporate Sponsor"
        "Description" = "Test corporate sponsor."
        "DisplayOrder" = "1"
        "IsActive" = "true"
        "__RequestVerificationToken" = $partnerToken
    }
    $partnerResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Partners/Create") -Method Post -Body $partnerBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $pCreated = ($partnerResp.StatusCode -eq 302)
    Log-Test "Step 4" "Partner Create (POST)" $pCreated ("Created " + $partnerName)

    $partnerId = Run-Sql "SELECT TOP 1 Id FROM Partners WHERE Name = '$partnerName'"
    if ($partnerId -gt 0) {
        $pDelPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Partners/Delete/" + $partnerId) -WebSession $adminSession -UseBasicParsing
        $pDelToken = Get-Token $pDelPage.Content
        $pDelResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Partners/Delete/" + $partnerId) -Method Post -Body @{ "__RequestVerificationToken" = $pDelToken } -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        Log-Test "Step 4" "Partner Delete (POST)" ($pDelResp.StatusCode -eq 302) ("Cleaned up test partner ID " + $partnerId)
    }
} catch {
    Log-Test "Step 4" "Partner CRUD Exception" $false $_.Exception.Message
}

# 4.3 Programme CRUD
try {
    $progPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Programmes/Create") -WebSession $adminSession -UseBasicParsing
    $progToken = Get-Token $progPage.Content
    $progTitle = "Test Welfare Camp " + [Guid]::NewGuid().ToString().Substring(0, 6)

    $progBody = @{
        "Title" = $progTitle
        "Description" = "Test humanitarian relief drive description."
        "StartDate" = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm")
        "Location" = "Rawalpindi Community Centre"
        "CoordinatorName" = "Muhammad Ali"
        "CoordinatorContact" = "+92 300 1234567"
        "Status" = "Scheduled"
        "__RequestVerificationToken" = $progToken
    }
    $progResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Programmes/Create") -Method Post -Body $progBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Log-Test "Step 4" "Programme Create (POST)" ($progResp.StatusCode -eq 302) ("Created programme " + $progTitle)

    $progId = Run-Sql "SELECT TOP 1 Id FROM Programmes WHERE Title = '$progTitle'"
    if ($progId -gt 0) {
        $prDelPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Programmes/Delete/" + $progId) -WebSession $adminSession -UseBasicParsing
        $prDelToken = Get-Token $prDelPage.Content
        $prDelResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Programmes/Delete/" + $progId) -Method Post -Body @{ "__RequestVerificationToken" = $prDelToken } -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        Log-Test "Step 4" "Programme Delete (POST)" ($prDelResp.StatusCode -eq 302) ("Cleaned up test programme ID " + $progId)
    }
} catch {
    Log-Test "Step 4" "Programme CRUD Exception" $false $_.Exception.Message
}

# =============================================================================
# STEP 5: TEST DONATION WORKFLOW
# =============================================================================
Write-Host ""
Write-Host "--- [STEP 5] Testing Donation Workflow (Simulated) ---" -ForegroundColor Cyan

try {
    # 5.1 Invalid Donation Amount Rejected Server-Side
    $donatePage = Invoke-WebRequest -Uri ($BaseUrl + "/Donate") -WebSession $userSession -UseBasicParsing
    $dToken = Get-Token $donatePage.Content

    $badDonationBody = @{
        "CauseId" = "1"
        "Amount" = "-50"
        "DonorName" = "Test Donor"
        "DonorEmail" = $testEmail
        "__RequestVerificationToken" = $dToken
    }
    $badDonationResp = Invoke-WebRequest -Uri ($BaseUrl + "/Donate") -Method Post -Body $badDonationBody -WebSession $userSession -UseBasicParsing
    $negRejected = ($badDonationResp.Content -match "between.*1 and" -or $badDonationResp.Content -match "greater than" -or $badDonationResp.Content -match "PKR 1" -or $badDonationResp.Content -match "invalid")
    Log-Test "Step 5" "Negative Donation Rejected" $negRejected "Server rejected negative donation amount"

    # 5.2 Valid Simulated Donation Initiation
    $goodDonationBody = @{
        "CauseId" = "1"
        "Amount" = "150.00"
        "DonorName" = "Regression Donor"
        "DonorEmail" = $testEmail
        "DonorPhone" = "+92 321 0000000"
        "__RequestVerificationToken" = $dToken
    }
    $initResp = Invoke-WebRequest -Uri ($BaseUrl + "/Donate") -Method Post -Body $goodDonationBody -WebSession $userSession -UseBasicParsing
    $atPaymentPage = ($initResp.Content -match "Simulated Payment Step" -or $initResp.Content -match "Demo Transaction")
    Log-Test "Step 5" "Donation Initiation (POST)" $atPaymentPage "Navigated to simulated payment step"

    # 5.3 Complete Simulated Payment
    $payToken = Get-Token $initResp.Content
    $causeId = Get-InputValue $initResp.Content "CauseId"
    $causeTitle = Get-InputValue $initResp.Content "CauseTitle"
    $ngoName = Get-InputValue $initResp.Content "NgoName"
    $amount = Get-InputValue $initResp.Content "Amount"
    $donorName = Get-InputValue $initResp.Content "DonorName"
    $donorEmail = Get-InputValue $initResp.Content "DonorEmail"
    $donorPhone = Get-InputValue $initResp.Content "DonorPhone"
    $idempToken = Get-InputValue $initResp.Content "IdempotencyToken"

    $payBody = @{
        "CauseId" = $causeId
        "CauseTitle" = $causeTitle
        "NgoName" = $ngoName
        "Amount" = $amount
        "DonorName" = $donorName
        "DonorEmail" = $donorEmail
        "DonorPhone" = $donorPhone
        "IdempotencyToken" = $idempToken
        "SimulatedOutcome" = "Success"
        "__RequestVerificationToken" = $payToken
    }

    $payResp = Invoke-WebRequest -Uri ($BaseUrl + "/Donate/ProcessPayment") -Method Post -Body $payBody -WebSession $userSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $payCompleted = ($payResp.StatusCode -eq 302)
    Log-Test "Step 5" "Simulated Payment Processed" $payCompleted "Payment simulated successfully and redirected"

    $loc = if ($payResp.Headers["Location"]) { $payResp.Headers["Location"] } else { "" }
    $mId = [regex]::Match($loc, 'Receipt/(\d+)')
    $donationId = if ($mId.Success) { [int]$mId.Groups[1].Value } else { Run-Sql "SELECT TOP 1 Id FROM Donations WHERE DonorEmail = '$testEmail' ORDER BY Id DESC" }

    if ($donationId -gt 0) {
        # 5.4 View Receipt by Donor
        $receipt = Invoke-WebRequest -Uri ($BaseUrl + "/Donate/Receipt/" + $donationId) -WebSession $userSession -UseBasicParsing
        $receiptOk = ($receipt.StatusCode -eq 200) -and ($receipt.Content -match "Donation Receipt" -or $receipt.Content -match "Contribution" -or $receipt.Content -match "Receipt")
        Log-Test "Step 5" "Donor Receipt Accessible" $receiptOk ("Donor successfully viewed receipt #" + $donationId)

        # 5.5 Cross-User Privacy Test
        $otherUserSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $otherEmail = "intruder_" + [Guid]::NewGuid().ToString().Substring(0, 6) + "@giveaid.org"
        $oRegPage = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -WebSession $otherUserSession -UseBasicParsing
        $oToken = Get-Token $oRegPage.Content
        $oRegBody = @{
            "FullName" = "Intruder User"
            "Email" = $otherEmail
            "Password" = "Password123"
            "ConfirmPassword" = "Password123"
            "__RequestVerificationToken" = $oToken
        }
        $oResp = Invoke-WebRequest -Uri ($BaseUrl + "/Account/Register") -Method Post -Body $oRegBody -WebSession $otherUserSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

        try {
            $intruderAttempt = Invoke-WebRequest -Uri ($BaseUrl + "/Donate/Receipt/" + $donationId) -WebSession $otherUserSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
            if ($intruderAttempt -and ($intruderAttempt.StatusCode -eq 302 -or $intruderAttempt.StatusCode -eq 301)) {
                $loc = $intruderAttempt.Headers["Location"]
                $isDenied = ($loc -match "AccessDenied" -or $loc -match "Login")
                Log-Test "Step 5" "Cross-User Receipt Access Blocked" $isDenied ("HTTP " + $intruderAttempt.StatusCode + " -> " + $loc)
            } elseif ($intruderAttempt -and $intruderAttempt.StatusCode -eq 403) {
                Log-Test "Step 5" "Cross-User Receipt Access Blocked" $true "HTTP 403 Forbidden"
            } else {
                Log-Test "Step 5" "Cross-User Receipt Access Blocked" $false ("Status: " + $intruderAttempt.StatusCode)
            }
        } catch {
            if ($_.Exception.Response) {
                $code = [int]$_.Exception.Response.StatusCode
                Log-Test "Step 5" "Cross-User Receipt Access Blocked" ($code -eq 403 -or $code -eq 302) ("HTTP " + $code + " - Access denied as expected")
            } else {
                Log-Test "Step 5" "Cross-User Receipt Access Blocked" $false $_.Exception.Message
            }
        }
    }
} catch {
    Log-Test "Step 5" "Donation Workflow Exception" $false $_.Exception.Message
}

# =============================================================================
# STEP 6: TEST CONTACT, INVITATION, AND PROGRAMME INTEREST
# =============================================================================
Write-Host ""
Write-Host "--- [STEP 6] Testing Contact, Invitation, and Programme Interest ---" -ForegroundColor Cyan

# 6.1 Contact Query Submission
try {
    $cSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $cPage = Invoke-WebRequest -Uri ($BaseUrl + "/Contact") -WebSession $cSession -UseBasicParsing
    $cToken = Get-Token $cPage.Content
    $cSubject = "Automated Regression Question " + [Guid]::NewGuid().ToString().Substring(0, 6)

    $cBody = @{
        "Name" = "Ahmad Khan"
        "Email" = "ahmad.khan@example.org"
        "Phone" = "+92 300 5551234"
        "Subject" = $cSubject
        "Message" = "This is an automated test inquiry submitted to verify contact storage."
        "__RequestVerificationToken" = $cToken
    }
    $cResp = Invoke-WebRequest -Uri ($BaseUrl + "/Contact") -Method Post -Body $cBody -WebSession $cSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Log-Test "Step 6" "Contact Query Submitted" ($cResp.StatusCode -eq 302) ("Submitted inquiry " + $cSubject)

    # Verify query stored in database
    $qCount = Run-Sql "SELECT COUNT(*) FROM ContactQueries WHERE Subject = '$cSubject'"
    Log-Test "Step 6" "Contact Query Saved in SQL" ($qCount -gt 0) "Record found in ContactQueries table"

    # Test Admin Query Reply
    $qId = Run-Sql "SELECT TOP 1 Id FROM ContactQueries WHERE Subject = '$cSubject'"
    if ($qId -gt 0) {
        $repPage = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Queries/Details/" + $qId) -WebSession $adminSession -UseBasicParsing
        $repToken = Get-Token $repPage.Content
        $repBody = @{
            "QueryId" = $qId
            "AdminReply" = "Official automated response from administrative coordination."
            "__RequestVerificationToken" = $repToken
        }
        $repResp = Invoke-WebRequest -Uri ($BaseUrl + "/Admin/Queries/Reply/" + $qId) -Method Post -Body $repBody -WebSession $adminSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        Log-Test "Step 6" "Admin Query Reply (POST)" ($repResp.StatusCode -eq 302) ("Admin posted official reply to Query #" + $qId)
    }
} catch {
    Log-Test "Step 6" "Contact Query Exception" $false $_.Exception.Message
}

# 6.2 Programme Interest Application
try {
    $targetProgId = Run-Sql "SELECT TOP 1 Id FROM Programmes WHERE Status = 'Scheduled'"
    if ($targetProgId -le 0) {
        $targetProgId = Run-Sql "SELECT TOP 1 Id FROM Programmes"
    }

    $progInterestPage = Invoke-WebRequest -Uri ($BaseUrl + "/Programmes/ExpressInterest?programmeId=" + $targetProgId) -WebSession $userSession -UseBasicParsing
    $piToken = Get-Token $progInterestPage.Content
    $applicantEmail = "volunteer_" + [Guid]::NewGuid().ToString().Substring(0, 6) + "@giveaid.org"

    $piBody = @{
        "ProgrammeId" = $targetProgId.ToString()
        "ApplicantName" = "Zahid Volunteer"
        "ApplicantEmail" = $applicantEmail
        "ApplicantPhone" = "+92 333 1112233"
        "InterestType" = "Volunteer"
        "Message" = "Interested in providing fieldwork assistance."
        "__RequestVerificationToken" = $piToken
    }
    $piResp = Invoke-WebRequest -Uri ($BaseUrl + "/Programmes/ExpressInterest") -Method Post -Body $piBody -WebSession $userSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Log-Test "Step 6" "Programme Interest Submitted" ($piResp.StatusCode -eq 302) ("Submitted interest for Programme #" + $targetProgId)

    # Test duplicate submission rejected
    $piDup = Invoke-WebRequest -Uri ($BaseUrl + "/Programmes/ExpressInterest") -Method Post -Body $piBody -WebSession $userSession -UseBasicParsing
    $piDupRejected = ($piDup.Content -match "already been submitted")
    Log-Test "Step 6" "Duplicate Volunteer Submission Blocked" $piDupRejected "Duplicate applicant email prevented"

    # Test My Applications view
    $myInterests = Invoke-WebRequest -Uri ($BaseUrl + "/Programmes/MyInterests") -WebSession $userSession -UseBasicParsing
    Log-Test "Step 6" "User MyInterests History View" ($myInterests.StatusCode -eq 200) "HTTP 200 - User saw their applications"
} catch {
    Log-Test "Step 6" "Programme Interest Exception" $false $_.Exception.Message
}

# 6.3 Invite Friends Workflow
try {
    $invPage = Invoke-WebRequest -Uri ($BaseUrl + "/Invite") -WebSession $userSession -UseBasicParsing
    $invToken = Get-Token $invPage.Content

    $friendEmail = "friend_" + [Guid]::NewGuid().ToString().Substring(0, 6) + "@example.org"
    $invBody = @{
        "RecipientEmail" = $friendEmail
        "Message" = "Check out Give-AID humanitarian initiatives!"
        "__RequestVerificationToken" = $invToken
    }
    $invResp = Invoke-WebRequest -Uri ($BaseUrl + "/Invite") -Method Post -Body $invBody -WebSession $userSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Log-Test "Step 6" "Invite Friend Submitted" ($invResp.StatusCode -eq 302) ("Invitation sent to " + $friendEmail)

    # Verify saved in SQL Invitations table
    $invCount = Run-Sql "SELECT COUNT(*) FROM Invitations WHERE RecipientEmail = '$friendEmail'"
    Log-Test "Step 6" "Invitation Saved in SQL" ($invCount -gt 0) "Record logged in Invitations table"
} catch {
    Log-Test "Step 6" "Invite Friends Exception" $false $_.Exception.Message
}

# =============================================================================
# SUMMARY REPORT
# =============================================================================
Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "                TASK 18 REGRESSION TEST SUMMARY                   " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$totalCount = $testResults.Count

Write-Host ("Total Tests Executed : " + $totalCount) -ForegroundColor White
Write-Host ("Passed               : " + $passCount) -ForegroundColor Green
$failColor = if ($failCount -gt 0) { "Red" } else { "Green" }
Write-Host ("Failed               : " + $failCount) -ForegroundColor $failColor

if ($startedProcess) {
    Write-Host "[INFO] Stopping started test server..." -ForegroundColor Gray
    Stop-Process -Id $startedProcess.Id -Force -ErrorAction SilentlyContinue
}

if ($failCount -eq 0) {
    Write-Host ">>> ALL REGRESSION AND END-TO-END TESTS PASSED SUCCESSFULLY! <<<" -ForegroundColor Green
    exit 0
} else {
    Write-Host ">>> REGRESSION FAILURES DETECTED! REVIEW LOG ABOVE. <<<" -ForegroundColor Red
    exit 1
}
