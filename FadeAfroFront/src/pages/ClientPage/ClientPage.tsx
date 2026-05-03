import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAllMasters, type MasterProfile } from '../../api/masters'
import { getMe, type UserResponse } from '../../api/user'
import { getMyAppointments, type ClientAppointment } from '../../api/appointments'
import MasterCard from '../../components/MasterCard/MasterCard'
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import styles from './ClientPage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'января','февраля','марта','апреля','мая','июня',
  'июля','августа','сентября','октября','ноября','декабря',
]

const formatDateTime = (isoUtc: string) => {
  const d = new Date(isoUtc)
  const hh = String(d.getHours()).padStart(2, '0')
  const mm = String(d.getMinutes()).padStart(2, '0')
  return `${d.getDate()} ${MONTHS_RU[d.getMonth()]} в ${hh}:${mm}`
}

// ── Логотип ────────────────────────────────────────────────────────────────

const Logo = () => (
  <div className={styles.logoPlaceholder}>✂</div>
)

// ── Секция ближайшей записи ────────────────────────────────────────────────

const UpcomingAppointment = ({
  appointment,
  onViewAll,
}: {
  appointment: ClientAppointment
  onViewAll: () => void
}) => {
  const masterName = appointment.master
    ? [appointment.master.firstName, appointment.master.lastName].filter(Boolean).join(' ')
    : 'Мастер'

  const serviceNames = appointment.services.map(s => s.serviceName).join(', ')

  return (
    <div className={styles.upcomingCard}>
      <div className={styles.upcomingLeft}>
        <div className={styles.upcomingDateRow}>
          <CalendarIcon />
          <span className={styles.upcomingDate}>{formatDateTime(appointment.startTime)}</span>
        </div>
        <span className={styles.upcomingInfo}>{masterName} · {serviceNames}</span>
      </div>
      <button className={styles.allAppointmentsBtn} onClick={onViewAll}>
        Все записи
        <ChevronRightIcon />
      </button>
    </div>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const ClientPage = () => {
  const navigate = useNavigate()
  const [user,         setUser]         = useState<UserResponse | null>(null)
  const [masters,      setMasters]      = useState<MasterProfile[]>([])
  const [appointments, setAppointments] = useState<ClientAppointment[]>([])
  const [loading,      setLoading]      = useState(true)

  useEffect(() => {
    Promise.all([getMe(), getAllMasters(), getMyAppointments()])
      .then(([userData, mastersData, appointmentsData]) => {
        setUser(userData)
        setMasters(mastersData)
        setAppointments(appointmentsData)
      })
      .finally(() => setLoading(false))
  }, [])

  const nearest = appointments
    .slice()
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())[0] ?? null

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <Logo />
      </div>

      {/* Карточка пользователя */}
      {user && (
        <UserInfoCard user={user} onSettingsClick={() => navigate('/client/settings')} />
      )}

      {/* Ближайшая запись */}
      {nearest && (
        <UpcomingAppointment
          appointment={nearest}
          onViewAll={() => navigate('/client/appointments')}
        />
      )}

      {/* Список мастеров */}
      <section className={styles.mastersSection}>
        <h2 className={styles.sectionTitle}>Выберите мастера:</h2>
        <div className={styles.mastersGrid}>
          {masters.map(master => (
            <MasterCard
              key={master.id}
              master={master}
              onSelect={m => navigate(`/client/master/${m.id}/services`, { state: { master: m } })}
            />
          ))}
        </div>
      </section>

    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const CalendarIcon = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
    <line x1="16" y1="2" x2="16" y2="6" />
    <line x1="8"  y1="2" x2="8"  y2="6" />
    <line x1="3"  y1="10" x2="21" y2="10" />
  </svg>
)

const ChevronRightIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="9 18 15 12 9 6" />
  </svg>
)

export default ClientPage
