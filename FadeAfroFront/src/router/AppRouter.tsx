import { HashRouter, Navigate, Route, Routes } from 'react-router-dom'
import { hasRole, useAuth } from '../context/AuthContext'
import type { UserRole } from '../api/user'
import LoadingScreen from '../components/LoadingScreen/LoadingScreen'
import ClientPage from '../pages/ClientPage/ClientPage'
import SettingsPage from '../pages/SettingsPage/SettingsPage'
import SelectServicePage from '../pages/SelectServicePage/SelectServicePage'
import SelectDatePage from '../pages/SelectDatePage/SelectDatePage'
import SelectTimePage from '../pages/SelectTimePage/SelectTimePage'
import ConfirmPage from '../pages/ConfirmPage/ConfirmPage'
import BookingSuccessPage from '../pages/BookingSuccessPage/BookingSuccessPage'
import ErrorPage from '../pages/ErrorPage/ErrorPage'
import MyAppointmentsPage from '../pages/MyAppointmentsPage/MyAppointmentsPage'
import NotificationsPage from '../pages/NotificationsPage/NotificationsPage'
import MasterPage from '../pages/MasterPage/MasterPage'
import MasterSettingsPage from '../pages/MasterSettingsPage/MasterSettingsPage'
import MasterSchedulePage from '../pages/MasterSchedulePage/MasterSchedulePage'
import MasterUnavailabilityPage from '../pages/MasterUnavailabilityPage/MasterUnavailabilityPage'
import MasterAppointmentsPage from '../pages/MasterAppointmentsPage/MasterAppointmentsPage'
import MasterServicesPage from '../pages/MasterServicesPage/MasterServicesPage'
import MasterServiceFormPage from '../pages/MasterServiceFormPage/MasterServiceFormPage'
import OwnerPage from '../pages/OwnerPage/OwnerPage'
import OwnerUsersPage from '../pages/OwnerUsersPage/OwnerUsersPage'

// ── Редирект на домашнюю страницу по роли ─────────────────────────────────

const RootRedirect = () => {
  const { roles } = useAuth()
  if (hasRole(roles, 'Owner'))  return <Navigate to="/owner"  replace />
  if (hasRole(roles, 'Master')) return <Navigate to="/master" replace />
  return <Navigate to="/client" replace />
}

// ── Защита маршрутов по роли ───────────────────────────────────────────────

const RequireRole = ({ role, children }: { role: UserRole; children: React.ReactNode }) => {
  const { roles } = useAuth()
  if (!hasRole(roles, role)) return <Navigate to="/" replace />
  return <>{children}</>
}

// ── Роутер ─────────────────────────────────────────────────────────────────

const AppRouter = () => {
  const { isLoading, isAuthenticated } = useAuth()

  if (isLoading) return <LoadingScreen />
  if (!isAuthenticated) return (
    <HashRouter>
      <Routes>
        <Route path="*" element={<ErrorPage />} />
      </Routes>
    </HashRouter>
  )

  return (
    <HashRouter>
      <Routes>
        <Route path="/" element={<RootRedirect />} />

        {/* ── Клиент ── */}
        <Route path="/client" element={<RequireRole role="Client"><ClientPage /></RequireRole>} />
        <Route path="/client/settings" element={<RequireRole role="Client"><SettingsPage /></RequireRole>} />
        <Route path="/client/appointments" element={<RequireRole role="Client"><MyAppointmentsPage /></RequireRole>} />
        <Route path="/client/notifications" element={<RequireRole role="Client"><NotificationsPage /></RequireRole>} />
        <Route path="/client/master/:masterProfileId/services" element={<RequireRole role="Client"><SelectServicePage /></RequireRole>} />
        <Route path="/client/master/:masterProfileId/date"     element={<RequireRole role="Client"><SelectDatePage /></RequireRole>} />
        <Route path="/client/master/:masterProfileId/time"     element={<RequireRole role="Client"><SelectTimePage /></RequireRole>} />
        <Route path="/client/master/:masterProfileId/confirm"  element={<RequireRole role="Client"><ConfirmPage /></RequireRole>} />
        <Route path="/client/booking-success" element={<RequireRole role="Client"><BookingSuccessPage /></RequireRole>} />

        {/* ── Мастер ── */}
        <Route path="/master" element={<RequireRole role="Master"><MasterPage /></RequireRole>} />
        <Route path="/master/settings" element={<RequireRole role="Master"><MasterSettingsPage /></RequireRole>} />
        <Route path="/master/notifications" element={<RequireRole role="Master"><NotificationsPage /></RequireRole>} />
        <Route path="/master/schedule"      element={<RequireRole role="Master"><MasterSchedulePage /></RequireRole>} />
        <Route path="/master/unavailability" element={<RequireRole role="Master"><MasterUnavailabilityPage /></RequireRole>} />
        <Route path="/master/appointments"  element={<RequireRole role="Master"><MasterAppointmentsPage /></RequireRole>} />
        <Route path="/master/services"          element={<RequireRole role="Master"><MasterServicesPage /></RequireRole>} />
        <Route path="/master/services/add"     element={<RequireRole role="Master"><MasterServiceFormPage /></RequireRole>} />
        <Route path="/master/services/:id/edit" element={<RequireRole role="Master"><MasterServiceFormPage /></RequireRole>} />

        {/* ── Владелец ── */}
        <Route path="/owner" element={<RequireRole role="Owner"><OwnerPage /></RequireRole>} />
        <Route path="/owner/settings" element={<RequireRole role="Owner"><SettingsPage /></RequireRole>} />
        <Route path="/owner/users"    element={<RequireRole role="Owner"><OwnerUsersPage /></RequireRole>} />

        <Route path="/error" element={<ErrorPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </HashRouter>
  )
}

export default AppRouter
