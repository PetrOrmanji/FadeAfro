import logo from '../../assets/logo.png'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { isRateLimitError } from '../../api/errors'
import {
  getMasterSchedules,
  setMySchedule,
  deleteMySchedule,
  normalizeDayOfWeek,
  type MasterScheduleItem,
} from '../../api/schedule'
import { getMyMasterProfile } from '../../api/masters'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './MasterSchedulePage.module.css'

// ── Константы ──────────────────────────────────────────────────────────────

const DAYS = [
  { dow: 1, label: 'Понедельник', short: 'Пн' },
  { dow: 2, label: 'Вторник',     short: 'Вт' },
  { dow: 3, label: 'Среда',       short: 'Ср' },
  { dow: 4, label: 'Четверг',     short: 'Чт' },
  { dow: 5, label: 'Пятница',     short: 'Пт' },
  { dow: 6, label: 'Суббота',     short: 'Сб' },
  { dow: 0, label: 'Воскресенье', short: 'Вс' },
]

const DEFAULT_START = '09:00'
const DEFAULT_END   = '18:00'

// ── Типы ───────────────────────────────────────────────────────────────────

interface DayState {
  enabled:   boolean
  startTime: string        // "HH:MM"
  endTime:   string        // "HH:MM"
  savedId:   string | null // id существующей записи на бэке
  dirty:     boolean       // были ли изменения после загрузки
  error:     string | null // inline-ошибка от бэка
}

// ── Утилиты ────────────────────────────────────────────────────────────────

/** "HH:MM:SS" → "HH:MM" */
const trimSeconds = (t: string) => t.slice(0, 5)

// ── Компонент ──────────────────────────────────────────────────────────────

const MasterSchedulePage = () => {
  useBackButton()
  const navigate = useNavigate()

  const [dayStates, setDayStates] = useState<Record<number, DayState>>(() =>
    Object.fromEntries(DAYS.map(d => [d.dow, {
      enabled: false, startTime: DEFAULT_START, endTime: DEFAULT_END,
      savedId: null, dirty: false, error: null,
    }]))
  )
  const [loading,  setLoading]  = useState(true)
  const [saving,   setSaving]   = useState(false)

  // Загрузка расписания
  useEffect(() => {
    ;(async () => {
      try {
        const profile = await getMyMasterProfile()
        const schedules = await getMasterSchedules(profile.id)

        setDayStates(prev => {
          const next = { ...prev }
          schedules.forEach((s: MasterScheduleItem) => {
            const dow = normalizeDayOfWeek(s.dayOfWeek)
            next[dow] = {
              enabled:   true,
              startTime: trimSeconds(s.startTime),
              endTime:   trimSeconds(s.endTime),
              savedId:   s.id,
              dirty:     false,
              error:     null,
            }
          })
          return next
        })
      } catch {
        navigate('/error', { replace: true })
      } finally {
        setLoading(false)
      }
    })()
  }, [navigate])

  const hasDirty = Object.values(dayStates).some(d => d.dirty)

  const update = (dow: number, patch: Partial<DayState>) =>
    setDayStates(prev => ({
      ...prev,
      [dow]: { ...prev[dow], ...patch, dirty: true, error: null },
    }))

  const handleToggle = (dow: number) => {
    const cur = dayStates[dow]
    update(dow, { enabled: !cur.enabled })
  }

  const handleSave = async () => {
    if (saving) return
    setSaving(true)

    // Сбрасываем старые ошибки
    setDayStates(prev => {
      const next = { ...prev }
      DAYS.forEach(({ dow }) => { next[dow] = { ...next[dow], error: null } })
      return next
    })

    // Сохраняем результат каждого дня независимо
    const results = await Promise.all(
      DAYS.map(async ({ dow }) => {
        const d = dayStates[dow]
        if (!d.dirty) return { dow, ok: true }
        try {
          if (d.enabled) {
            await setMySchedule(dow, d.startTime + ':00', d.endTime + ':00')
          } else {
            if (d.savedId) await deleteMySchedule(d.savedId)
          }
          return { dow, ok: true }
        } catch (e: unknown) {
          if (isRateLimitError(e)) return { dow, ok: false, fatal: false, msg: 'Слишком много запросов. Попробуйте через минуту.' }
          const status = (e as { response?: { status?: number } })?.response?.status
          if (!status || status >= 500) return { dow, ok: false, fatal: true }
          const msg = (e as { response?: { data?: { error?: string } } })?.response?.data?.error
          return { dow, ok: false, fatal: false, msg: msg ?? 'Ошибка' }
        }
      })
    )

    // Если есть фатальная ошибка — на страницу ошибки
    if (results.some(r => !r.ok && r.fatal)) {
      navigate('/error', { replace: true })
      return
    }

    // Показываем inline-ошибки
    const hasInlineErrors = results.some(r => !r.ok && !r.fatal)
    if (hasInlineErrors) {
      setDayStates(prev => {
        const next = { ...prev }
        results.forEach(r => {
          if (!r.ok && !r.fatal) next[r.dow] = { ...next[r.dow], error: r.msg ?? 'Ошибка', dirty: true }
        })
        return next
      })
    }

    // Перезагружаем расписание с бэка
    try {
      const profile = await getMyMasterProfile()
      const schedules = await getMasterSchedules(profile.id)
      setDayStates(prev => {
        const next: Record<number, DayState> = Object.fromEntries(
          DAYS.map(d => [d.dow, {
            enabled: false, startTime: DEFAULT_START, endTime: DEFAULT_END,
            savedId: null, dirty: false, error: prev[d.dow].error,
          }])
        )
        schedules.forEach((s: MasterScheduleItem) => {
          const dow = normalizeDayOfWeek(s.dayOfWeek)
          next[dow] = {
            enabled:   true,
            startTime: trimSeconds(s.startTime),
            endTime:   trimSeconds(s.endTime),
            savedId:   s.id,
            dirty:     !!prev[dow].error, // оставляем dirty если была ошибка
            error:     prev[dow].error,
          }
        })
        return next
      })
    } catch {
      navigate('/error', { replace: true })
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <LoadingScreen />

  return (
    <>
      <div className={styles.page}>

        {/* Шапка — не скроллится */}
        <div className={styles.header}>
          <div className={styles.logoWrap}>
            <img src={logo} className={styles.logo} alt="FadeAfro" />
          </div>
          <h1 className={styles.title}>График работы</h1>
        </div>

        {/* Список дней — скроллится */}
        <div className={`${styles.list} ${hasDirty ? styles.listWithPanel : ''}`}>
          {DAYS.map(({ dow, label }) => {
            const d = dayStates[dow]
            return (
              <div key={dow} className={styles.card}>

                {/* Строка: название + тоггл */}
                <div className={styles.cardRow}>
                  <span className={`${styles.dayLabel} ${d.enabled ? styles.dayLabelActive : ''}`}>
                    {label}
                  </span>
                  <button
                    className={`${styles.toggle} ${d.enabled ? styles.toggleOn : ''}`}
                    onClick={() => handleToggle(dow)}
                    aria-label={d.enabled ? 'Выключить' : 'Включить'}
                  >
                    <span className={styles.toggleThumb} />
                  </button>
                </div>

                {/* Время — появляется когда включён */}
                {d.enabled && (
                  <div className={styles.timeRow}>
                    <div className={styles.timeGroup}>
                      <span className={styles.timeLabel}>Начало</span>
                      <input
                        type="time"
                        className={styles.timeInput}
                        value={d.startTime}
                        onChange={e => update(dow, { startTime: e.target.value })}
                      />
                    </div>
                    <span className={styles.timeSep}>—</span>
                    <div className={styles.timeGroup}>
                      <span className={styles.timeLabel}>Конец</span>
                      <input
                        type="time"
                        className={styles.timeInput}
                        value={d.endTime}
                        onChange={e => update(dow, { endTime: e.target.value })}
                      />
                    </div>
                  </div>
                )}

                {/* Inline-ошибка */}
                {d.error && (
                  <p className={styles.errorText}>{d.error}</p>
                )}
              </div>
            )
          })}
        </div>

      </div>

      {/* Кнопка сохранения */}
      {hasDirty && (
        <div className={styles.bottomPanel}>
          <button
            className={styles.saveBtn}
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      )}
    </>
  )
}

export default MasterSchedulePage
