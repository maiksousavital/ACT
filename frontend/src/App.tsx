import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './contexts/AuthContext'
import { BrandProvider } from './contexts/BrandContext'
import { ProtectedRoute } from './components/Auth/ProtectedRoute'
import { ErrorBoundary } from './components/Common/ErrorBoundary'
import { OfflineBanner } from './components/Common/OfflineBanner'
import { InstallPrompt } from './components/Common/InstallPrompt'
import { AppShell } from './components/Layout/AppShell'
import { LoginPage } from './pages/Login/LoginPage'
import { NotFoundPage } from './pages/NotFound/NotFoundPage'
import { DashboardPage } from './pages/Dashboard/DashboardPage'
import { ClientListPage } from './pages/Clients/ClientListPage'
import { ClientFormPage } from './pages/Clients/ClientFormPage'
import { ClientDetailPage } from './pages/Clients/ClientDetailPage'
import { TreatmentTypeListPage } from './pages/TreatmentTypes/TreatmentTypeListPage'
import { TreatmentTypeFormPage } from './pages/TreatmentTypes/TreatmentTypeFormPage'
import { TreatmentListPage } from './pages/Treatments/TreatmentListPage'
import { TreatmentFormPage } from './pages/Treatments/TreatmentFormPage'
import { FollowUpsPage } from './pages/FollowUps/FollowUpsPage'
import { BrandSettingsPage } from './pages/Settings/BrandSettingsPage'
import { CompanyListPage } from './pages/Admin/Companies/CompanyListPage'
import { CompanyFormPage } from './pages/Admin/Companies/CompanyFormPage'
import { UserListPage } from './pages/Admin/Users/UserListPage'
import { UserFormPage } from './pages/Admin/Users/UserFormPage'
import { AuditLogPage } from './pages/Admin/AuditLogs/AuditLogPage'
import { LoginHistoryPage } from './pages/Admin/AuditLogs/LoginHistoryPage'
import { RoleGuard } from './components/Auth/RoleGuard'

export default function App() {
  return (
    <ErrorBoundary>
    <BrowserRouter>
      <AuthProvider>
        <BrandProvider>
        <OfflineBanner />
        <Toaster position="top-right" />
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route
            element={
              <ProtectedRoute>
                <AppShell />
              </ProtectedRoute>
            }
          >
            <Route path="/" element={<DashboardPage />} />
            <Route path="/clients" element={<ClientListPage />} />
            <Route path="/clients/new" element={<ClientFormPage />} />
            <Route path="/clients/:id" element={<ClientDetailPage />} />
            <Route path="/clients/:id/edit" element={<ClientFormPage />} />
            <Route path="/treatment-types" element={<TreatmentTypeListPage />} />
            <Route path="/treatment-types/new" element={<TreatmentTypeFormPage />} />
            <Route path="/treatment-types/:id/edit" element={<TreatmentTypeFormPage />} />
            <Route path="/treatments" element={<TreatmentListPage />} />
            <Route path="/treatments/new" element={<TreatmentFormPage />} />
            <Route path="/treatments/:id/edit" element={<TreatmentFormPage />} />
            <Route path="/follow-ups" element={<FollowUpsPage />} />
            <Route path="/settings/branding" element={<BrandSettingsPage />} />
            <Route path="/admin/companies" element={<RoleGuard requiredRole="SuperAdmin"><CompanyListPage /></RoleGuard>} />
            <Route path="/admin/companies/new" element={<RoleGuard requiredRole="SuperAdmin"><CompanyFormPage /></RoleGuard>} />
            <Route path="/admin/companies/:id/edit" element={<RoleGuard requiredRole="SuperAdmin"><CompanyFormPage /></RoleGuard>} />
            <Route path="/admin/users" element={<RoleGuard requiredRole="Admin"><UserListPage /></RoleGuard>} />
            <Route path="/admin/users/new" element={<RoleGuard requiredRole="Admin"><UserFormPage /></RoleGuard>} />
            <Route path="/admin/users/:id/edit" element={<RoleGuard requiredRole="Admin"><UserFormPage /></RoleGuard>} />
            <Route path="/admin/audit-logs" element={<RoleGuard requiredRole="Admin"><AuditLogPage /></RoleGuard>} />
            <Route path="/admin/login-history" element={<RoleGuard requiredRole="Admin"><LoginHistoryPage /></RoleGuard>} />
            <Route path="*" element={<NotFoundPage />} />
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Routes>
        <InstallPrompt />
        </BrandProvider>
      </AuthProvider>
    </BrowserRouter>
    </ErrorBoundary>
  )
}

