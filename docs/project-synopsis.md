# Give-AID NGO Web Portal — Project Synopsis

**Academic Year:** 2026  
**Project Title:** Give-AID Humanitarian NGO Web Platform  
**Target Domain:** Non-Governmental Organizations & Community Welfare Management  
**Implementation Technology:** ASP.NET Core MVC (.NET 10), C#, Entity Framework Core, ASP.NET Core Identity, SQL Server Express  
**Document Classification:** Academic eProject Software Documentation  

---

## 1. Project Title
**Give-AID — Integrated Non-Governmental Organization Web Management & Humanitarian Outreach Portal**

---

## 2. Introduction
Non-governmental organizations (NGOs) and humanitarian charities play an essential role in addressing socioeconomic disparities, disaster relief, poverty alleviation, healthcare outreach, and community empowerment. However, many grassroots non-profit organizations struggle with fragmented workflows, reliance on spreadsheets, opaque donation records, and limited public visibility.

**Give-AID** is an enterprise-grade, web-based platform developed using ASP.NET Core MVC that centralizes charitable campaigns, affiliated NGO directories, corporate sponsorship rosters, field welfare programmes, volunteer recruitment, public photo galleries, community inquiries, and demo donation workflows under a single secure, unified architecture.

---

## 3. Background
Historically, smaller and mid-sized charitable foundations have relied on disparate digital tools: social media channels for campaign announcements, third-party spreadsheets for financial records, and manual email correspondence for volunteer recruitment. This fragmentation introduces several operational risks:
- Lack of centralized audit trails for charitable contributions.
- Risk of donor privacy breaches and data leakage.
- Absence of role-based administration for field coordinators and management.
- Inconsistent branding and communication channels.
- Difficulty in providing transparent impact reporting to stakeholders.

Give-AID was conceived to address these shortcomings by offering an accessible, standards-compliant web system designed specifically for the operational needs of humanitarian initiatives in developing and transitional regions.

---

## 4. Problem Definition
The manual and disjointed administration of non-profit operations results in:
1. **Inefficient Campaign Management:** Inability to dynamically publish, update, and monitor fundraising targets and collected amounts.
2. **Opaque Volunteer & Participant Intake:** Unstructured volunteer applications leading to delayed follow-ups and uncoordinated field events.
3. **Insecure Donor Privacy:** Lack of authenticated access controls exposing transaction records and personal donor contact information.
4. **Administrative Overhead:** Disjointed communication channels where public queries, volunteer applications, and partner inquiries are scattered across personal inboxes.
5. **Vulnerability to Unauthorized Modification:** Unprotected administrative backends without strict Role-Based Access Control (RBAC).

---

## 5. Proposed Solution
The Give-AID platform provides a monolithic, secure web application engineered with ASP.NET Core MVC adhering to the Model-View-Controller design pattern. Key elements of the solution include:
- **Centralized Database:** Managed via Entity Framework Core with SQL Server Express, enforcing relational constraints, decimal precision for monetary audit trails, and cascading behaviors.
- **Strict Role-Based Access Control:** ASP.NET Core Identity configuring distinct `Admin` and `User` roles with server-side claim evaluation preventing privilege escalation.
- **Dedicated Administrative Dashboard:** Single consolidated administrative interface featuring operational KPI metrics and full CRUD management for NGOs, partners, causes, programmes, and galleries.
- **Controlled Demo Donation Simulation:** Interactive, educational donation simulation that validates financial thresholds, tracks campaign funding progress, issues formatted digital receipts, and strictly forbids storage of sensitive payment credentials (e.g., credit card numbers, CVVs).
- **Public Engagement Workflows:** Direct volunteer/participant applications for community programmes, an authenticated friend referral invitation system, and a structured contact query resolution workflow with administrative response capabilities.

---

## 6. Project Objectives
1. **Develop a Scalable Web Architecture:** Build a modular ASP.NET Core MVC application supporting clear separation of concerns among models, views, controllers, and services.
2. **Implement Robust Role-Based Security:** Protect administrative modules with `[Authorize(Roles = "Admin")]` and protect user financial history with strict claim-based ownership checks.
3. **Deliver Transparent Cause & Programme Management:** Allow authorized administrators to schedule field camps, publish fundraising causes, and monitor progress bars dynamically.
4. **Provide a Safe, Educational Donation Experience:** Demonstrate financial intake workflows using deterministic simulation while maintaining an immutable audit log and generating donor receipts.
5. **Facilitate Community Interaction:** Enable public visitors to express volunteer interest, submit inquiries, access an accordion FAQ centre, and allow registered members to invite friends via email.
6. **Ensure High Standards of Web Security:** Integrate Anti-Forgery Token validation, security headers middleware, input validation, and defensive file upload pipelines.

---

## 7. Project Scope
### Within Scope (Implemented & Verified):
- Public responsive website: Home, About Us (7 manageable sections), Active Causes, Partner directory, Welfare Programmes, Photo Gallery, Help/FAQ Centre, Contact Us.
- Member authentication: Registration, login with lockout protection, logout, session persistence.
- Role management: Role-based authorization separating anonymous visitors, standard members, and administrators.
- Admin dashboard: Statistical summary cards (total causes, donations, members, NGOs, partners, programmes, gallery photos, queries, pending applications) and centralized navigation.
- CRUD management: Complete management of NGOs, Corporate Partners, Humanitarian Programmes, Gallery Assets, and Relief Causes.
- Simulated donation workflow: Range-validated contribution intake, deterministic outcome simulation, cause target progress updating, unique reference generation (`TXN-`), digital receipt rendering, donor personal donation history, and strict cross-user privacy enforcement.
- Programme participation & volunteer intake: Public application submission, duplicate submission prevention, user history tracking (`/Programmes/MyInterests`), and administrative review (Pending/Approved/Rejected).
- Contact queries: Visitor inquiry submission, user profile association, admin status filtering (New, In Progress, Answered, Closed), internal notes, and administrative official replies.
- Member invitations: Authenticated friend email invitations logged to the database.
- Defensive file upload pipeline: Extension and magic byte MIME signature validation for `.jpg`, `.jpeg`, `.png`, and `.webp` with size caps and GUID-based filename sanitization.

### Out of Scope (Deliberate Architectural Boundaries):
- Integration with live commercial payment gateways (e.g., Stripe, PayPal, JazzCash) — strictly simulated to prevent financial liability.
- Live external SMTP mail delivery with production email transport — email delivery is simulated and logged unless local/external SMTP credentials are configured.
- Multi-currency live foreign exchange conversion.
- Native mobile application clients (the responsive web interface satisfies mobile access requirements).

---

## 8. Target Users
1. **Public Visitors / Donors:** Browse campaigns, review field impact in galleries, search FAQ questions, submit general inquiries, and simulate charitable donations.
2. **Volunteers & Community Participants:** Apply to participate in scheduled field relief programmes, track application status, and view past engagements.
3. **Registered Members:** Maintain a personal profile, view authenticated donation receipts, monitor volunteer applications, and invite friends to visit Give-AID.
4. **NGO Administrators & Coordinators:** Manage affiliated NGOs, corporate partners, fundraising drives, welfare events, review volunteer requests, and reply to visitor inquiries.

---

## 9. Main Features
- **Public Hero & Impact Showcase:** Verified campaign progress bars, recent field activities, partner banners, and call-to-action buttons.
- **Dynamic Causes Catalog:** Campaign filtering, target vs. raised progress indicators, organizing NGO attribution, and direct donation buttons.
- **Welfare Programme Scheduling:** Chronological event grouping into upcoming scheduled drives and past completed initiatives with venue locations and coordinator contacts.
- **Accessible Photo Gallery:** Field photographic evidence categorized with mandatory WCAG AA-compliant alternative text (`AltText`).
- **Simulated Payment Gateway:** Multi-step donation workflow featuring preset contribution amounts (PKR 1,000 to PKR 25,000), custom amount inputs, deterministic success/failure toggles, idempotency token protection, and formatted digital receipts.
- **Interactive Help & FAQ Centre:** Categorized accordion questions covering general giving, tax deductions, volunteer requirements, and safety policies.
- **Unified Admin Portal:** Clean administrative layout isolated from public navigation, featuring sidebar shortcuts, metric widgets, and confirmation modals for delete actions.

---

## 10. Technologies Used
- **Server Framework:** ASP.NET Core MVC (.NET 10.0)
- **Programming Language:** C# 13
- **Object-Relational Mapper:** Entity Framework Core 10.0 (Code-First Migrations)
- **Database Engine:** Microsoft SQL Server Express (`.\SQLEXPRESS`)
- **Identity & Security:** ASP.NET Core Identity, BCrypt/PBKDF2 password hashing, Anti-Forgery Tokens, Custom Security Headers Middleware
- **Frontend Presentation:** Razor View Engine (`.cshtml`), Bootstrap 5.3, Custom CSS Variables Design System (`site.css`, `admin.css`)
- **Testing & Tooling:** PowerShell 5.1/7 Automated Regression Runner, `sqlcmd` Database Diagnostics

---

## 11. Expected Benefits
- **Operational Transparency:** Provides an immutable, traceable digital audit log of charitable contributions and community programmes.
- **Data Protection:** Eliminates the risk of exposing sensitive donor payment credentials by employing a strict simulation model.
- **Enhanced Coordination:** Centralizes volunteer recruitment and community inquiries, enabling staff to approve applications and record official replies in one location.
- **Consistent Visual Identity:** Delivers a cohesive, accessible humanitarian brand palette (Deep Teal `#0f766e`, Warm Crimson `#e11d48`, Neutral Slate `#1e293b`).

---

## 12. Project Limitations
- **Simulated Payment Gateway:** Real credit/debit card transactions are not executed; payment outcomes are deterministically generated for educational and evaluation purposes.
- **Email Delivery Fallback:** When external SMTP server credentials are not supplied in configuration or user secrets, outgoing email notifications (receipts, replies, invites) are safely simulated and logged to the application console.
- **Single Currency:** All financial figures are denominated in Pakistani Rupees (PKR).

---

## 13. Future Enhancements
- Integration with third-party verified payment aggregators (e.g., Stripe Checkout, 1Link, EasyPaisa) under production PCI-DSS compliance.
- Integration of a cloud-based transactional email delivery provider (e.g., SendGrid, AWS SES) with DKIM/SPF domain verification.
- Automated SMS alerts for field programme emergency updates and volunteer coordination.
- Export of financial summary reports and tax-exempt donor certificates in PDF format.

---

## 14. Conclusion
The Give-AID NGO web platform demonstrates the practical application of enterprise software engineering principles to the non-profit domain. By combining ASP.NET Core MVC, Entity Framework Core, SQL Server, and ASP.NET Core Identity, the application delivers a complete, secure, and user-centric solution that meets all specified functional and architectural requirements while upholding stringent standards of security, privacy, and data integrity.
