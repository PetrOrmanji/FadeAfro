import { HashRouter, Navigate, Route, Routes } from 'react-router-dom'
import { hasRole, useAuth } from '../context/AuthContext'
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
import OwnerPage from '../pages/OwnerPage'

// Куда редиректить в зависимости от ролей
const RootRedirect = () => {
  const { roles } = useAuth()

  if (hasRole(roles, 'Owner'))  return <Navigate to="/owner"  replace />
  if (hasRole(roles, 'Master')) return <Navigate to="/master" replace />
  return <Navigate to="/client" replace />
}

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
        <Route path="/client" element={<ClientPage />} />
        <Route path="/client/settings" element={<SettingsPage />} />
        <Route path="/client/master/:masterProfileId/services" element={<SelectServicePage />} />
        <Route path="/client/master/:masterProfileId/date"     element={<SelectDatePage />} />
        <Route path="/client/master/:masterProfileId/time"     element={<SelectTimePage />} />
        <Route path="/client/master/:masterProfileId/confirm"  element={<ConfirmPage />} />
        <Route path="/client/booking-success" element={<BookingSuccessPage />} />
        <Route path="/error" element={<ErrorPage />} />
        <Route path="/client/appointments" element={<MyAppointmentsPage />} />
        <Route path="/client/notifications" element={<NotificationsPage />} />
        <Route path="/master" element={<MasterPage />} />
        <Route path="/owner"  element={<OwnerPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </HashRouter>
  )
}

export default AppRouter
