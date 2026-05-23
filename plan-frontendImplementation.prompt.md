# ACT Frontend Implementation Plan (React PWA)

## Project Location

**Monorepo** — frontend lives at `ACT/frontend/` alongside the backend `src/` folder.

```
ACT/
├── src/              # .NET backend
├── frontend/         # React PWA (Vite + TypeScript + Bootstrap 5)
├── tests/
└── ACT.sln
```

---

## Tech Stack

- **Framework**: React 18+ with TypeScript
- **Router**: React Router v6
- **State**: React Context + `useReducer` (upgrade to Zustand if needed later)
- **HTTP**: Axios with interceptors (JWT token, refresh, error handling)
- **Styling**: React Bootstrap + Bootstrap 5 (familiar, responsive, component-based)
- **Forms**: React Hook Form + Zod validation
- **Tables**: React Bootstrap Table + custom Pagination component
- **Notifications**: React Hot Toast
- **PWA**: Vite PWA plugin (`vite-plugin-pwa`)
- **Build**: Vite

---

## Architecture

```
frontend/src/
├── styles/
│   └── theme.css           # Bootstrap CSS variable overrides (default palette)
├── api/                    # Axios instance, API service functions
│   ├── axiosInstance.ts    # Base config, JWT interceptor
│   ├── authApi.ts          # login, register
│   ├── clientApi.ts        # client CRUD
│   ├── treatmentApi.ts     # treatment CRUD
│   ├── treatmentTypeApi.ts # treatment type CRUD
│   ├── companyApi.ts       # company CRUD (SuperAdmin)
│   ├── userApi.ts          # user management
│   ├── brandApi.ts         # brand settings
│   └── auditApi.ts         # audit logs, login history
├── components/             # Reusable UI components
│   ├── Layout/             # AppShell, Sidebar, TopBar
│   ├── Table/              # PagedTable, Pagination
│   ├── Form/               # FormField, FormSelect, FormDatePicker
│   ├── Auth/               # ProtectedRoute, RoleGuard
│   └── Common/             # Button, Modal, ConfirmDialog, LoadingSpinner
├── contexts/               # React Context providers
│   ├── AuthContext.tsx      # user, token, login/logout, role
│   └── BrandContext.tsx     # company colours, logo, theme
├── hooks/                  # Custom hooks
│   ├── useAuth.ts
│   ├── useBrand.ts
│   ├── usePagination.ts
│   └── useApi.ts           # generic fetch with loading/error
├── pages/                  # Route-level pages
│   ├── Login/
│   ├── Dashboard/
│   ├── Clients/
│   ├── Treatments/
│   ├── TreatmentTypes/
│   ├── FollowUps/
│   ├── Settings/           # Brand settings for Admin
│   └── Admin/              # SuperAdmin pages
│       ├── Companies/
│       ├── Users/
│       └── AuditLogs/
├── types/                  # TypeScript interfaces (mirror backend DTOs)
│   ├── auth.ts
│   ├── client.ts
│   ├── treatment.ts
│   ├── company.ts
│   ├── user.ts
│   ├── brand.ts
│   ├── audit.ts
│   └── common.ts           # PagedResult<T>, etc.
├── utils/                  # Helpers
│   ├── token.ts            # JWT decode, storage, expiry check
│   └── formatters.ts       # date, phone, etc.
├── App.tsx                 # Routes + providers
├── main.tsx                # Entry point
└── vite.config.ts          # PWA config
```

---

## Implementation Phases

### Phase F1 — Project Setup & Auth (Login Page)

#### F1.1 Scaffold project
- [x] Create Vite + React + TypeScript project
- [x] Install dependencies: `react-router-dom`, `axios`, `react-bootstrap`, `bootstrap`, `react-hook-form`, `zod`
- [x] Import Bootstrap CSS in `main.tsx` (`import 'bootstrap/dist/css/bootstrap.min.css'`)
- [x] Configure Vite PWA plugin (`vite-plugin-pwa`)
- [x] Set up folder structure as above
- [x] Create `.env` with `VITE_API_URL=http://localhost:5105/api`

#### F1.2 Axios instance & JWT interceptor
- [x] Create `api/axiosInstance.ts` — base URL from env
- [x] Add request interceptor: attach `Authorization: Bearer <token>` from localStorage
- [x] Add response interceptor: on 401, redirect to login
- [x] Create `utils/token.ts` — `getToken()`, `setToken()`, `removeToken()`, `isTokenExpired()`

#### F1.3 Auth context & types
- [x] Create `types/auth.ts` — `LoginRequest`, `AuthResponse`, `User` (decoded from JWT)
- [x] Create `contexts/AuthContext.tsx` — stores user, token, role, companyId
- [x] Expose `login()`, `logout()`, `isAuthenticated`, `user`, `isSuperAdmin`, `isAdmin`
- [x] On app load, check localStorage for existing token, validate expiry

#### F1.4 Login page
- [x] Create `pages/Login/LoginPage.tsx`
- [x] Form: email + password (React Hook Form + Zod validation)
- [x] On submit: call `POST /api/auth/login`, store token, redirect to dashboard
- [x] Show error on invalid credentials
- [x] Responsive design (works on mobile/tablet)

#### F1.5 Protected routes & layout
- [x] Create `components/Auth/ProtectedRoute.tsx` — redirects to `/login` if not authenticated
- [x] Create `components/Auth/RoleGuard.tsx` — shows 403 if user lacks required role
- [x] Create `components/Layout/AppShell.tsx` — sidebar + top bar + main content area
- [x] Sidebar: show/hide menu items based on role (SuperAdmin sees Admin section)
- [x] Top bar: user email, role badge, logout button
- [x] Wire routes in `App.tsx`

---

### Phase F2 — Dashboard

#### F2.1 Dashboard page
- [x] Create `pages/Dashboard/DashboardPage.tsx`
- [x] Cards: total clients, total treatments, today's follow-ups, overdue follow-ups
- [x] Fetch data from: `GET /api/client/paged`, `GET /api/followups/today`, `GET /api/followups/due`
- [x] Quick action buttons: "Add Client", "Add Treatment"

---

### Phase F3 — Clients Module

#### F3.1 Client types & API
- [x] Create `types/client.ts` — `ClientDto`, `CreateClientRequest`, `PagedResult<ClientDto>`
- [x] Create `api/clientApi.ts` — `getClientsPaged()`, `getClientById()`, `createClient()`, `updateClient()`

#### F3.2 Client list page
- [x] Create `pages/Clients/ClientListPage.tsx`
- [x] Paginated table with columns: Name, Phone, Email, Status, Actions
- [x] Search/filter by name (client-side or query param)
- [x] "Add Client" button → opens form

#### F3.3 Client form (create/edit)
- [x] Create `pages/Clients/ClientFormPage.tsx` (or modal)
- [x] Fields: FirstName, LastName, Phone, Email, Notes
- [x] Validation with Zod
- [x] On submit: POST (create) or PUT (edit), then redirect to list

#### F3.4 Client detail page
- [x] Create `pages/Clients/ClientDetailPage.tsx`
- [x] Show client info + list of their treatments
- [x] "Edit" button, "Add Treatment" button

---

### Phase F4 — Treatment Types Module

#### F4.1 Treatment Type types & API
- [x] Create `types/treatmentType.ts`
- [x] Create `api/treatmentTypeApi.ts`

#### F4.2 Treatment Type list page
- [x] Paginated table: Name, Follow-Up Interval, Active status, Actions
- [x] "Add Treatment Type" button

#### F4.3 Treatment Type form (create/edit)
- [x] Fields: Name, FollowUpIntervalDays (dropdown from enum), IsActive (toggle)
- [x] Dropdown values fetched from `GET /api/treatmenttype/add-edit-metadata`

---

### Phase F5 — Treatments & Follow-Ups Module

#### F5.1 Treatment types & API
- [x] Create `types/treatment.ts`
- [x] Create `api/treatmentApi.ts`

#### F5.2 Treatment list page
- [x] Paginated table: Client Name, Treatment Type, Date, Next Follow-Up, Status, Actions
- [x] Filter by client (dropdown or search)

#### F5.3 Treatment form (create/edit)
- [x] Fields: Client (searchable dropdown), TreatmentType (dropdown), Date, Notes
- [x] NextFollowUpDate auto-calculated from TreatmentType interval

#### F5.4 Follow-ups page
- [x] Two tabs/sections: "Due Today" and "All Overdue"
- [x] Each row: Client name, phone, treatment type, due date, "Complete" button
- [x] "Complete" button → modal with follow-up notes → calls `POST /api/followups/{id}/complete`

---

### Phase F6 — Settings & Branding (Admin)

#### F6.1 Brand settings page
- [x] Create `pages/Settings/BrandSettingsPage.tsx`
- [x] Fields: PrimaryColor (color picker), SecondaryColor, AccentColor, Theme (dropdown: light/dark/custom), LogoUrl
- [x] Preview: show live preview of colour changes
- [x] On save: `PUT /api/brandsettings`

#### F6.2 Brand context integration
- [x] Create `contexts/BrandContext.tsx` — fetch brand settings on login
- [x] Apply colours as CSS variables on `<html>` root (override Bootstrap's `--bs-primary`, `--bs-secondary`, etc.)
- [x] Custom `brand.css` overrides Bootstrap theme colours using CSS variables
- [x] Logo displayed in sidebar/top bar

---

### Phase F7 — Admin Portal (SuperAdmin)

#### F7.1 Company management
- [ ] Create `pages/Admin/Companies/CompanyListPage.tsx` — paginated table
- [ ] Create `pages/Admin/Companies/CompanyFormPage.tsx` — create/edit company
- [ ] After creating company: prompt to create first Admin user

#### F7.2 User management
- [ ] Create `pages/Admin/Users/UserListPage.tsx` — filter by company
- [ ] Create `pages/Admin/Users/UserFormPage.tsx` — create user (email, password, role, company)
- [ ] Deactivate user button with confirmation

#### F7.3 Audit logs
- [ ] Create `pages/Admin/AuditLogs/AuditLogPage.tsx` — paginated table of audit logs
- [ ] Create `pages/Admin/AuditLogs/LoginHistoryPage.tsx` — paginated login history
- [ ] Filter by company (dropdown for SuperAdmin)

---

### Phase F8 — PWA Features

#### F8.1 PWA manifest & service worker
- [ ] Configure `vite-plugin-pwa` with app name, icons, theme colour
- [ ] Add `manifest.json` (name: "ACT", icons, background_color from brand)
- [ ] Service worker: cache static assets, offline fallback page
- [ ] "Install App" prompt on compatible browsers

#### F8.2 Offline support (basic)
- [ ] Cache last-fetched data in localStorage for read-only offline viewing
- [ ] Show "Offline" banner when no connection
- [ ] Queue mutations (create/update) and sync when back online (optional, v2)

---

### Phase F9 — Polish & Testing

- [ ] Loading states (skeleton/spinner on all data-fetching pages)
- [ ] Error boundaries (global error page)
- [ ] 404 page
- [ ] Responsive design audit (mobile, tablet, desktop)
- [ ] Accessibility basics (labels, keyboard nav, focus management)
- [ ] Form validation error messages (consistent UX)
- [ ] Confirm dialogs before destructive actions (delete, deactivate)
- [ ] Toast notifications on success/error (react-hot-toast)

---

## Route Map

```
/login                          — LoginPage (public)
/                               — Dashboard (redirect if not auth'd)
/clients                        — ClientListPage
/clients/new                    — ClientFormPage (create)
/clients/:id                    — ClientDetailPage
/clients/:id/edit               — ClientFormPage (edit)
/treatments                     — TreatmentListPage
/treatments/new                 — TreatmentFormPage
/treatments/:id/edit            — TreatmentFormPage
/treatment-types                — TreatmentTypeListPage
/treatment-types/new            — TreatmentTypeFormPage
/treatment-types/:id/edit       — TreatmentTypeFormPage
/follow-ups                     — FollowUpsPage
/settings/branding              — BrandSettingsPage (Admin+)
/admin/companies                — CompanyListPage (SuperAdmin)
/admin/companies/new            — CompanyFormPage (SuperAdmin)
/admin/companies/:id/edit       — CompanyFormPage (SuperAdmin)
/admin/users                    — UserListPage (Admin+)
/admin/users/new                — UserFormPage (Admin+)
/admin/audit-logs               — AuditLogPage (Admin+)
/admin/login-history            — LoginHistoryPage (Admin+)
```

---

## Key Decisions

| Topic | Decision |
|-------|----------|
| **State management** | React Context + useReducer (simple, no extra dependency) |
| **Styling** | React Bootstrap + Bootstrap 5 (component-based, responsive, easy branding via CSS vars) |
| **No inline CSS** | Never use `style={}` props — Bootstrap utilities + CSS Modules (`.module.css`) only |
| **Responsive** | Mobile-first, Bootstrap breakpoints, tested at 320px / 768px / 1200px+ |
| **Forms** | React Hook Form + Zod (type-safe, performant) |
| **Tables** | React Bootstrap Table + custom Pagination |
| **PWA** | Vite PWA plugin (service worker, manifest, install prompt) |
| **Mobile** | PWA (not React Native) — one codebase, installable, works offline |
| **Auth storage** | JWT in localStorage (simple; upgrade to httpOnly cookie if needed) |
| **Brand theming** | Override Bootstrap CSS variables (`--bs-primary`, etc.) on `:root` |
| **API types** | Generated from OpenAPI spec (`openapi-typescript`) to stay in sync with backend |

---

## Styling Rules

1. **No inline styles** — never use `style={}` in JSX. All styling through Bootstrap utility classes or CSS Modules.
2. **Bootstrap utilities** — for layout, spacing, typography, and responsive visibility (`d-none d-md-block`, etc.).
3. **CSS Modules** — for component-specific styles, co-located as `ComponentName.module.css`.
4. **Responsive** — mobile-first approach. Sidebar collapses to off-canvas hamburger on < 992px. Tables use `table-responsive` or card layout on mobile. Forms full-width on mobile, constrained on desktop.

---

## Default Color Palette

Defined in `frontend/src/styles/theme.css` (imported in `main.tsx` after Bootstrap CSS):

```css
:root {
  --bs-primary: #6366F1;       /* Indigo 500 — vibrant purple-blue */
  --bs-secondary: #06B6D4;    /* Cyan 500 — fresh teal */
  --bs-success: #10B981;      /* Emerald 500 */
  --bs-danger: #EF4444;       /* Red 500 */
  --bs-warning: #F59E0B;      /* Amber 500 */
  --bs-info: #8B5CF6;         /* Violet 500 */
  --bs-light: #F8FAFC;        /* Slate 50 */
  --bs-dark: #1E293B;         /* Slate 800 */
}
```

**Theming logic:**
- The vibrant palette is the **default/unauthenticated look** (login page, public pages, companies without BrandSettings).
- Companies with BrandSettings override these variables via JS on `document.documentElement` **after login**.

---

## Responsive Design Requirements

- **Mobile-first**: all components designed for 320px first, then enhanced for larger screens.
- **Breakpoints**: Bootstrap 5 — sm: 576px, md: 768px, lg: 992px, xl: 1200px, xxl: 1400px.
- **Navigation**: sidebar collapses to off-canvas hamburger on screens < 992px.
- **Tables**: `table-responsive` wrapper; consider card-based layout on mobile for complex data.
- **Forms**: full-width on mobile, `max-width: 600px` on desktop.
- **Testing**: every page must be verified at mobile (320px), tablet (768px), and desktop (1200px+).
- **Fully responsive**: the entire application must work seamlessly across all device sizes — phone, tablet, laptop, and desktop.

---

## Implementation Order Summary

| Phase | What | Depends on |
|-------|------|-----------|
| **F1** | Project setup, auth, login, layout | Backend Phases 1-3 |
| **F2** | Dashboard | F1 |
| **F3** | Clients CRUD | F1 |
| **F4** | Treatment Types CRUD | F1 |
| **F5** | Treatments + Follow-ups | F3, F4 |
| **F6** | Branding / Settings | F1, Backend Phase 4 |
| **F7** | Admin Portal (companies, users, audit) | F1, Backend Phases 2,5,6 |
| **F8** | PWA features | F1-F7 done |
| **F9** | Polish & testing | All phases |

---

**Instructions:**
- Execute phases F1 → F9 in order.
- Each phase produces a working, testable increment.
- Mark items `[x]` as completed.






