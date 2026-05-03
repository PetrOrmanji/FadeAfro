import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { MasterProfile } from '../../api/masters'
import type { MasterService } from '../../api/services'
import { getMasterSchedules, getMasterUnavailabilities, normalizeDayOfWeek } from '../../api/schedule'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './SelectDatePage.module.css'

// ── Константы ──────────────────────────────────────────────────────────────

const MONTHS_RU = [
  'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
  'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
]
const DAY_HEADERS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

// ── Утилиты ────────────────────────────────────────────────────────────────

/** Date → "YYYY-MM-DD" */
const toISO = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`

/** Строит массив дат для календарной сетки (пн–вс, 6 строк макс.) */
const buildCalendarDays = (year: number, month: number): Date[] => {
  const days: Date[] = []
  const firstDay  = new Date(year, month, 1)
  const firstDow  = (firstDay.getDay() + 6) % 7   // 0=Пн … 6=Вс
  const lastDate  = new Date(year, month + 1, 0).getDate()

  // Хвост предыдущего месяца
  for (let i = 0; i < firstDow; i++) {
    days.push(new Date(year, month, 1 - firstDow + i))
  }
  // Текущий месяц
  for (let d = 1; d <= lastDate; d++) {
    days.push(new Date(year, month, d))
  }
  // Начало следующего месяца
  let nextDay = 1
  while (days.length % 7 !== 0) {
    days.push(new Date(year, month + 1, nextDay++))
  }
  return days
}

// ── Типы ───────────────────────────────────────────────────────────────────

interface LocationState {
  master: MasterProfile
  selectedServices: MasterService[]
}

// ── Компонент ──────────────────────────────────────────────────────────────

const SelectDatePage = () => {
  const { masterProfileId } = useParams<{ masterProfileId: string }>()
  const navigate  = useNavigate()
  useBackButton()
  const state     = (history.state?.usr as LocationState) ?? {}
  const { master, selectedServices } = state

  // Сегодня (начало дня)
  const today = useMemo(() => {
    const d = new Date()
    d.setHours(0, 0, 0, 0)
    return d
  }, [])

  // Просматриваемый месяц
  const [viewYear,  setViewYear]  = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const [selectedDate,      setSelectedDate]      = useState<Date | null>(null)
  const [workingDays,       setWorkingDays]       = useState<Set<number>>(new Set())
  const [unavailableDates,  setUnavailableDates]  = useState<Set<string>>(new Set())
  const [loading,           setLoading]           = useState(true)

  // Индекс месяца для сравнения (year*12 + month)
  const todayIdx = today.getFullYear() * 12 + today.getMonth()
  const viewIdx  = viewYear * 12 + viewMonth
  const canGoPrev = viewIdx > todayIdx
  const canGoNext = viewIdx < todayIdx + 1   // разрешаем текущий + следующий

  // Загрузка расписания и недоступностей
  useEffect(() => {
    if (!masterProfileId) return
    ;(async () => {
      try {
        const [schedules, unavailabilities] = await Promise.all([
          getMasterSchedules(masterProfileId),
          getMasterUnavailabilities(masterProfileId),
        ])
        setWorkingDays(new Set(schedules.map(s => normalizeDayOfWeek(s.dayOfWeek))))
        setUnavailableDates(new Set(unavailabilities.map(u => u.date)))
      } catch {
        navigate('/error', { replace: true })
        return
      } finally {
        setLoading(false)
      }
    })()
  }, [masterProfileId, navigate])

  // Навигация по месяцам
  const goToPrev = () => {
    if (!canGoPrev) return
    setSelectedDate(null)
    if (viewMonth === 0) { setViewMonth(11); setViewYear(y => y - 1) }
    else setViewMonth(m => m - 1)
  }
  const goToNext = () => {
    if (!canGoNext) return
    setSelectedDate(null)
    if (viewMonth === 11) { setViewMonth(0); setViewYear(y => y + 1) }
    else setViewMonth(m => m + 1)
  }

  // Генерация ячеек
  const calendarDays = useMemo(() => buildCalendarDays(viewYear, viewMonth), [viewYear, viewMonth])

  // Предикаты
  const isCurrentMonth = (d: Date) => d.getFullYear() === viewYear && d.getMonth() === viewMonth
  const isPast         = (d: Date) => d < today
  const isToday        = (d: Date) => toISO(d) === toISO(today)
  const isOff          = (d: Date) => !workingDays.has(d.getDay()) || unavailableDates.has(toISO(d))
  const isSelected     = (d: Date) => selectedDate !== null && toISO(d) === toISO(selectedDate)
  const isSelectable   = (d: Date) => isCurrentMonth(d) && !isPast(d) && !isOff(d)

  // Класс ячейки: isOff перекрывает isPast и !isCurrentMonth (как в референсе)
  const getDayClass = (d: Date) => {
    if (isSelected(d))                   return `${styles.day} ${styles.daySelected}`
    if (isPast(d) || !isCurrentMonth(d)) return `${styles.day} ${styles.dayPast}`
    if (isOff(d))                        return `${styles.day} ${styles.dayOff}`
    if (isToday(d))                      return `${styles.day} ${styles.dayToday}`
    return styles.day
  }

  const handleDayClick = (d: Date) => {
    if (!isSelectable(d)) return
    setSelectedDate(prev => (prev && toISO(prev) === toISO(d)) ? null : d)
  }

  const handleNext = () => {
    if (!selectedDate) return
    navigate(`/client/master/${masterProfileId}/time`, {
      state: { master, selectedServices, selectedDate: toISO(selectedDate) },
    })
  }

  if (loading) return <LoadingScreen />

  return (
    <>
    <div className={`${styles.page} ${selectedDate ? styles.pageWithPanel : ''}`}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      {/* Заголовок */}
      <h1 className={styles.title}>Выберите дату</h1>

      {/* Календарь */}
      <div className={styles.calendarCard}>

        {/* Шапка с навигацией */}
        <div className={styles.calendarHeader}>
          <button
            className={`${styles.navBtn} ${!canGoPrev ? styles.navBtnDisabled : ''}`}
            onClick={goToPrev}
          >
            ‹
          </button>
          <span className={styles.monthTitle}>
            {MONTHS_RU[viewMonth]} {viewYear}
          </span>
          <button
            className={`${styles.navBtn} ${!canGoNext ? styles.navBtnDisabled : ''}`}
            onClick={goToNext}
          >
            ›
          </button>
        </div>

        {/* Сетка */}
        <div className={styles.grid}>
          {/* Заголовки дней недели */}
          {DAY_HEADERS.map(h => (
            <div key={h} className={styles.dayHeader}>{h}</div>
          ))}

          {/* Ячейки дней */}
          {calendarDays.map((d, i) => (
            <button
              key={i}
              className={getDayClass(d)}
              onClick={() => handleDayClick(d)}
            >
              {d.getDate()}
            </button>
          ))}
        </div>
      </div>

      {/* Легенда */}
      <div className={styles.legend}>
        <span className={styles.legendDot} />
        <span className={styles.legendText}>Выходные мастера</span>
      </div>

    </div>

      {/* Нижняя панель — вне анимированного div */}
      {selectedDate && (
        <div className={styles.bottomPanel}>
          <button className={styles.nextBtn} onClick={handleNext}>Далее</button>
        </div>
      )}
    </>
  )
}

export default SelectDatePage
