# Give-AID — Requirements Tracking Checklist

**Document ID:** `DOC-09`  
**Task ID:** `TASK-01` — Requirements Analysis and Project Scope  
**Status:** Baseline Established  
**Date:** October 2026  

---

## 1. Grouped Requirements Checklist

This checklist tracks the implementation status of all confirmed requirements across the project lifecycle.

### 1.1 Public Website Module
- [ ] `FR-001` Hero banner with primary "Donate Now" CTA
- [ ] `FR-002` Featured causes showcase section
- [ ] `FR-003` Upcoming welfare programmes highlight
- [ ] `FR-004` Organization impact metrics display
- [ ] `FR-005` Partner and sponsor logos display
- [ ] `FR-006` Shared responsive layout, navbar, and footer
- [ ] `FR-007` About Us: What We Do section
- [ ] `FR-008` About Us: Our Mission & Vision section
- [ ] `FR-009` About Us: Our Team section
- [ ] `FR-010` About Us: Career With Us section
- [ ] `FR-011` About Us: Our Achievements section
- [ ] `FR-012` About Us: Our Supporters section
- [ ] `FR-013` About Us: Read About Us section
- [ ] `FR-014` About Us: Admin content manageability
- [ ] `FR-015` Public causes catalog
- [ ] `FR-016` Cause detail view with embedded donation trigger
- [ ] `FR-017` Empty state handling for causes
- [x] `FR-033` Public NGO & Partner showcase
- [x] `FR-034` Partner and NGO profile detail views
- [x] `FR-038` Public programmes catalog (Upcoming vs. Past)
- [x] `FR-039` Programme detail view
- [x] `FR-044` Public photo gallery grid
- [x] `FR-045` Programme-associated gallery image filtering
- [x] `FR-046` Image modal/lightbox inspection
- [x] `FR-049` Frequently Asked Questions (FAQ) accordion
- [x] `FR-050` Contact Us inquiry form
- [x] `FR-051` Server-side validation for contact submissions

### 1.2 User Registration & Member Workflows
- [ ] `FR-020` Public donation initiation form
- [ ] `FR-021` Guest and authenticated donation support
- [ ] `FR-022` Deterministic simulated payment processing
- [ ] `FR-023` Zero sensitive payment card data storage
- [ ] `FR-024` Digital donation receipt generation
- [ ] `FR-025` Authenticated user personal donation history
- [ ] `FR-027` User account registration
- [ ] `FR-028` Secure user login via ASP.NET Core Identity
- [ ] `FR-029` User logout with session invalidation
- [ ] `FR-032` User profile management & password change
- [ ] `FR-040` Expression of interest / volunteer submission
- [ ] `FR-041` Prevention of duplicate programme interest submissions
- [x] `FR-054` Contact queries linked to logged-in user profile
- [ ] `FR-055` "Invite a Friend" referral submission
- [ ] `FR-056` Anti-abuse rate limiting on invitations

### 1.3 Administration Back-Office (`/Admin`)
- [ ] `FR-018` Admin CRUD for charitable causes
- [ ] `FR-019` Cause image upload sanitization & validation
- [ ] `FR-026` Admin donation search and transaction audit
- [ ] `FR-030` Role-based authorization enforcement (`Admin` vs `User`)
- [ ] `FR-031` Automated admin account and role seeding
- [x] `FR-035` Admin CRUD for affiliated NGOs
- [x] `FR-036` Admin CRUD for corporate partners
- [x] `FR-037` Upload sanitization for NGO/partner logos
- [x] `FR-042` Admin CRUD for welfare programmes
- [ ] `FR-043` Admin moderation of volunteer/participation requests
- [x] `FR-047` Admin gallery image upload with metadata
- [x] `FR-048` Admin gallery image deletion with physical file cleanup
- [x] `FR-052` Admin inquiry dashboard
- [x] `FR-053` Admin query status tracking and internal notes
- [ ] `FR-059` Protected admin layout and route authorization
- [ ] `FR-060` Live KPI counters from SQL Server entities
- [ ] `FR-061` Centralized admin navigation sidebar
- [ ] `FR-062` Recent activity feeds on dashboard
- [ ] `FR-063` Immediate 403 Forbidden on unauthorized admin access
- [ ] `FR-064` Confirmation dialogs on destructive actions
- [ ] `FR-065` Admin oversight of registered users

### 1.4 Non-Functional & Quality Standards
- [ ] `NFR-001` Identity PBKDF2 password hashing (no plaintext)
- [ ] `NFR-002` Antiforgery token validation on all POST actions
- [ ] `NFR-003` Server-side `[Authorize(Roles = "Admin")]` enforcement
- [ ] `NFR-004` Automatic Razor HTML encoding against XSS
- [ ] `NFR-005` Entity Framework Core parameterized queries against SQL Injection
- [ ] `NFR-006` Secure, HttpOnly cookie configuration
- [ ] `NFR-007` User Secrets / Environment variables for credentials
- [ ] `NFR-008` Zero storage of card PAN, CVV, or PINs
- [ ] `NFR-011` Server-side ViewModel validation attributes
- [ ] `NFR-013` Upload extension, MIME, and GUID filename sanitization
- [ ] `NFR-014` Responsive layout (mobile, tablet, desktop)
- [ ] `NFR-016` WCAG 2.1 AA contrast and image alt text
- [ ] `NFR-018` Foreign key relational constraints and data integrity
- [ ] `NFR-019` Asynchronous async/await database operations
- [ ] `NFR-020` Strict MVC architectural separation of concerns
- [ ] `NFR-022` User-friendly custom error pages (no stack traces in prod)
- [ ] `NFR-023` Custom 404 Not Found page
- [ ] `NFR-025` Documented manual test execution
