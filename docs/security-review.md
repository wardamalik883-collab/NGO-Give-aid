# Give-AID NGO — Security, Validation, and Error Handling Review

**Document Version:** 1.0  
**Date:** 2026-10-16  
**Status:** Completed  
**Associated Task:** TASK-17 — Validation, Security and Error Handling  

---

## 1. Executive Summary

This security review documents the systematic audit, vulnerability assessment, and hardening measures implemented across the **Give-AID NGO** ASP.NET Core MVC web application. The audit verified server-side input validation, antiforgery protections, role-based authorization, user data isolation, file upload restrictions, HTTP security headers, and user-friendly error handling.

---

## 2. Server-Side Validation Audit

### 2.1 Principles Enforced
* **Zero Client-Trust:** While client-side unobtrusive validation is enabled for responsive user feedback, **all constraints are strictly re-evaluated on the server** via `ModelState.IsValid`.
* **Entity Existence Checks:** Submitted relational identifiers (e.g., `CauseId`, `ProgrammeId`, `NGOId`) are validated against active database entities before saving.
* **Normalization & Sanitization:** All text inputs are trimmed and bounded by explicit `[StringLength]` and regex attributes to prevent buffer overflow and excessive payload attacks.

### 2.2 Form Validation Matrix

| Area | Controller & Action | Validations Enforced | Server Action on Invalid State |
| :--- | :--- | :--- | :--- |
| **Authentication** | `AccountController.Register` | Email format, uniqueness, minimum password length (8), confirmation match. | Redisplays form with validation summary. |
| **Authentication** | `AccountController.Login` | Required email & password, lockout after 5 consecutive failures. | Redisplays form with generic error message to prevent account enumeration. |
| **Inquiries** | `ContactController.Index` | Required name, subject, message (max 2000 chars), valid email format, phone regex. | Redisplays form preserving valid input. |
| **Donations** | `DonateController.Index` | Active cause validation, numeric range ($1 – $10,000,000), valid donor email. | Reloads available causes dropdown; returns form. |
| **Demo Payment** | `DonateController.ProcessPayment` | Expiry format (`MM/YY`), numeric card length, demo CVV length, idempotency check. | Retains donation context, shows payment validation. |
| **Volunteering** | `ProgrammesController.ExpressInterest` | Active programme ID, allowed roles ("Volunteer", "Participant"), duplicate email submission check. | Re-populates programme context, displays error. |
| **NGO Management** | `AdminNgosController.Create/Edit` | Organization name, registration format, address, URL format, logo file constraints. | Retains inputs, repopulates existing logo preview. |
| **Partner Management** | `AdminPartnersController.Create/Edit`| Partner name, allowed tier classification, display order integer bounds. | Retains input data with validation feedback. |
| **Programme Management**| `AdminProgrammesController.Create/Edit`| Start date, title, location, valid status ("Scheduled", "Completed", "Cancelled"). | Repopulates form and dates. |
| **Gallery Uploads** | `AdminGalleryController.Upload/Edit` | Title length, mandatory alt text for accessibility, programme existence check, image file validation. | Repopulates programme dropdown and returns view. |
| **Query Replies** | `AdminQueriesController.Reply` | Non-empty admin reply, valid query ID match. | Preserves original query details for review. |

---

## 3. Cross-Site Request Forgery (CSRF) & Antiforgery Protection

### 3.1 Defense-in-Depth Implementation
* **Global Antiforgery Filter:** Registered `AutoValidateAntiforgeryTokenAttribute` globally in `Program.cs`:
  ```csharp
  builder.Services.AddControllersWithViews(options =>
  {
      options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
  });
  ```
  This guarantees that all state-changing HTTP requests (`POST`, `PUT`, `DELETE`, `PATCH`) are automatically verified for a valid CSRF token, even if an individual action attribute is omitted.
* **Explicit Action Decorators:** All `[HttpPost]` actions across public and admin controllers explicitly declare `[ValidateAntiForgeryToken]`.
* **GET Idempotency:** No state-changing mutations or deletions are executed via `GET` requests. Delete workflows follow a two-step pattern: `GET` displays a dedicated confirmation view, and `POST` executes the deletion.
* **Secure Cookies:** Antiforgery and Identity authentication cookies are configured with `HttpOnly = true`, `SameSite = SameSiteMode.Lax` or `Strict`, and `SecurePolicy = CookieSecurePolicy.Always`.

---

## 4. Authentication, Authorization & Role-Based Access Control (RBAC)

### 4.1 Role Segregation
* **Admin Role:** Strictly guarded with `[Authorize(Roles = "Admin")]`. Non-admin users attempting access are redirected to `/Account/AccessDenied` or `/Account/Login`.
* **User Role:** Assigned automatically to registered members. Regular users cannot self-assign or elevate privileges to `Admin`.
* **Guest Users:** Restricted to public browsing (`Causes`, `Programmes`, `Gallery`, `Contact`, `Help`).

### 4.2 Endpoint Protection Summary

```
Public Endpoints (Anonymous Allowed):
├── / (Home)
├── /Causes (Index, Details)
├── /Programmes (Index, Details)
├── /Gallery (Index, Details)
├── /Contact (Index [GET, POST])
├── /Help (Index)
├── /Partners (Index)
├── /Account/Login
├── /Account/Register
└── /Account/AccessDenied

Member Endpoints ([Authorize]):
├── /Donate (Index, Payment, History, Receipt)
└── /Programmes/MyInterests

Administrator Endpoints ([Authorize(Roles = "Admin")]):
├── /Admin (Dashboard Overview)
├── /Admin/Ngos (CRUD)
├── /Admin/Partners (CRUD)
├── /Admin/Programmes (CRUD)
├── /Admin/Gallery (CRUD & Upload)
├── /Admin/Queries (Review & Reply)
├── /Admin/Donations (Audit & Status Update)
└── /Admin/Interests (Screening, Approve & Reject)
```

---

## 5. User Data Isolation & IDOR Prevention

### 5.1 Insecure Direct Object Reference (IDOR) Mitigation
* **Server-Side Identity Claims:** User identifiers are extracted directly from the authenticated session context using `User.FindFirstValue(ClaimTypes.NameIdentifier)` or `UserManager.GetUserId(User)`.
* **Donation History:** In `DonateController.History()`, query strictly enforces `.Where(d => d.UserId == currentUserId)`. A user cannot view other users' donation ledgers.
* **Donation Receipts:** In `DonateController.Receipt(int id)`, an explicit ownership check compares the record's `UserId` against the current caller:
  ```csharp
  if (!isAdmin && donation.UserId != currentUserId)
  {
      _logger.LogWarning("Access Denied: User {UserId} attempted to view receipt {DonationId}", currentUserId, id);
      return Forbid();
  }
  ```
* **Volunteer Applications:** In `ProgrammesController.MyInterests()`, queries are filtered by `pi.UserId == currentUserId`. Submissions assign `UserId` strictly from claims.

---

## 6. Secure File Upload Architecture

File upload functionality is centralized in [`ImageUploadService.cs`](file:///d:/Give-AID%20NGO/GiveAid.Web/Services/ImageUploadService.cs) to prevent malicious file uploads, executable script execution, and path traversal:

1. **Size Restriction:** Configurable maximum size (5 MB for gallery and banners; 2 MB for logos).
2. **Extension Whitelisting:** Permitted extensions are strictly limited to `.jpg`, `.jpeg`, `.png`, and `.webp`. Executable files (`.exe`, `.dll`, `.aspx`, `.php`, `.sh`, `.bat`) are rejected immediately.
3. **Magic Byte Signature Inspection:** The service reads the binary header bytes of the stream to verify genuine image file formats:
   * **JPEG/JPG:** `0xFF, 0xD8, 0xFF`
   * **PNG:** `0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A`
   * **WebP:** `RIFF` header (`0x52, 0x49, 0x46, 0x46`)
4. **Unique Server-Side Filename:** Uploaded filenames are completely discarded. A cryptographically unique GUID filename is generated:
   ```csharp
   var safeFileName = $"{Guid.NewGuid():N}{extension}";
   ```
5. **Path Traversal Prevention:** Files are saved strictly inside `wwwroot/uploads/{subfolder}`. Deletion routines check that the path begins with `/uploads/` and contains no traversal sequences (`..` or `\`).

---

## 7. Error Handling & Information Leakage Prevention

### 7.1 Production Error Handling
* **Status Code Re-execution:** Configured `app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");` in `Program.cs`.
* **Friendly Error Views:** Custom error view (`Views/Shared/Error.cshtml`) provides status-specific messaging for `400 Bad Request`, `403 Forbidden`, `404 Not Found`, and `500 Server Error`.
* **Diagnostic Logging Without Exposure:** Technical stack traces and internal exception details are written to server-side logs via `ILogger`, while the user is shown a clean message and an opaque support tracking ID (`RequestId`).
* **Zero Secret Exposure:** Database connection strings, SMTP passwords, and API keys are not exposed in error views, HTML comments, or client scripts.

---

## 8. HTTP Security Headers

The following response headers are added via custom middleware in `Program.cs`:

```http
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

* **X-Content-Type-Options (`nosniff`):** Disables browser MIME sniffing, preventing non-script files from being interpreted as scripts.
* **X-Frame-Options (`SAMEORIGIN`):** Mitigates clickjacking attacks by ensuring the application cannot be framed by external origins.
* **X-XSS-Protection (`1; mode=block`):** Activates built-in browser reflection XSS filters.
* **Referrer-Policy (`strict-origin-when-cross-origin`):** Restricts leakage of internal URLs to third-party domains.

---

## 9. Automated Verification & Test Results

A full test suite was executed against the running application covering 35 functional and security checks:

```
[PASS] Public Routes (Home, Causes, Donate, Programmes, Gallery, Contact, Help, Partners, Login, Register)
[PASS] Security: Unauthenticated access to /Admin redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Ngos redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Partners redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Programmes redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Gallery redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Queries redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Donations redirecting to /Account/Login (302)
[PASS] Security: Unauthenticated access to /Admin/Interests redirecting to /Account/Login (302)
[PASS] Admin Authentication & Anti-Forgery Token Validation
[PASS] Admin Dashboard Overview & Live KPI Metrics
[PASS] Admin Management Endpoints accessible with Admin Session (200)
[PASS] Simulated Donation Workflow & User Donation History isolation
[PASS] Security Headers present (nosniff, SAMEORIGIN, X-XSS, Referrer-Policy)
[PASS] Friendly 404 Error page re-execution on invalid URL
[PASS] Server-Side Form Validation rejecting malformed submissions
----------------------------------------------------------------------
Summary: 35 Tests Executed, 35 Passed, 0 Failed.
```

---

## 10. Conclusion

The Give-AID NGO web platform complies with the core security requirements for ASP.NET Core applications. Input validation, antiforgery checks, role authorization, file upload security, and user-friendly error handling are in place.
