import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
  startTime: string   // "HH:MM"
  endTime:   string   // "HH:MM"
  savedId:   string | null  // id существующей записи на бэке
  dirty:     boolean        // были ли изменения после загрузки
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
      savedId: null, dirty: false,
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
      [dow]: { ...prev[dow], ...patch, dirty: true },
    }))

  const handleToggle = (dow: number) => {
    const cur = dayStates[dow]
    update(dow, { enabled: !cur.enabled })
  }

  const handleSave = async () => {
    if (saving) return
    setSaving(true)
    try {
      await Promise.all(
        DAYS.map(async ({ dow }) => {
          const d = dayStates[dow]
          if (!d.dirty) return

          if (d.enabled) {
            // Если уже была запись — удаляем и создаём заново (бэк не имеет PUT)
            if (d.savedId) await deleteMySchedule(d.savedId)
            await setMySchedule(dow, d.startTime + ':00', d.endTime + ':00')
          } else {
            if (d.savedId) await deleteMySchedule(d.savedId)
          }
        })
      )
      // Перезагружаем чтобы получить новые id
      const profile = await getMyMasterProfile()
      const schedules = await getMasterSchedules(profile.id)
      setDayStates(prev => {
        const next = Object.fromEntries(
          DAYS.map(d => [d.dow, {
            enabled: false, startTime: DEFAULT_START, endTime: DEFAULT_END,
            savedId: null, dirty: false,
          }])
        )
        schedules.forEach((s: MasterScheduleItem) => {
          const dow = normalizeDayOfWeek(s.dayOfWeek)
          next[dow] = {
            enabled: true,
            startTime: trimSeconds(s.startTime),
            endTime: trimSeconds(s.endTime),
            savedId: s.id,
            dirty: false,
          }
        })
        // Дни, которые выключены — тоже обновляем dirty:false
        DAYS.forEach(({ dow }) => {
          if (!next[dow].enabled) {
            next[dow] = { ...prev[dow], enabled: false, savedId: null, dirty: false }
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
            <div className={styles.logoPlaceholder}>✂</div>
          </div>
          <h1 className={styles.title}>График</h1>
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
