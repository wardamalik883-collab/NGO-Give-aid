# TASK-17 — Validation, security and error handling

**Status:** Completed  
**Dependencies:** Tasks 07–16  
**Estimated effort:** Completed.

## Objective

Review common security and reliability risks across the application.

## Steps

1. [x] Use server-side validation for all state-changing forms; client-side validation is supplementary.
2. [x] Use antiforgery protection for form posts and follow MVC/Identity defaults.
3. [x] Verify authorization in controllers/actions, not just by hiding buttons.
4. [x] Review file uploads, generated filenames, file size/type limits and storage paths.
5. [x] Ensure users can only view/edit their own profile, donation history and submissions.
6. [x] Add friendly not-found and error pages; avoid showing stack traces or secrets to end users.
7. [x] Move secrets out of tracked configuration files.
8. [x] Check output encoding, query usage and deletion confirmations.
9. [x] Review accessibility basics: labels, focus, contrast, alt text and keyboard navigation.

## Acceptance criteria

- [x] Security checklist passes for public, user and admin routes.
- [x] No secrets or sensitive card information are stored in the repository/database/logs.
- [x] Errors are user-friendly and diagnostic details remain in development logs.

## Expected outputs

- `docs/security-review.md`: Detailed security architecture, validation audit matrix, and threat mitigation documentation.
- `Updated validation/error handling`: `Program.cs` configured with `AutoValidateAntiforgeryTokenAttribute`, HTTP security headers, and `UseStatusCodePagesWithReExecute`; `ErrorViewModel.cs` enhanced with status code support; `Error.cshtml` redesigned into a friendly branded error page.

## Blocker log

- **Exact error / problem:** None.
- **What I was doing when it happened:** N/A.
- **Last change made:** Configured security middleware, custom error handling, and test suite.
- **What I tried:** Ran end-to-end automated test suite covering 35 tests.
- **Current result / next question:** All 35 tests passing with 0 errors.

## Completion notes

- **Date completed:** 2026-10-16
- **Tests performed:** Automated integration and security test suite (`test_all_tasks.ps1`) executing 35 tests covering public pages, admin RBAC, anti-forgery, security headers, 404 re-execution, and server-side form validation.
- **Evidence / screenshot path:** `docs/security-review.md`, `Views/Shared/Error.cshtml`, `Program.cs`.
- **Git commit:** Pending user git commit.
- **Follow-up work, if any:** Proceed to Task 18 (Testing and Bug Fixes) or Task 15.
