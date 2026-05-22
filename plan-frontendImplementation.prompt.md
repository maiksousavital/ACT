# ACT Frontend Implementation Plan (React PWA)

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
src/
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
- [ ] Create Vite + React + TypeScript project
- [ ] Install dependencies: `react-router-dom`, `axios`, `react-bootstrap`, `bootstrap`, `react-hook-form`, `zod`
- [ ] Import Bootstrap CSS in `main.tsx` (`import 'bootstrap/dist/css/bootstrap.min.css'`)
- [ ] Configure Vite PWA plugin (`vite-plugin-pwa`)
- [ ] Set up folder structure as above
- [ ] Create `.env` with `VITE_API_URL=http://localhost:5105/api`

#### F1.2 Axios instance & JWT interceptor
- [ ] Create `api/axiosInstance.ts` — base URL from env
- [ ] Add request interceptor: attach `Authorization: Bearer <token>` from localStorage
- [ ] Add response interceptor: on 401, redirect to login
- [ ] Create `utils/token.ts` — `getToken()`, `setToken()`, `removeToken()`, `isTokenExpired()`

#### F1.3 Auth context & types
- [ ] Create `types/auth.ts` — `LoginRequest`, `AuthResponse`, `User` (decoded from JWT)
- [ ] Create `contexts/AuthContext.tsx` — stores user, token, role, companyId
- [ ] Expose `login()`, `logout()`, `isAuthenticated`, `user`, `isSuperAdmin`, `isAdmin`
- [ ] On app load, check localStorage for existing token, validate expiry

#### F1.4 Login page
- [ ] Create `pages/Login/LoginPage.tsx`
- [ ] Form: email + password (React Hook Form + Zod validation)
- [ ] On submit: call `POST /api/auth/login`, store token, redirect to dashboard
- [ ] Show error on invalid credentials
- [ ] Responsive design (works on mobile/tablet)

#### F1.5 Protected routes & layout
- [ ] Create `components/Auth/ProtectedRoute.tsx` — redirects to `/login` if not authenticated
- [ ] Create `components/Auth/RoleGuard.tsx` — shows 403 if user lacks required role
- [ ] Create `components/Layout/AppShell.tsx` — sidebar + top bar + main content area
- [ ] Sidebar: show/hide menu items based on role (SuperAdmin sees Admin section)
- [ ] Top bar: user email, role badge, logout button
- [ ] Wire routes in `App.tsx`

---

### Phase F2 — Dashboard

#### F2.1 Dashboard page
- [ ] Create `pages/Dashboard/DashboardPage.tsx`
- [ ] Cards: total clients, total treatments, today's follow-ups, overdue follow-ups
- [ ] Fetch data from: `GET /api/client/paged`, `GET /api/followups/today`, `GET /api/followups/due`
- [ ] Quick action buttons: "Add Client", "Add Treatment"

---

### Phase F3 — Clients Module

#### F3.1 Client types & API
- [ ] Create `types/client.ts` — `ClientDto`, `CreateClientRequest`, `PagedResult<ClientDto>`
- [ ] Create `api/clientApi.ts` — `getClientsPaged()`, `getClientById()`, `createClient()`, `updateClient()`

#### F3.2 Client list page
- [ ] Create `pages/Clients/ClientListPage.tsx`
- [ ] Paginated table with columns: Name, Phone, Email, Status, Actions
- [ ] Search/filter by name (client-side or query param)
- [ ] "Add Client" button → opens form

#### F3.3 Client form (create/edit)
- [ ] Create `pages/Clients/ClientFormPage.tsx` (or modal)
- [ ] Fields: FirstName, LastName, Phone, Email, Notes
- [ ] Validation with Zod
- [ ] On submit: POST (create) or PUT (edit), then redirect to list

#### F3.4 Client detail page
- [ ] Create `pages/Clients/ClientDetailPage.tsx`
- [ ] Show client info + list of their treatments
- [ ] "Edit" button, "Add Treatment" button

---

### Phase F4 — Treatment Types Module

#### F4.1 Treatment Type types & API
- [ ] Create `types/treatmentType.ts`
- [ ] Create `api/treatmentTypeApi.ts`

#### F4.2 Treatment Type list page
- [ ] Paginated table: Name, Follow-Up Interval, Active status, Actions
- [ ] "Add Treatment Type" button

#### F4.3 Treatment Type form (create/edit)
- [ ] Fields: Name, FollowUpIntervalDays (dropdown from enum), IsActive (toggle)
- [ ] Dropdown values fetched from `GET /api/treatmenttype/add-edit-metadata`

---

### Phase F5 — Treatments & Follow-Ups Module

#### F5.1 Treatment types & API
- [ ] Create `types/treatment.ts`
- [ ] Create `api/treatmentApi.ts`

#### F5.2 Treatment list page
- [ ] Paginated table: Client Name, Treatment Type, Date, Next Follow-Up, Status, Actions
- [ ] Filter by client (dropdown or search)

#### F5.3 Treatment form (create/edit)
- [ ] Fields: Client (searchable dropdown), TreatmentType (dropdown), Date, Notes
- [ ] NextFollowUpDate auto-calculated from TreatmentType interval

#### F5.4 Follow-ups page
- [ ] Two tabs/sections: "Due Today" and "All Overdue"
- [ ] Each row: Client name, phone, treatment type, due date, "Complete" button
- [ ] "Complete" button → modal with follow-up notes → calls `POST /api/followups/{id}/complete`

---

### Phase F6 — Settings & Branding (Admin)

#### F6.1 Brand settings page
- [ ] Create `pages/Settings/BrandSettingsPage.tsx`
- [ ] Fields: PrimaryColor (color picker), SecondaryColor, AccentColor, Theme (dropdown: light/dark/custom), LogoUrl
- [ ] Preview: show live preview of colour changes
- [ ] On save: `PUT /api/brandsettings`

#### F6.2 Brand context integration
- [ ] Create `contexts/BrandContext.tsx` — fetch brand settings on login
- [ ] Apply colours as CSS variables on `<html>` root (override Bootstrap's `--bs-primary`, `--bs-secondary`, etc.)
- [ ] Custom `brand.css` overrides Bootstrap theme colours using CSS variables
- [ ] Logo displayed in sidebar/top bar

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
| **Forms** | React Hook Form + Zod (type-safe, performant) |
| **Tables** | React Bootstrap Table + custom Pagination |
| **PWA** | Vite PWA plugin (service worker, manifest, install prompt) |
| **Mobile** | PWA (not React Native) — one codebase, installable, works offline |
| **Auth storage** | JWT in localStorage (simple; upgrade to httpOnly cookie if needed) |
| **Brand theming** | Override Bootstrap CSS variables (`--bs-primary`, etc.) on `:root` |

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






