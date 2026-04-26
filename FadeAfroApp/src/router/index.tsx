import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '@/components/AppLayout'
import { ProtectedRoute } from './ProtectedRoute'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      // Client
      {
        index: true,
        lazy: () => import('@/pages/client/MastersPage').then(m => ({ Component: m.MastersPage })),
      },
      {
        path: 'master/:id',
        lazy: () => import('@/pages/client/MasterProfilePage').then(m => ({ Component: m.MasterProfilePage })),
      },
      {
        path: 'book/:masterProfileId',
        element: <ProtectedRoute roles={['Client', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/client/BookingPage').then(m => ({ Component: m.BookingPage })),
      },
      {
        path: 'my-appointments',
        element: <ProtectedRoute roles={['Client', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/client/MyAppointmentsPage').then(m => ({ Component: m.MyAppointmentsPage })),
      },

      // Master
      {
        path: 'master',
        element: <ProtectedRoute roles={['Master', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/master/MasterPage').then(m => ({ Component: m.MasterPage })),
      },
      {
        path: 'master-panel/appointments',
        element: <ProtectedRoute roles={['Master', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/master/MasterAppointmentsPage').then(m => ({ Component: m.MasterAppointmentsPage })),
      },
      {
        path: 'master-panel/schedule',
        element: <ProtectedRoute roles={['Master', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/master/SchedulePage').then(m => ({ Component: m.SchedulePage })),
      },
      {
        path: 'master-panel/unavailabilities',
        element: <ProtectedRoute roles={['Master', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/master/UnavailabilitiesPage').then(m => ({ Component: m.UnavailabilitiesPage })),
      },
      {
        path: 'master-panel/services',
        element: <ProtectedRoute roles={['Master', 'Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/master/ServicesPage').then(m => ({ Component: m.ServicesPage })),
      },

      // Owner
      {
        path: 'owner',
        element: <ProtectedRoute roles={['Owner']}><span /></ProtectedRoute>,
        lazy: () => import('@/pages/owner/OwnerPage').then(m => ({ Component: m.OwnerPage })),
      },
    ],
  },
])
