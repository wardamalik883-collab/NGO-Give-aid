# Give-AID NGO — GUI Standards & Design System Documentation

**Document ID:** `GUI-01`  
**CSS Sources:** [`wwwroot/css/site.css`](file:///d:/Give-AID%20NGO/GiveAid.Web/wwwroot/css/site.css) & [`wwwroot/css/admin.css`](file:///d:/Give-AID%20NGO/GiveAid.Web/wwwroot/css/admin.css)  
**Framework:** Bootstrap 5.3 + Custom CSS Variables  
**Date:** October 2026  

---

## 1. Visual Design Philosophy & Brand Identity

The visual identity of **Give-AID** is designed around a modern, humanitarian color theme. It conveys institutional integrity, clinical reliability, and warmth. The design system uses CSS custom properties (variables) defined at the root level to ensure uniformity across public and administrative portals.

---

## 2. Color Palette & Semantic Tokens

### 2.1 Primary Brand Palette

| CSS Variable | Hex Code | Visual Sample | Semantic Purpose |
| :--- | :--- | :--- | :--- |
| `--giveaid-primary` | `#0f766e` | Deep Teal | Primary brand actions, header badges, active navbar items. |
| `--giveaid-primary-dark` | `#115e59` | Dark Pine Teal | Hover and active states for primary buttons. |
| `--giveaid-primary-light`| `#f0fdfa` | Pale Mint | Section background tints, active sidebar selections. |
| `--giveaid-primary-subtle`| `#ccfbf1`| Soft Cyan Tint | Category pills, light accent borders, tag backgrounds. |
| `--giveaid-secondary` | `#1e293b` | Dark Slate | Top bars, footer backgrounds, admin sidebar navigation. |
| `--giveaid-accent` | `#e11d48` | Warm Crimson | Urgent calls-to-action, donate highlights, danger badges. |
| `--giveaid-accent-dark` | `#be123c` | Deep Rose | Active/hover state for accent buttons. |
| `--giveaid-accent-light` | `#fff1f2` | Soft Blush | Error banners, high-priority alert backgrounds. |

### 2.2 Neutrals, Surfaces & Typography Colors

| CSS Variable | Hex Code | Semantic Role |
| :--- | :--- | :--- |
| `--giveaid-bg` | `#f8fafc` | Application canvas / body background color. |
| `--giveaid-surface` | `#ffffff` | Card surfaces, modals, elevated white panels. |
| `--giveaid-border` | `#e2e8f0` | Standard component borders, card dividers. |
| `--giveaid-text-main` | `#0f172a` | High-contrast headings and primary paragraph text. |
| `--giveaid-text-muted` | `#475569` | Secondary captions, subheadings, labels. |
| `--giveaid-text-light` | `#64748b` | Tertiary metadata, timestamps, card footer notes. |

---

## 3. Typography Hierarchy

The platform utilizes a modern system font stack that renders with native clarity across Windows, macOS, Android, and iOS without external font latency:

```css
font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
```

| Element | CSS Size | Weight | Line Height | Usage |
| :--- | :--- | :--- | :--- | :--- |
| **H1** | `2.25rem` (36px) | 800 (Bold) | 1.2 | Page main titles, hero banners |
| **H2** | `1.75rem` (28px) | 700 (Bold) | 1.3 | Major section headers, card group titles |
| **H3** | `1.35rem` (22px) | 600 (Semibold)| 1.4 | Module headings, card titles, dashboard widgets |
| **H4 / H5**| `1.15rem` (18px) | 600 (Semibold)| 1.4 | Subheadings, modal titles, table headers |
| **Body** | `1.00rem` (16px) | 400 (Regular) | 1.6 | Standard descriptive copy, article text |
| **Small / Meta**| `0.875rem` (14px)| 500 (Medium) | 1.5 | Timestamps, author metadata, form hints |

---

## 4. UI Components & Patterns

### 4.1 Buttons

- **Primary Brand Button (`.btn-giveaid-primary`):**
  - Background: `var(--giveaid-primary)` (`#0f766e`)
  - Text: `#ffffff`
  - Hover: `var(--giveaid-primary-dark)` (`#115e59`) with subtle elevation.
- **Outline Button (`.btn-giveaid-outline`):**
  - Border: 1.5px solid `var(--giveaid-primary)`
  - Text: `var(--giveaid-primary)`
  - Hover: Background fills with `var(--giveaid-primary)` and text turns white.
- **Accent / Donate Button (`.btn-giveaid-accent`):**
  - Background: `var(--giveaid-accent)` (`#e11d48`)
  - Text: `#ffffff`
  - Purpose: High-visibility calls-to-action for immediate contributions.

### 4.2 Cards (`.giveaid-card`)
- Elevated white surface (`background: #ffffff`) with rounded corners (`border-radius: 10px` / `var(--giveaid-radius-md)`).
- Border: `1px solid var(--giveaid-border)` (`#e2e8f0`).
- Shadow: `0 4px 12px rgba(15, 23, 42, 0.08)`.
- Interactive hover transition: Lifts 2px vertically with a smooth transition (`all 0.2s ease-in-out`).

### 4.3 Forms & Inputs
- **Form Controls:** Clean input borders (`#e2e8f0`), rounded corners (`6px`), with teal focus ring (`box-shadow: 0 0 0 0.25rem rgba(15, 118, 110, 0.25)`).
- **Floating Labels:** Applied on authentication and contact forms for enhanced readability.
- **Validation Messages:** Red indicator text (`.text-danger.small`) positioned immediately below invalid inputs.
- **Validation Summaries:** Rendered in alert containers (`.alert.alert-danger`) at the top of forms.

### 4.4 Tables & Lists
- Styled using `.table.table-hover.align-middle`.
- Headers: Clean light background (`.table-light`), uppercase 12px medium font for column titles.
- Action Buttons: Grouped right-aligned icon buttons (`Details`, `Edit`, `Delete`) with distinct color themes.
- Status Badges: Soft background pills:
  - Active / Completed / Approved: `.badge.bg-success-subtle.text-success`
  - Inactive / Closed / Rejected: `.badge.bg-secondary-subtle.text-secondary`
  - Scheduled / In Progress / Pending: `.badge.bg-warning-subtle.text-warning`

---

## 5. Responsive Layout Breakpoints

The responsive grid follows standard Bootstrap 5 container specifications:

| Breakpoint | Window Width | Layout Behavior |
| :--- | :--- | :--- |
| **Mobile (`xs`)** | `< 576px` | Single-column stack, full-width cards, collapsed off-canvas mobile navbar. |
| **Tablet (`sm`/`md`)**| `576px – 991px` | Two-column cards, condensed tables, hamburger navigation. |
| **Desktop (`lg`)** | `992px – 1199px`| Full desktop navbar, 3-column card grids, visible admin sidebar. |
| **Wide Desktop (`xl`/`xxl`)| `1200px+` | Max container width 1320px, 4-column widgets, expanded data tables. |

---

## 6. Accessibility Standards (WCAG 2.1 AA)

1. **Color Contrast:** Deep Teal (`#0f766e`) on white achieves a contrast ratio of **7.2:1**, well exceeding the WCAG AA requirement of 4.5:1.
2. **Keyboard Navigation:** Explicit `:focus-visible` styling provides an accessible outline for keyboard tab navigation.
3. **Skip-to-Content Link:** A visually hidden link (`.skip-link`) at the top of every page becomes visible on keyboard focus, allowing screen reader users to skip header navigation directly to `<main id="main-content">`.
4. **Mandatory Image Alt Text:** Gallery uploads enforce non-empty `AltText` in model validation.
5. **ARIA Attributes:** Form errors, alert banners, and accordion panels include standard ARIA attributes (`role="alert"`, `aria-expanded`, `aria-describedby`).
