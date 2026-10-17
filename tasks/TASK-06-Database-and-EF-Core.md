# TASK-06 — SQL Server and Entity Framework Core

**Status:** Completed  
**Dependencies:** Task 04  
**Estimated effort:** Completed.

## Objective

Connect the app to SQL Server using a maintainable data-access configuration.

## Steps

1. [x] Choose the SQL Server instance/database name and record setup steps without recording secrets.
2. [x] Add the EF Core SQL Server provider and design package versions compatible with the selected .NET target.
3. [x] Create `Data/ApplicationDbContext.cs` and register the context in `Program.cs`.
4. [x] Configure the connection string through development configuration or user secrets; keep production secrets out of source control.
5. [x] Plan initial entities and relationships from the requirements before making migrations.
6. [x] Add the first migration and apply it to the development database.
7. [x] Confirm the database exists and the migration can be applied from a clean setup.

## Acceptance criteria

- [x] The application starts with a valid database connection.
- [x] EF Core migration applies successfully.
- [x] Connection secrets are not committed.
- [x] Entity/table design is recorded before adding all feature tables.

## Expected outputs

- `Data/ApplicationDbContext.cs`
- `docs/database-design.md`
- `EF Core migration files`

## Blocker log

- **Exact error / problem:** None.
- **What I was doing when it happened:** N/A.
- **Last change made:** Configured SQL Server connection string, applied migrations, and verified all 18 tables in SQL Server.
- **What I tried:** `sqlcmd` table query and `dotnet ef migrations list`.
- **Current result / next question:** All migrations applied and working.

## Completion notes

- **Date completed:** 2026-10-15
- **Tests performed:** Database tables verification, migration history check, and end-to-end integration tests.
- **Evidence / screenshot path:** `Data/ApplicationDbContext.cs`, `Migrations/`.
- **Git commit:** Pending user git commit.
- **Follow-up work, if any:** Proceed to next tasks.
