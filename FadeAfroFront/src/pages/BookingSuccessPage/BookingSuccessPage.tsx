import { useNavigate } from 'react-router-dom'
import type { MasterProfile } from '../../api/masters'
import type { MasterService } from '../../api/services'
import styles from './BookingSuccessPage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'января','февраля','марта','апреля','мая','июня',
  'июля','августа','сентября','октября','ноября','декабря',
]

const formatDate = (iso: string) => {
  const [y, m, d] = iso.split('-').map(Number)
  return `${d} ${MONTHS_RU[m - 1]} ${y}`
}

const formatTime = (t: string) => t.substring(0, 5)

// ── Типы ───────────────────────────────────────────────────────────────────

interface LocationState {
  master: MasterProfile
  selectedServices: MasterService[]
  selectedDate: string
  selectedTime: string
  totalPrice: number
}

// ── Компонент ──────────────────────────────────────────────────────────────

const BookingSuccessPage = () => {
  const navigate = useNavigate()
  const state = (history.state?.usr as LocationState) ?? {}
  const { master, selectedServices, selectedDate, selectedTime, totalPrice } = state

  const fullName = master?.lastName
    ? `${master.firstName} ${master.lastName}`
    : master?.firstName ?? ''

  const serviceNames = selectedServices?.map(s => s.name).join(', ') ?? ''

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      {/* Иконка успеха */}
      <div className={styles.successIconWrap}>
        <div className={styles.successIcon}>
          <CheckIcon />
        </div>
      </div>

      {/* Заголовок */}
      <div className={styles.titleBlock}>
        <h1 className={styles.title}>Запись подтверждена!</h1>
        <p className={styles.subtitle}>
          Мы ждём вас {formatDate(selectedDate)} в {formatTime(selectedTime)}
        </p>
      </div>

      {/* Карточка с деталями */}
      <div className={styles.card}>
        <div className={styles.row}>
          <span className={styles.rowLabel}>Мастер</span>
          <span className={styles.rowValue}>{fullName}</span>
        </div>
        <div className={styles.divider} />
        <div className={styles.row}>
          <span className={styles.rowLabel}>Услуги</span>
          <span className={styles.rowValue}>{serviceNames}</span>
        </div>
        <div className={styles.divider} />
        <div className={styles.row}>
          <span className={styles.rowLabel}>Итого</span>
          <span className={`${styles.rowValue} ${styles.rowValueAccent}`}>
            {totalPrice} ₽
          </span>
        </div>
      </div>

      {/* Кнопки */}
      <div className={styles.actions}>
        <button className={styles.mainBtn} onClick={() => navigate('/client')}>
          На главную
        </button>
        <button className={styles.secondaryBtn} onClick={() => navigate('/client')}>
          Мои записи
        </button>
      </div>

    </div>
  )
}

// ── Иконка ─────────────────────────────────────────────────────────────────

const CheckIcon = () => (
  <svg width="36" height="36" viewBox="0 0 24 24" fill="none"
    stroke="#ffffff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12" />
  </svg>
)

export default BookingSuccessPage
