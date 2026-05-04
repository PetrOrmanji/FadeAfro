import logo from '../../assets/logo.png'
import { useEffect, useRef, useState } from 'react'
import { useNavigate, type NavigateFunction } from 'react-router-dom'
import type { ClientAppointment } from '../../api/appointments'
import { cancelMyAppointment, getMyAppointments } from '../../api/appointments'
import { isRateLimitError, showRateLimitAlert } from '../../api/errors'
import { durationToMinutes, minutesToFormatted } from '../../utils/duration'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './MyAppointmentsPage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'января','февраля','марта','апреля','мая','июня',
  'июля','августа','сентября','октября','ноября','декабря',
]

const formatDate = (isoUtc: string) => {
  const d = new Date(isoUtc)
  return `${d.getDate()} ${MONTHS_RU[d.getMonth()]} ${d.getFullYear()}`
}

const formatTime = (isoUtc: string) => {
  const d = new Date(isoUtc)
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

// ── Карточка записи ────────────────────────────────────────────────────────

const AppointmentCard = ({
  appointment,
  onCancel,
  navigate,
}: {
  appointment: ClientAppointment
  onCancel: (id: string) => void
  navigate: NavigateFunction
}) => {
  const [photoError, setPhotoError] = useState(false)
  const [cancelling, setCancelling] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const masterName = appointment.master
    ? [appointment.master.firstName, appointment.master.lastName].filter(Boolean).join(' ')
    : 'Мастер'

  const initials = [
    appointment.master?.firstName?.charAt(0),
    appointment.master?.lastName?.charAt(0),
  ].filter(Boolean).join('')

  const totalPrice    = appointment.services.reduce((s, svc) => s + svc.price, 0)
  const totalMinutes  = appointment.services.reduce((s, svc) => s + durationToMinutes(svc.duration), 0)
  const totalDuration = minutesToFormatted(totalMinutes)

  const handleCancel = async () => {
    if (!ref.current || cancelling) return
    setCancelling(true)

    try {
      await cancelMyAppointment(appointment.id)
    } catch (e: unknown) {
      setCancelling(false)
      if (isRateLimitError(e)) { showRateLimitAlert(); return }
      navigate('/error', { replace: true })
      return
    }

    // API успешен — запускаем анимацию
    const el = ref.current
    el.style.height = `${el.offsetHeight}px`
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        el.style.transition = 'height 0.3s ease, margin 0.3s ease, opacity 0.2s ease, transform 0.2s ease'
        el.style.height = '0'
        el.style.marginBottom = '0'
        el.style.opacity = '0'
        el.style.transform = 'translateX(50px)'
        el.style.overflow = 'hidden'
      })
    })

    setTimeout(() => onCancel(appointment.id), 350)
  }

  return (
    <div ref={ref} className={styles.card}>

      {/* Мастер */}
      <div className={styles.section}>
        <div className={styles.masterAvatar}>
          {appointment.master?.photoUrl && !photoError
            ? <img
                src={appointment.master.photoUrl}
                alt={masterName}
                className={styles.avatarImg}
                onError={() => setPhotoError(true)}
              />
            : <span className={styles.avatarInitials}>{initials}</span>
          }
        </div>
        <div className={styles.sectionInfo}>
          <span className={styles.sectionTitle}>{masterName}</span>
          <span className={styles.sectionSub}>Ваш мастер</span>
        </div>
      </div>

      <div className={styles.divider} />

      {/* Услуги */}
      <div className={styles.section}>
        <div className={styles.iconCircle}><ScissorsIcon /></div>
        <div className={styles.sectionInfo}>
          {appointment.services.map((s, i) => (
            <span key={i} className={styles.sectionTitle}>{s.serviceName}</span>
          ))}
        </div>
      </div>

      <div className={styles.divider} />

      {/* Дата и время */}
      <div className={styles.section}>
        <div className={styles.iconCircle}><CalendarIcon /></div>
        <div className={styles.sectionInfo}>
          <span className={styles.sectionTitle}>{formatDate(appointment.startTime)}</span>
          <span className={styles.sectionSub}>в {formatTime(appointment.startTime)} часов</span>
        </div>
      </div>

      <div className={styles.divider} />

      {/* Статистика */}
      <div className={styles.stats}>
        <div className={styles.statItem}>
          <span className={styles.statValue}>{totalPrice} ₽</span>
          <span className={styles.statLabel}>Итого</span>
        </div>
        <div className={styles.statDivider} />
        <div className={styles.statItem}>
          <span className={styles.statValue}>{totalDuration}</span>
          <span className={styles.statLabel}>Длительность</span>
        </div>
      </div>

      <div className={styles.divider} />

      {/* Отмена */}
      <button
        className={styles.cancelBtn}
        onClick={handleCancel}
        disabled={cancelling}
      >
        {cancelling ? 'Отмена...' : 'Отменить запись'}
      </button>

    </div>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const MyAppointmentsPage = () => {
  useBackButton()
  const navigate = useNavigate()
  const [appointments, setAppointments] = useState<ClientAppointment[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMyAppointments()
      .then(data => {
        const sorted = [...data].sort((a, b) =>
          new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
        )
        setAppointments(sorted)
      })
      .catch(() => navigate('/error'))
      .finally(() => setLoading(false))
  }, [navigate])

  const handleCancel = (id: string) => {
    setAppointments(prev => prev.filter(a => a.id !== id))
  }

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Шапка — не прокручивается */}
      <div className={styles.header}>
        <div className={styles.logoWrap}>
          <img src={logo} className={styles.logo} alt="FadeAfro" />
        </div>
        <h1 className={styles.title}>Мои записи</h1>
      </div>

      {/* Контент — прокручивается */}
      {appointments.length === 0 ? (
        <div className={styles.emptyCard}>
          <EmptyCalendarIllustration />
          <p className={styles.emptyTitle}>Нет активных записей</p>
          <p className={styles.emptyText}>Запишитесь к мастеру прямо сейчас</p>
        </div>
      ) : (
        <div className={styles.list}>
          {appointments.map(a => (
            <AppointmentCard key={a.id} appointment={a} onCancel={handleCancel} navigate={navigate} />
          ))}
        </div>
      )}

    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const ScissorsIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="6" cy="6" r="3" />
    <circle cx="6" cy="18" r="3" />
    <line x1="20" y1="4" x2="8.12" y2="15.88" />
    <line x1="14.47" y1="14.48" x2="20" y2="20" />
    <line x1="8.12" y1="8.12" x2="12" y2="12" />
  </svg>
)

const CalendarIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
    <line x1="16" y1="2" x2="16" y2="6" />
    <line x1="8"  y1="2" x2="8"  y2="6" />
    <line x1="3"  y1="10" x2="21" y2="10" />
  </svg>
)

const EmptyCalendarIllustration = () => (
  <svg width="140" height="140" viewBox="0 0 140 140" fill="none" xmlns="http://www.w3.org/2000/svg">
    {/* Фоновый круг */}
    <circle cx="70" cy="75" r="52" fill="var(--tg-button)" opacity="0.08"/>
    {/* Тело календаря */}
    <rect x="18" y="28" width="104" height="90" rx="14" fill="var(--tg-bg)" stroke="var(--tg-button)" strokeWidth="3.5"/>
    {/* Шапка */}
    <rect x="18" y="28" width="104" height="32" rx="14" fill="var(--tg-button)" opacity="0.18"/>
    <rect x="18" y="46" width="104" height="14" fill="var(--tg-button)" opacity="0.18"/>
    {/* Крючки */}
    <line x1="44" y1="14" x2="44" y2="38" stroke="var(--tg-button)" strokeWidth="4.5" strokeLinecap="round"/>
    <line x1="96" y1="14" x2="96" y2="38" stroke="var(--tg-button)" strokeWidth="4.5" strokeLinecap="round"/>
    {/* Крестик внутри */}
    <circle cx="70" cy="86" r="20" fill="var(--tg-button)" opacity="0.1"/>
    <line x1="60" y1="76" x2="80" y2="96" stroke="var(--tg-button)" strokeWidth="4" strokeLinecap="round"/>
    <line x1="80" y1="76" x2="60" y2="96" stroke="var(--tg-button)" strokeWidth="4" strokeLinecap="round"/>
  </svg>
)

export default MyAppointmentsPage
