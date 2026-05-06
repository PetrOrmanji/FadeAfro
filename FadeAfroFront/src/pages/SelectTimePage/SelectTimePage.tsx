import logo from '../../assets/logo.png'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { MasterProfile } from '../../api/masters'
import type { MasterService } from '../../api/services'
import type { TimeSlot } from '../../api/availability'
import { getDayAvailability } from '../../api/availability'
import { durationToMinutes } from '../../utils/duration'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './SelectTimePage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'января', 'февраля', 'марта', 'апреля', 'мая', 'июня',
  'июля', 'августа', 'сентября', 'октября', 'ноября', 'декабря',
]

const formatDate = (iso: string) => {
  const [y, m, d] = iso.split('-').map(Number)
  return `${d} ${MONTHS_RU[m - 1]} ${y}`
}

// Бэк отдаёт корректный UTC — конвертируем в локальное время через Date
const formatTime = (iso: string) => {
  const d = new Date(iso)
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const getHour = (iso: string) => new Date(iso).getHours()

const minutesToDuration = (minutes: number): string => {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`
}

// ── Типы ───────────────────────────────────────────────────────────────────

interface LocationState {
  master: MasterProfile
  selectedServices: MasterService[]
  selectedDate: string  // "YYYY-MM-DD"
}

// ── Компоненты ─────────────────────────────────────────────────────────────

const SlotButton = ({
  slot,
  selected,
  onSelect,
}: {
  slot: TimeSlot
  selected: boolean
  onSelect: () => void
}) => (
  <button
    className={[
      styles.slot,
      selected ? styles.slotSelected : '',
      !slot.isActive ? styles.slotUnavailable : '',
    ].join(' ')}
    onClick={() => slot.isActive && onSelect()}
  >
    {formatTime(slot.time)}
  </button>
)

const SlotGroup = ({
  title,
  slots,
  selectedTime,
  onSelect,
}: {
  title: string
  slots: TimeSlot[]
  selectedTime: string | null
  onSelect: (time: string) => void
}) => {
  if (slots.length === 0) return null
  return (
    <section className={styles.section}>
      <h2 className={styles.sectionTitle}>{title}</h2>
      <div className={styles.slotsGrid}>
        {slots.map(slot => (
          <SlotButton
            key={slot.time}
            slot={slot}
            selected={selectedTime === slot.time}
            onSelect={() => onSelect(slot.time)}
          />
        ))}
      </div>
    </section>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const SelectTimePage = () => {
  const { masterProfileId } = useParams<{ masterProfileId: string }>()
  const navigate = useNavigate()
  useBackButton()
  const state = (history.state?.usr as LocationState) ?? {}
  const { master, selectedServices, selectedDate } = state

  const [slots,        setSlots]        = useState<TimeSlot[]>([])
  const [selectedTime, setSelectedTime] = useState<string | null>(null)
  const [loading,      setLoading]      = useState(true)

  // Считаем суммарную длительность услуг → передаём в API
  const serviceDuration = useMemo(() => {
    if (!selectedServices?.length) return '00:30:00'
    const total = selectedServices.reduce((sum, s) => sum + durationToMinutes(s.duration), 0)
    return minutesToDuration(total)
  }, [selectedServices])

  useEffect(() => {
    if (!masterProfileId || !selectedDate) return
    ;(async () => {
      try {
        const result = await getDayAvailability(masterProfileId, selectedDate, serviceDuration)
        setSlots(result)
      } catch {
        navigate('/error', { replace: true })
        return
      } finally {
        setLoading(false)
      }
    })()
  }, [masterProfileId, selectedDate, serviceDuration, navigate])

  const morningSlots   = slots.filter(s => getHour(s.time) < 12)
  const afternoonSlots = slots.filter(s => getHour(s.time) >= 12 && getHour(s.time) < 18)
  const eveningSlots   = slots.filter(s => getHour(s.time) >= 18)

  const handleNext = () => {
    if (!selectedTime) return
    navigate(`/client/master/${masterProfileId}/confirm`, {
      state: { master, selectedServices, selectedDate, selectedTime },
    })
  }

  if (loading) return <LoadingScreen />

  return (
    <>
      <div className={`${styles.page} ${selectedTime ? styles.pageWithPanel : ''}`}>

        <div className={styles.logoWrap}>
          <img src={logo} className={styles.logo} alt="FadeAfro" />
          <div className={styles.dateRow}>
            <CalendarIcon />
            <span className={styles.dateText}>{formatDate(selectedDate)}</span>
          </div>
        </div>

        <h1 className={styles.title}>Выберите время</h1>

        {/* Слоты */}
        <SlotGroup title="Утро"  slots={morningSlots}   selectedTime={selectedTime} onSelect={setSelectedTime} />
        <SlotGroup title="День"  slots={afternoonSlots} selectedTime={selectedTime} onSelect={setSelectedTime} />
        <SlotGroup title="Вечер" slots={eveningSlots}   selectedTime={selectedTime} onSelect={setSelectedTime} />

        {slots.length === 0 && (
          <p className={styles.noSlots}>Нет доступных слотов на выбранный день</p>
        )}
      </div>

      {/* Нижняя панель — вне анимированного div */}
      {selectedTime && (
        <div className={styles.bottomPanel}>
          <button className={styles.nextBtn} onClick={handleNext}>Далее</button>
        </div>
      )}
    </>
  )
}

// ── Иконка ─────────────────────────────────────────────────────────────────

const CalendarIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
    <line x1="16" y1="2" x2="16" y2="6" />
    <line x1="8"  y1="2" x2="8"  y2="6" />
    <line x1="3"  y1="10" x2="21" y2="10" />
  </svg>
)

export default SelectTimePage
