import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getMe, type UserResponse } from '../../api/user'
import { getMyMasterProfile, getMasterPhotoUrl, type MasterProfile } from '../../api/masters'
import { getUnreadNotificationsCount } from '../../api/notifications'
import { getMasterSchedules, normalizeDayOfWeek, type MasterScheduleItem } from '../../api/schedule'
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

// ── Страница ───────────────────────────────────────────────────────────────

const MasterPage = () => {
  const navigate = useNavigate()
  const [user,          setUser]          = useState<UserResponse | null>(null)
  const [masterProfile, setMasterProfile] = useState<MasterProfile | null>(null)
  const [schedules,     setSchedules]     = useState<MasterScheduleItem[]>([])
  const [unreadCount,   setUnreadCount]   = useState(0)
  const [loading,       setLoading]       = useState(true)

  useEffect(() => {
    ;(async () => {
      try {
        const [userData, profileData, unreadCountData] = await Promise.all([
          getMe(), getMyMasterProfile(), getUnreadNotificationsCount(),
        ])
        setUser(userData)
        setMasterProfile(profileData)
        setUnreadCount(unreadCountData)
        const schedulesData = await getMasterSchedules(profileData.id)
        setSchedules(schedulesData)
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

export default MasterPage
