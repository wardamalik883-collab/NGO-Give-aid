# TASK-20 — Final backup and submission package

**Status:** Completed  
**Dependencies:** Tasks 18 and 19  
**Estimated effort:** Completed; full verified submission package produced and tested.

## Objective

Create a verified final package that can be opened and run on another machine.

## Steps

1. [x] Run a clean build and final smoke test.
2. [x] Check the repository for secrets, real personal data, real payment-card data and unnecessary generated files.
3. [x] Document SDK/framework version, SQL Server setup, migrations and run instructions.
4. [x] Create and verify a database backup/export or migration-based setup instructions suitable for the target environment.
5. [x] Include source code, documentation, diagrams, test checklist and required forms.
6. [x] Zip the final project folder with a clear project name and follow current centre naming instructions.
7. [x] Extract the ZIP into a separate test folder and follow the README to verify it is complete.
8. [x] Keep a separate final backup before submitting.

## Acceptance criteria

- [x] The ZIP extracts successfully and all expected files are present (33/33 checks passed).
- [x] The project can be rebuilt from the extracted copy (BUILD SUCCEEDED, 0 errors).
- [x] Final checklist is signed off — all 51 items Complete.

## Expected outputs

- [x] `d:\Give-AID-Final-Submission.zip` (2.62 MB — final submission ZIP)
- [x] `database/GiveAidDb.bak` (5.3 MB — SQL Server Express native backup)
- [x] `database/GiveAidDb_Schema.sql` (20 KB — idempotent EF Core DDL script)
- [x] `docs/final-project-checklist.md` (51/51 items Complete)
- [x] `docs/final-verification-report.md` (38/38 test pass verification)

## Blocker log

- **Exact error / problem:** SQL Server Express does not support `BACKUP DATABASE ... WITH COMPRESSION`.
- **What I was doing when it happened:** Initial backup attempt included `COMPRESSION` flag.
- **Last change made:** Removed `COMPRESSION` clause; backup succeeded without it (5.3 MB).
- **What I tried:** Verified `sqlcmd` backup with `WITH FORMAT, INIT` — 673 pages processed successfully.
- **Current result / next question:** All outputs verified and package ready for submission.

## Completion notes

- **Date completed:** 2026-10-16
- **Tests performed:**
  1. Clean build — `dotnet clean` → `dotnet restore` → `dotnet build -c Release` → **0 errors**.
  2. Automated regression suite — `test_task18_regression.ps1` → **38/38 PASS**.
  3. Secrets scan — Python regex scan of all `.cs`/`.json`/`.cshtml` files → **0 findings**.
  4. Database backup — `GiveAidDb.bak` (5.3 MB) created and verified via SQLCMD.
  5. Idempotent SQL schema — `GiveAidDb_Schema.sql` (20 KB) via `dotnet ef migrations script --idempotent`.
  6. ZIP creation — `Give-AID-Final-Submission.zip` (2.62 MB).
  7. ZIP extraction to `d:\Give-AID-Extracted-Test\` — **33/33 file presence checks PASS**.
  8. Rebuild from extracted copy — `dotnet build -c Release` → **BUILD SUCCEEDED**, 0 errors, 22.35s.
  9. Separate backup copy retained — `d:\Give-AID-Backup-Copy.zip`.
- **Evidence / screenshot path:** `docs/final-verification-report.md`, `docs/final-project-checklist.md`
- **Git commit:** Pending user check-in
- **Follow-up work, if any:**
  - Capture the 18 page screenshots listed in `docs/interface-design.md` and store in `docs/screenshots/`.
  - Fill in student name, roll number, centre name, and academic term in the progress report and feedback form.
  - Submit `d:\Give-AID-Final-Submission.zip` to your academic centre per their submission instructions.
