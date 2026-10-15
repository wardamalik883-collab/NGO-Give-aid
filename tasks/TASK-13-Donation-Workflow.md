# TASK-13 — Simulated donation workflow and history

**Status:** Done  
**Dependencies:** Tasks 07 and 09  
**Estimated effort:** Estimate after checking your pace; do not treat this as a deadline.

## Objective

Complete a safe, testable donation journey with a dummy payment outcome.

## Steps

1. [x] Create/confirm Donation model with user, cause, amount, reference, status and timestamps.
2. [x] Validate amount on the server and confirm the selected cause exists and is active.
3. [x] Implement a clearly labelled simulated payment step with deterministic success/failure outcomes for testing.
4. [x] Never store real card number, CVV, PIN or actual payment credentials; do not claim a real transaction occurred.
5. [x] Save donation result/status and a non-sensitive reference in SQL Server.
6. [x] Build a confirmation/receipt view and a history page limited to the signed-in user's own donations.
7. [x] Build an admin view for donation records and status.

## Acceptance criteria

- [x] Valid demo donations are recorded with the correct user and cause.
- [x] Invalid amounts and unavailable causes are rejected.
- [x] A user cannot view another user's donation history.
- [x] No sensitive card data is logged or stored.

## Expected outputs

- `Donation model and migration`
- `Donation service/controller`
- `History/receipt views`

## Blocker log

- **Exact error / problem:** None
- **What I was doing when it happened:** N/A
- **Last change made:** N/A
- **What I tried:** N/A
- **Current result / next question:** All acceptance criteria satisfied and verified via automated test suite.

## Completion notes

- **Date completed:** 2026-10-15
- **Tests performed:** 30 automated integration tests covering anonymous access redirection ([Authorize]), amount validations (zero, negative, max limit), cause validation, simulated payment step, deterministic success & failure execution, database record status & amount increments, receipt generation, cross-user privacy enforcement (HTTP 403), user history ledger with KPIs, admin RBAC protection, admin auditing & filtering, status adjustments, idempotency duplicate prevention, and zero card credentials verification. Result: 30 / 30 PASSED (100%).
- **Evidence / screenshot path:** `scratch/test-task13.ps1`, `docs/21-Simulated-Donation-Workflow-and-History.md`
- **Git commit:** Pending commit on branch main
- **Follow-up work, if any:** None

