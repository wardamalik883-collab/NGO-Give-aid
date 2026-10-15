# TASK-12 — Help Centre, Contact and queries

**Status:** Done  
**Dependencies:** Tasks 06 and 07  
**Estimated effort:** Estimate after checking your pace; do not treat this as a deadline.

## Objective

Let visitors submit questions and let the admin review and answer them.

## Steps

1. [x] Build Help Centre/FAQ and Contact Us pages.
2. [x] Create a ContactQuery model with message, contact details, created date, status and optional user relationship.
3. [x] Validate and save submitted queries to SQL Server.
4. [x] Build an admin-only query list/detail screen and reply/status workflow.
5. [x] Encode output safely and validate length/required fields.
6. [x] If email replies are implemented, use the shared email service and configuration; never expose credentials.

## Acceptance criteria

- [x] Valid queries persist and appear in admin.
- [x] Invalid submissions are rejected with clear messages.
- [x] Only authorized admin users can read/manage all queries.

## Expected outputs

- `ContactQuery model and views`
- `Migration`
- `Help Centre/FAQ view`

## Blocker log

- **Exact error / problem:** None
- **What I was doing when it happened:** N/A
- **Last change made:** N/A
- **What I tried:** N/A
- **Current result / next question:** All requirements implemented and verified.

## Completion notes

- **Date completed:** 2026-10-15
- **Tests performed:** 30 automated integration tests covering Help Centre FAQ accordion, Contact form validation, anonymous & authenticated submissions, server-side UserId binding, admin role authorization ([Authorize(Roles = "Admin")]), query listing, KPI filters, detailed inspection, status updating, email reply simulation, and safe deletion. All 30 tests passed (100%).
- **Evidence / screenshot path:** `scratch/test-task12.ps1`, `docs/20-Help-Centre-Contact-and-Queries.md`
- **Git commit:** Pending commit on branch main
- **Follow-up work, if any:** None

