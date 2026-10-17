# TASK-18 — End-to-end testing and regression fixes

**Status:** Completed  
**Dependencies:** Tasks 08–17  
**Estimated effort:** Completed; comprehensive automated and manual test suite verified.

## Objective

Test the website as a visitor, user and admin before documentation and submission.

## Steps

1. [x] Build the solution and fix all build warnings/errors that can reasonably be resolved.
2. [x] Test public navigation, empty states, forms and responsive layout.
3. [x] Test register/login/logout and Admin/User authorization.
4. [x] Test CRUD for causes, NGOs, partners, programmes and gallery.
5. [x] Test donation success/failure simulation, history privacy and amount validation.
6. [x] Test contact query, admin reply, invitation handling and programme-interest submission.
7. [x] Test invalid inputs, invalid uploads, unauthenticated access and direct URL access to admin pages.
8. [x] Record results, expected outcomes, actual outcomes and bug fixes in a test checklist.

## Acceptance criteria

- [x] All critical workflows pass manually and via automated test suite.
- [x] No known blocker remains for core functionality.
- [x] Test evidence and known limitations are documented.

## Expected outputs

- [x] `docs/unit-testing-checklist.md`
- [x] `docs/bug-log.md`
- [x] `test_task18_regression.ps1` (Comprehensive 38-test automated regression runner)

## Blocker log

- **Exact error / problem:** None remaining.
- **What I was doing when it happened:** N/A.
- **Last change made:** Resolved process ID collision, Admin reply route alignment, dynamic programme lookup, and cross-user receipt security handling.
- **What I tried:** Verified with full 38-assertion regression run.
- **Current result / next question:** 38 / 38 tests passing (100% pass rate).

## Completion notes

- **Date completed:** 2026-10-16
- **Tests performed:** 38 end-to-end tests across 6 functional suites:
  1. Build & Server Diagnostics (3 tests)
  2. Public Page Rendering & Navigation (11 tests)
  3. Authentication & RBAC Authorization (6 tests)
  4. Administrative Module CRUD Operations for NGOs, Partners, Programmes (8 tests)
  5. Simulated Donation Workflow, Receipt Generation & Cross-User Privacy Guard (5 tests)
  6. Contact Queries, Admin Replies, Volunteer Interest, and Friend Invitations (5 tests)
- **Evidence / test log path:** `test_task18_regression.ps1`, `docs/unit-testing-checklist.md`, `docs/bug-log.md`
- **Git commit:** Pending user check-in
- **Follow-up work, if any:** Proceed to Task 19 (Final Documentation, Deliverables & Submission).
