import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { MasterProfile } from '../../api/masters'
import { getMasterPhotoUrl } from '../../api/masters'
import type { MasterService } from '../../api/services'
import { bookAppointment } from '../../api/appointments'
import { durationToMinutes, minutesToFormatted } from '../../utils/duration'
import styles from './ConfirmPage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'января','февраля','марта','апреля','мая','июня',
  'июля','августа','сентября','октября','ноября','декабря',
]

const formatDate = (iso: string) => {
  const [y, m, d] = iso.split('-').map(Number)
  return `${d} ${MONTHS_RU[m - 1]} ${y}`
}

const formatTime = (isoUtc: string) => {
  const d = new Date(isoUtc)
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

// ── Типы ───────────────────────────────────────────────────────────────────

interface LocationState {
  master: MasterProfile
  selectedServices: MasterService[]
  selectedDate: string   // "YYYY-MM-DD"
  selectedTime: string   // "HH:MM:SS"
}

// ── Компонент ──────────────────────────────────────────────────────────────

const ConfirmPage = () => {
  const { masterProfileId } = useParams<{ masterProfileId: string }>()
  const navigate = useNavigate()
  const state = (history.state?.usr as LocationState) ?? {}
  const { master, selectedServices, selectedDate, selectedTime } = state

  const [comment,   setComment]   = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error,     setError]     = useState<string | null>(null)

  // Статистика
  const totalPrice = selectedServices?.reduce((s, svc) => s + svc.price, 0) ?? 0
  const totalMinutes = selectedServices?.reduce((s, svc) => s + durationToMinutes(svc.duration), 0) ?? 0
  const totalDuration = minutesToFormatted(totalMinutes)

  const fullName = master?.lastName
    ? `${master.firstName} ${master.lastName}`
    : master?.firstName ?? ''

  const initials = (master?.firstName?.charAt(0) ?? '') + (master?.lastName?.charAt(0) ?? '')

  const handleBook = async () => {
    if (!masterProfileId || !selectedDate || !selectedTime) return
    setSubmitting(true)
    setError(null)
    try {
      await bookAppointment({
        masterProfileId,
        serviceIds: selectedServices.map(s => s.id),
        startTime: selectedTime,
        comment: comment.trim() || undefined,
      })
      navigate('/client/booking-success', {
        state: { master, selectedServices, selectedDate, selectedTime, totalPrice },
      })
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Не удалось создать запись. Попробуйте ещё раз.')
    } finally {
      setSubmitting(false)
    }
  }


  return (
    <>
      <div className={`${styles.page} ${styles.pageWithPanel}`}>

        {/* Логотип */}
        <div className={styles.logoWrap}>
          <div className={styles.logoPlaceholder}>✂</div>
        </div>

        {/* Карточка мастера */}
        <div className={styles.card}>
          <div className={styles.cardLeft}>
            <div className={styles.masterAvatar}>
              {master?.photoUrl
                ? <img src={getMasterPhotoUrl(master.id)} alt={fullName} className={styles.avatarImg} />
                : <span className={styles.avatarInitials}>{initials}</span>
              }
            </div>
            <div className={styles.cardInfo}>
              <span className={styles.cardTitle}>{fullName}</span>
              <span className={styles.cardSub}>Ваш мастер</span>
            </div>
          </div>
        </div>

        {/* Карточка услуг */}
        <div className={styles.card}>
          <div className={styles.cardLeft}>
            <div className={styles.iconCircle}><ScissorsIcon /></div>
            <div className={styles.cardInfo}>
              {selectedServices?.map(s => (
                <span key={s.id} className={styles.cardTitle}>{s.name}</span>
              ))}
            </div>
          </div>
        </div>

        {/* Карточка даты и времени */}
        <div className={styles.card}>
          <div className={styles.cardLeft}>
            <div className={styles.iconCircle}><CalendarIcon /></div>
            <div className={styles.cardInfo}>
              <span className={styles.cardTitle}>{formatDate(selectedDate)}</span>
              <span className={styles.cardSub}>в {formatTime(selectedTime)} часов</span>
            </div>
          </div>
        </div>

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

        {/* Комментарий */}
        <div className={styles.inputCard}>
          <span className={styles.inputLabel}>Комментарий</span>
          <textarea
            className={styles.textarea}
            value={comment}
            onChange={e => setComment(e.target.value)}
            rows={3}
          />
        </div>

        {/* Ошибка */}
        {error && <p className={styles.error}>{error}</p>}
      </div>

      {/* Нижняя панель */}
      <div className={styles.bottomPanel}>
        <button
          className={styles.submitBtn}
          onClick={handleBook}
          disabled={submitting}
        >
          {submitting ? 'Запись...' : 'Подтвердить запись'}
        </button>
      </div>
    </>
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

export default ConfirmPage
