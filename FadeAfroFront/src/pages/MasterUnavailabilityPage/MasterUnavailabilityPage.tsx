import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  getMasterSchedules,
  getMasterUnavailabilities,
  addMyUnavailability,
  deleteMyUnavailability,
  normalizeDayOfWeek,
} from '../../api/schedule'
import { getMyMasterProfile } from '../../api/masters'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './MasterUnavailabilityPage.module.css'

// ── Константы ──────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'Январь','Февраль','Март','Апрель','Май','Июнь',
  'Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь',
]
const DAY_HEADERS = ['Пн','Вт','Ср','Чт','Пт','Сб','Вс']

// ── Утилиты ────────────────────────────────────────────────────────────────

const toISO = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`

const buildCalendarDays = (year: number, month: number): Date[] => {
  const days: Date[] = []
  const firstDow = (new Date(year, month, 1).getDay() + 6) % 7
  const lastDate = new Date(year, month + 1, 0).getDate()
  for (let i = 0; i < firstDow; i++) days.push(new Date(year, month, 1 - firstDow + i))
  for (let d = 1; d <= lastDate; d++) days.push(new Date(year, month, d))
  let next = 1
  while (days.length % 7 !== 0) days.push(new Date(year, month + 1, next++))
  return days
}

// ── Страница ───────────────────────────────────────────────────────────────

const MasterUnavailabilityPage = () => {
  useBackButton()
  const navigate = useNavigate()

  const today = useMemo(() => {
    const d = new Date(); d.setHours(0,0,0,0); return d
  }, [])

  const [viewYear,  setViewYear]  = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const [workingDows, setWorkingDows] = useState<Set<number>>(new Set())
  const [unavailMap,  setUnavailMap]  = useState<Map<string, string>>(new Map())
  const [selectedISO, setSelectedISO] = useState<string | null>(null)
  const [saving,      setSaving]      = useState(false)
  const [loading,     setLoading]     = useState(true)

  const [masterId, setMasterId] = useState<string | null>(null)

  const reloadUnavailabilities = async (id: string) => {
    const unavails = await getMasterUnavailabilities(id)
    setUnavailMap(new Map(unavails.map(u => [u.date.slice(0, 10), u.id])))
  }

  useEffect(() => {
    ;(async () => {
      try {
        const profile = await getMyMasterProfile()
        const [schedules, unavails] = await Promise.all([
          getMasterSchedules(profile.id),
          getMasterUnavailabilities(profile.id),
        ])
        setMasterId(profile.id)
        setWorkingDows(new Set(schedules.map(s => normalizeDayOfWeek(s.dayOfWeek))))
        setUnavailMap(new Map(unavails.map(u => [u.date.slice(0, 10), u.id])))
      } catch {
        navigate('/error', { replace: true })
      } finally {
        setLoading(false)
      }
    })()
  }, [navigate])

  // Навигация: только текущий и следующий месяц
  const todayIdx = today.getFullYear() * 12 + today.getMonth()
  const viewIdx  = viewYear * 12 + viewMonth
  const canPrev  = viewIdx > todayIdx
  const canNext  = viewIdx < todayIdx + 1

  const goToPrev = () => {
    if (!canPrev) return
    setSelectedISO(null)
    if (viewMonth === 0) { setViewMonth(11); setViewYear(y => y - 1) }
    else setViewMonth(m => m - 1)
  }
  const goToNext = () => {
    if (!canNext) return
    setSelectedISO(null)
    if (viewMonth === 11) { setViewMonth(0); setViewYear(y => y + 1) }
    else setViewMonth(m => m + 1)
  }

  const days = useMemo(() => buildCalendarDays(viewYear, viewMonth), [viewYear, viewMonth])
  const todayISO = toISO(today)

  const isSelectable = (d: Date) => {
    const isCurrentMonth = d.getMonth() === viewMonth
    const isPast = d < today
    const isWorking = workingDows.has(d.getDay())
    return isCurrentMonth && !isPast && isWorking
  }

  const getCellClass = (d: Date) => {
    const iso = toISO(d)
    const isCurrentMonth = d.getMonth() === viewMonth
    const isPast = d < today
    const isToday = iso === todayISO
    const isWorking = workingDows.has(d.getDay())
    const isUnavail = unavailMap.has(iso)
    const isSelected = iso === selectedISO

    if (!isCurrentMonth)             return `${styles.day} ${styles.dayOther}`
    if (isPast)                      return `${styles.day} ${styles.dayPast}`
    if (!isWorking)                  return `${styles.day} ${styles.dayOff}`
    if (isSelected && isUnavail)     return `${styles.day} ${styles.daySelectedUnavail}`
    if (isSelected)                  return `${styles.day} ${styles.daySelected}`
    if (isUnavail)                   return `${styles.day} ${styles.dayUnavail}`
    if (isToday)                     return `${styles.day} ${styles.dayToday}`
    return styles.day
  }

  const handleDayClick = (d: Date) => {
    if (!isSelectable(d)) return
    const iso = toISO(d)
    setSelectedISO(prev => prev === iso ? null : iso)
  }

  const selectedIsUnavail = selectedISO ? unavailMap.has(selectedISO) : false

  const handleConfirm = async () => {
    if (!selectedISO || saving || !masterId) return
    setSaving(true)
    try {
      if (selectedIsUnavail) {
        const id = unavailMap.get(selectedISO)!
        await deleteMyUnavailability(id)
      } else {
        await addMyUnavailability(selectedISO)
      }
      await reloadUnavailabilities(masterId)
      setSelectedISO(null)
    } catch (e: unknown) {
      const status = (e as { response?: { status?: number } })?.response?.status
      if (!status || status >= 500) navigate('/error', { replace: true })
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <LoadingScreen />

  return (
    <>
      <div className={styles.page}>

        {/* Шапка */}
        <div className={styles.header}>
          <div className={styles.logoWrap}>
            <div className={styles.logoPlaceholder}>✂</div>
          </div>
          <h1 className={styles.title}>Дни отсутствия</h1>
        </div>

        {/* Контент */}
        <div className={`${styles.content} ${selectedISO ? styles.contentWithPanel : ''}`}>

          {/* Карточка календаря */}
          <div className={styles.calendarCard}>
            <div className={styles.calendarHeader}>
              <button
                className={`${styles.navBtn} ${!canPrev ? styles.navBtnDisabled : ''}`}
                onClick={goToPrev}
              >‹</button>
              <span className={styles.monthTitle}>
                {MONTHS_RU[viewMonth]} {viewYear}
              </span>
              <button
                className={`${styles.navBtn} ${!canNext ? styles.navBtnDisabled : ''}`}
                onClick={goToNext}
              >›</button>
            </div>
            <div className={styles.grid}>
              {DAY_HEADERS.map(h => (
                <div key={h} className={styles.dayHeader}>{h}</div>
              ))}
              {days.map((d, i) => (
                <button key={i} className={getCellClass(d)} onClick={() => handleDayClick(d)}>
                  {d.getDate()}
                </button>
              ))}
            </div>
          </div>

          {/* Легенда */}
          <div className={styles.legend}>
            <div className={styles.legendItem}>
              <span className={`${styles.legendDot} ${styles.legendDotOff}`} />
              <span className={styles.legendText}>Выходной</span>
            </div>
            <div className={styles.legendItem}>
              <span className={`${styles.legendDot} ${styles.legendDotUnavail}`} />
              <span className={styles.legendText}>Отсутствие</span>
            </div>
          </div>

        </div>
      </div>

      {/* Нижняя панель */}
      {selectedISO && (
        <div className={styles.bottomPanel}>
          <button
            className={`${styles.actionBtn} ${selectedIsUnavail ? styles.actionBtnDelete : ''}`}
            onClick={handleConfirm}
            disabled={saving}
          >
            {saving
              ? 'Сохранение...'
              : selectedIsUnavail
                ? 'Удалить отсутствие'
                : 'Отметить отсутствие'
            }
          </button>
        </div>
      )}
    </>
  )
}

export default MasterUnavailabilityPage
