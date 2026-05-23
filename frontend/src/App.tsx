import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './contexts/AuthContext'
import { ProtectedRoute } from './components/Auth/ProtectedRoute'
import { AppShell } from './components/Layout/AppShell'
import { LoginPage } from './pages/Login/LoginPage'
import { DashboardPage } from './pages/Dashboard/DashboardPage'
import { ClientListPage } from './pages/Clients/ClientListPage'
import { ClientFormPage } from './pages/Clients/ClientFormPage'
import { ClientDetailPage } from './pages/Clients/ClientDetailPage'
import { TreatmentTypeListPage } from './pages/TreatmentTypes/TreatmentTypeListPage'
import { TreatmentTypeFormPage } from './pages/TreatmentTypes/TreatmentTypeFormPage'
import { TreatmentListPage } from './pages/Treatments/TreatmentListPage'
import { TreatmentFormPage } from './pages/Treatments/TreatmentFormPage'
import { FollowUpsPage } from './pages/FollowUps/FollowUpsPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
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
            {/* Phase F6-F7 routes will be added here */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

