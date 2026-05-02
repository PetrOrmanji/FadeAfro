import { HashRouter, Navigate, Route, Routes } from 'react-router-dom'
import { hasRole, useAuth } from '../context/AuthContext'
import ClientPage from '../pages/ClientPage/ClientPage'
import SettingsPage from '../pages/SettingsPage/SettingsPage'
import SelectServicePage from '../pages/SelectServicePage/SelectServicePage'
import SelectDatePage from '../pages/SelectDatePage/SelectDatePage'
import MasterPage from '../pages/MasterPage'
import OwnerPage from '../pages/OwnerPage'

// Показываем пока идёт загрузка
const Loading = () => <div>Загрузка...</div>

// Куда редиректить в зависимости от ролей
const RootRedirect = () => {
  const { roles } = useAuth()

  if (hasRole(roles, 'Owner'))  return <Navigate to="/owner"  replace />
  if (hasRole(roles, 'Master')) return <Navigate to="/master" replace />
  return <Navigate to="/client" replace />
}

const AppRouter = () => {
  const { isLoading, isAuthenticated } = useAuth()

  if (isLoading) return <Loading />
  if (!isAuthenticated) return <div>Не удалось авторизоваться</div>

  return (
    <HashRouter>
      <Routes>
        <Route path="/" element={<RootRedirect />} />
        <Route path="/client" element={<ClientPage />} />
        <Route path="/client/settings" element={<SettingsPage />} />
        <Route path="/client/master/:masterProfileId/services" element={<SelectServicePage />} />
        <Route path="/client/master/:masterProfileId/date"     element={<SelectDatePage />} />
        <Route path="/master" element={<MasterPage />} />
        <Route path="/owner"  element={<OwnerPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </HashRouter>
  )
}

export default AppRouter
