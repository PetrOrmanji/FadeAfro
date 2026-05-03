import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
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

const trimSeconds = (t: string) => t.slice(0, 5)

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
  const times = schedules.map(s => `${trimSeconds(s.startTime)}–${trimSeconds(s.endTime)}`)
  const uniqueTimes = [...new Set(times)]
  const timeLabel = uniqueTimes.length === 1 ? uniqueTimes[0] : null

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

      {/* Подпись */}
      <div className={styles.scheduleFooter}>
        {workCount === 0 ? (
          <span className={styles.scheduleHint}>График не настроен</span>
        ) : timeLabel ? (
          <span className={styles.scheduleTime}>{timeLabel}</span>
        ) : null}
      </div>

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

// ── Страница ───────────────────────────────────────────────────────────────

const MasterPage = () => {
  const navigate = useNavigate()
  const [user,          setUser]          = useState<UserResponse | null>(null)
  const [masterProfile, setMasterProfile] = useState<MasterProfile | null>(null)
  const [schedules,       setSchedules]       = useState<MasterScheduleItem[]>([])
  const [unavailabilities, setUnavailabilities] = useState<MasterUnavailabilityItem[]>([])
  const [unreadCount,     setUnreadCount]     = useState(0)
  const [loading,         setLoading]         = useState(true)

  useEffect(() => {
    ;(async () => {
      try {
        const [userData, profileData, unreadCountData] = await Promise.all([
          getMe(), getMyMasterProfile(), getUnreadNotificationsCount(),
        ])
        setUser(userData)
        setMasterProfile(profileData)
        setUnreadCount(unreadCountData)
        const [schedulesData, unavailData] = await Promise.all([
          getMasterSchedules(profileData.id),
          getMasterUnavailabilities(profileData.id),
        ])
        setSchedules(schedulesData)
        setUnavailabilities(unavailData)
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

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
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
      <div className={styles.scheduleCard} onClick={() => navigate('/master/appointments')}>
        <div className={styles.scheduleHeader}>
          <div className={styles.scheduleTitleRow}>
            <AppointmentsIcon />
            <span className={styles.scheduleTitle}>Записи</span>
          </div>
          <ChevronRightIcon />
        </div>
      </div>

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
