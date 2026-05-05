import logo from '../../assets/logo.png'
import { useEffect, useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { getMe, type UserResponse } from '../../api/user'
import { getMyMasterProfile, getMasterPhotoUrl, type MasterProfile } from '../../api/masters'
import { getUnreadNotificationsCount } from '../../api/notifications'
import {
  getMasterSchedules,
  getMasterUnavailabilities,
  normalizeDayOfWeek,
  type MasterScheduleItem,
  type MasterUnavailabilityItem,
} from '../../api/schedule'
import { getMasterAppointments, type MasterAppointment } from '../../api/appointments'
import { getMasterServices, type MasterService } from '../../api/services'
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import styles from './MasterPage.module.css'

// ── Константы ──────────────────────────────────────────────────────────────

const DAYS = [
  { dow: 1, short: 'Пн' },
  { dow: 2, short: 'Вт' },
  { dow: 3, short: 'Ср' },
  { dow: 4, short: 'Чт' },
  { dow: 5, short: 'Пт' },
  { dow: 6, short: 'Сб' },
  { dow: 0, short: 'Вс' },
]

// ── Карточка графика ───────────────────────────────────────────────────────

const ScheduleCard = ({
  schedules,
  onClick,
}: {
  schedules: MasterScheduleItem[]
  onClick: () => void
}) => {
  const workingDows = new Set(schedules.map(s => normalizeDayOfWeek(s.dayOfWeek)))

  // Определяем общее время (если у всех дней одинаковое — показываем одно)
  const workCount = workingDows.size

  return (
    <div className={styles.scheduleCard} onClick={onClick}>

      {/* Заголовок */}
      <div className={styles.scheduleHeader}>
        <div className={styles.scheduleTitleRow}>
          <ClockIcon />
          <span className={styles.scheduleTitle}>График работы</span>
        </div>
        <ChevronRightIcon />
      </div>

      {/* Дни недели */}
      <div className={styles.daysRow}>
        {DAYS.map(({ dow, short }) => (
          <div
            key={dow}
            className={`${styles.dayChip} ${workingDows.has(dow) ? styles.dayChipActive : ''}`}
          >
            {short}
          </div>
        ))}
      </div>

      {workCount === 0 && (
        <span className={styles.scheduleHint}>График не настроен</span>
      )}

    </div>
  )
}

// ── Утилита: форматирование даты ───────────────────────────────────────────

const MONTHS_SHORT = ['янв','фев','мар','апр','мая','июн','июл','авг','сен','окт','ноя','дек']

const formatDate = (iso: string) => {
  const d = new Date(iso)
  return `${d.getDate()} ${MONTHS_SHORT[d.getMonth()]}`
}

// ── Карточка отсутствий ────────────────────────────────────────────────────

const UnavailabilityCard = ({
  unavailabilities,
  onClick,
}: {
  unavailabilities: MasterUnavailabilityItem[]
  onClick: () => void
}) => {
  const today = new Date(); today.setHours(0, 0, 0, 0)

  const upcoming = unavailabilities
    .filter(u => new Date(u.date.slice(0, 10)) >= today)
    .sort((a, b) => a.date.localeCompare(b.date))

  return (
    <div className={styles.unavailCard} onClick={onClick}>

      <div className={styles.scheduleHeader}>
        <div className={styles.scheduleTitleRow}>
          <CalendarOffIcon />
          <span className={styles.scheduleTitle}>Дни отсутствия</span>
        </div>
        <ChevronRightIcon />
      </div>

      {upcoming.length === 0 ? (
        <span className={styles.scheduleHint}>Нет отмеченных дней</span>
      ) : (
        <div className={styles.unavailChips}>
          {upcoming.slice(0, 4).map(u => (
            <span key={u.id} className={styles.unavailChip}>
              {formatDate(u.date)}
            </span>
          ))}
          {upcoming.length > 4 && (
            <span className={styles.unavailChipMore}>+{upcoming.length - 4}</span>
          )}
        </div>
      )}

    </div>
  )
}

// ── Карточка услуг ────────────────────────────────────────────────────────

const ServicesCard = ({
  services,
  onClick,
}: {
  services: MasterService[]
  onClick: () => void
}) => (
  <div className={styles.scheduleCard} onClick={onClick}>
    <div className={styles.scheduleHeader}>
      <div className={styles.scheduleTitleRow}>
        <ServicesIcon />
        <span className={styles.scheduleTitle}>Услуги</span>
      </div>
      <ChevronRightIcon />
    </div>
    {services.length === 0 ? (
      <span className={styles.scheduleHint}>Услуги не добавлены</span>
    ) : (
      <div className={styles.serviceChips}>
        {services.slice(0, 3).map(s => (
          <span key={s.id} className={styles.serviceChip}>{s.name}</span>
        ))}
        {services.length > 3 && (
          <span className={styles.serviceChipMore}>+{services.length - 3}</span>
        )}
      </div>
    )}
  </div>
)

// ── Карточка записей ───────────────────────────────────────────────────────

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

const AppointmentsCard = ({
  nearest,
  onClick,
}: {
  nearest: MasterAppointment | null
  onClick: () => void
}) => {
  const clientName = nearest?.client
    ? [nearest.client.firstName, nearest.client.lastName].filter(Boolean).join(' ')
    : null
  const serviceNames = nearest?.services.map(s => s.serviceName).join(', ')

  return (
    <div className={styles.scheduleCard} onClick={onClick}>

      <div className={styles.scheduleHeader}>
        <div className={styles.scheduleTitleRow}>
          <AppointmentsIcon />
          <span className={styles.scheduleTitle}>Записи</span>
        </div>
        <ChevronRightIcon />
      </div>

      {nearest ? (
        <div className={styles.appointmentNearest}>
          <span className={styles.appointmentDate}>{formatDateTime(nearest.startTime)}</span>
          <span className={styles.appointmentClient}>{clientName} · {serviceNames}</span>
        </div>
      ) : (
        <span className={styles.scheduleHint}>Нет активных записей</span>
      )}

    </div>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const MasterPage = () => {
  const navigate  = useNavigate()
  const location  = useLocation()
  const fromOwner = (location.state as { from?: string } | null)?.from === 'owner'
  const [user,          setUser]          = useState<UserResponse | null>(null)
  const [masterProfile, setMasterProfile] = useState<MasterProfile | null>(null)
  const [schedules,       setSchedules]       = useState<MasterScheduleItem[]>([])
  const [unavailabilities,  setUnavailabilities]  = useState<MasterUnavailabilityItem[]>([])
  const [appointments,      setAppointments]      = useState<MasterAppointment[]>([])
  const [services,          setServices]          = useState<MasterService[]>([])
  const [unreadCount,       setUnreadCount]       = useState(0)
  const [loading,           setLoading]           = useState(true)

  useEffect(() => {
    ;(async () => {
      try {
        const [userData, profileData, unreadCountData] = await Promise.all([
          getMe(), getMyMasterProfile(), getUnreadNotificationsCount(),
        ])
        setUser(userData)
        setMasterProfile(profileData)
        setUnreadCount(unreadCountData)
        const [schedulesData, unavailData, appointmentsData, servicesData] = await Promise.all([
          getMasterSchedules(profileData.id),
          getMasterUnavailabilities(profileData.id),
          getMasterAppointments(),
          getMasterServices(profileData.id),
        ])
        setSchedules(schedulesData)
        setUnavailabilities(unavailData)
        setAppointments(appointmentsData)
        setServices(servicesData)
      } catch {
        navigate('/error')
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  const masterPhotoUrl = masterProfile?.photoUrl
    ? getMasterPhotoUrl(masterProfile.id)
    : null

  const nearest = appointments
    .slice()
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())[0] ?? null

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <img src={logo} className={styles.logo} alt="FadeAfro" />
      </div>

      {/* Карточка пользователя */}
      {user && (
        <UserInfoCard
          user={user}
          unreadCount={unreadCount}
          overridePhotoUrl={masterPhotoUrl}
          avatarShape="rect"
          onSettingsClick={() => navigate('/master/settings')}
          onNotificationsClick={() => navigate('/master/notifications')}
        />
      )}

      {/* Таб-свитчер — только если пришли из OwnerPage */}
      {fromOwner && (
        <div className={styles.tabSwitcher}>
          <button
            className={styles.tabBtn}
            onClick={() => navigate('/owner')}
          >
            Владелец
          </button>
          <button className={`${styles.tabBtn} ${styles.tabBtnActive}`}>
            Мастер
          </button>
        </div>
      )}

      {/* Контент */}
      {loading ? (
        <div className={styles.contentLoader}>
          <div className={styles.spinner} />
        </div>
      ) : (
        <>
          {/* Услуги */}
          <ServicesCard
            services={services}
            onClick={() => navigate('/master/services')}
          />

          {/* График */}
          <ScheduleCard
            schedules={schedules}
            onClick={() => navigate('/master/schedule')}
          />

          {/* Дни отсутствия */}
          <UnavailabilityCard
            unavailabilities={unavailabilities}
            onClick={() => navigate('/master/unavailability')}
          />

          {/* Записи */}
          <AppointmentsCard
            nearest={nearest}
            onClick={() => navigate('/master/appointments')}
          />
        </>
      )}

    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const ClockIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <polyline points="12 6 12 12 16 14" />
  </svg>
)

const ChevronRightIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="9 18 15 12 9 6" />
  </svg>
)

const AppointmentsIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
    <circle cx="9" cy="7" r="4" />
    <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
    <path d="M16 3.13a4 4 0 0 1 0 7.75" />
  </svg>
)

const ServicesIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
  </svg>
)

const CalendarOffIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
    <line x1="16" y1="2" x2="16" y2="6" />
    <line x1="8" y1="2" x2="8" y2="6" />
    <line x1="3" y1="10" x2="21" y2="10" />
    <line x1="9" y1="14" x2="15" y2="20" />
    <line x1="15" y1="14" x2="9" y2="20" />
  </svg>
)

export default MasterPage
