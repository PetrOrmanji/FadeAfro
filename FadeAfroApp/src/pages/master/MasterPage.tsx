import React, { useState, useRef, useEffect } from 'react'
import axios from 'axios'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner, Placeholder } from '@telegram-apps/telegram-ui'
import {
  getMyMasterProfile,
  updateMasterProfile,
  uploadMasterPhoto,
} from '@/api/masters'
import { updateUserName } from '@/api/users'
import { getSchedule, setSchedule, deleteSchedule } from '@/api/schedules'
import { getServices, addService, updateService, deleteService, type ServiceItem } from '@/api/services'
import { type UnavailabilityItem, getUnavailabilities, addUnavailability, deleteUnavailability } from '@/api/unavailabilities'

// ─── Хелпер для ошибок ───────────────────────────────────────────────────────

function showError(error: unknown, fallback: string) {
  const message = axios.isAxiosError(error)
    ? (error.response?.data?.error ?? fallback)
    : fallback
  window.Telegram.WebApp.showAlert(message)
}

// ─── Расписание ───────────────────────────────────────────────────────────────

// Маппинг C# DayOfWeek enum name → number
const DAY_OF_WEEK: Record<string, number> = {
  Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
  Thursday: 4, Friday: 5, Saturday: 6,
}

// C# DayOfWeek: 0=Sun, 1=Mon…6=Sat. Отображаем Пн–Вс.
const DAYS: { dayOfWeek: number; label: string; short: string }[] = [
  { dayOfWeek: 1, label: 'Понедельник', short: 'Пн' },
  { dayOfWeek: 2, label: 'Вторник',     short: 'Вт' },
  { dayOfWeek: 3, label: 'Среда',       short: 'Ср' },
  { dayOfWeek: 4, label: 'Четверг',     short: 'Чт' },
  { dayOfWeek: 5, label: 'Пятница',     short: 'Пт' },
  { dayOfWeek: 6, label: 'Суббота',     short: 'Сб' },
  { dayOfWeek: 0, label: 'Воскресенье', short: 'Вс' },
]

const DEFAULT_START = '09:00'
const DEFAULT_END   = '18:00'

type DayState = {
  enabled: boolean
  startTime: string   // "HH:mm"
  endTime: string     // "HH:mm"
  existingId: string | null
}

type ScheduleState = Record<number, DayState>  // key = dayOfWeek

function buildInitial(schedules: { id: string; dayOfWeek: string; startTime: string; endTime: string }[]): ScheduleState {
  const state: ScheduleState = {}
  for (const { dayOfWeek } of DAYS) {
    const entry = schedules.find(s => DAY_OF_WEEK[s.dayOfWeek] === dayOfWeek)
    state[dayOfWeek] = entry
      ? { enabled: true, startTime: entry.startTime.slice(0, 5), endTime: entry.endTime.slice(0, 5), existingId: entry.id }
      : { enabled: false, startTime: DEFAULT_START, endTime: DEFAULT_END, existingId: null }
  }
  return state
}

function ScheduleBlock() {
  const queryClient = useQueryClient()
  const [days, setDays] = useState<ScheduleState>({})
  const [savingDays, setSavingDays] = useState<Record<number, boolean>>({})
  const [errorDays, setErrorDays] = useState<Record<number, boolean>>({})

  const { data: profile, isLoading: profileLoading } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })

  const masterProfileId = profile?.id ?? ''

  const { data, isLoading, isError } = useQuery({
    queryKey: ['schedule', masterProfileId],
    queryFn: () => getSchedule(masterProfileId),
    enabled: !!masterProfileId,
  })

  useEffect(() => {
    if (data) setDays(buildInitial(data.schedules))
  }, [data])

  function updateDay(dayOfWeek: number, patch: Partial<DayState>) {
    setDays(prev => ({ ...prev, [dayOfWeek]: { ...prev[dayOfWeek], ...patch } }))
  }

  async function handleTimeBlur(dayOfWeek: number) {
    const day = days[dayOfWeek]
    if (!day || !day.enabled || savingDays[dayOfWeek]) return

    setSavingDays(prev => ({ ...prev, [dayOfWeek]: true }))
    try {
      const id = await setSchedule(masterProfileId, dayOfWeek, day.startTime, day.endTime)
      updateDay(dayOfWeek, { existingId: id })
      setErrorDays(prev => ({ ...prev, [dayOfWeek]: false }))
      queryClient.invalidateQueries({ queryKey: ['schedule', masterProfileId] })
    } catch (error) {
      setErrorDays(prev => ({ ...prev, [dayOfWeek]: true }))
      showError(error, 'Не удалось сохранить расписание')
    } finally {
      setSavingDays(prev => ({ ...prev, [dayOfWeek]: false }))
    }
  }

  async function handleToggle(dayOfWeek: number) {
    const day = days[dayOfWeek]
    if (!day || savingDays[dayOfWeek]) return

    const enabling = !day.enabled
    updateDay(dayOfWeek, { enabled: enabling })
    setSavingDays(prev => ({ ...prev, [dayOfWeek]: true }))

    try {
      if (enabling) {
        const id = await setSchedule(masterProfileId, dayOfWeek, day.startTime, day.endTime)
        updateDay(dayOfWeek, { existingId: id })
      } else {
        await deleteSchedule(day.existingId!)
        updateDay(dayOfWeek, { existingId: null })
      }
      setErrorDays(prev => ({ ...prev, [dayOfWeek]: false }))
      queryClient.invalidateQueries({ queryKey: ['schedule', masterProfileId] })
    } catch (error) {
      // Откатываем тогл при ошибке
      updateDay(dayOfWeek, { enabled: !enabling })
      setErrorDays(prev => ({ ...prev, [dayOfWeek]: true }))
      showError(error, 'Не удалось обновить расписание')
    } finally {
      setSavingDays(prev => ({ ...prev, [dayOfWeek]: false }))
    }
  }

  if (profileLoading || isLoading || Object.keys(days).length === 0) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
        <Spinner size="l" />
      </div>
    )
  }

  if (isError) {
    return <Placeholder header="Ошибка" description="Не удалось загрузить расписание" />
  }

  const workingDays = DAYS.filter(d => days[d.dayOfWeek]?.enabled)
  const daysOff     = DAYS.filter(d => !days[d.dayOfWeek]?.enabled)

  const renderToggle = (dayOfWeek: number) => {
    const isSaving = !!savingDays[dayOfWeek]
    const day = days[dayOfWeek]
    return (
      <div
        onClick={() => handleToggle(dayOfWeek)}
        style={{
          width: 40, height: 24, borderRadius: 12,
          background: day?.enabled ? 'var(--tgui--button_color)' : 'var(--tgui--hint_color)',
          position: 'relative',
          cursor: isSaving ? 'default' : 'pointer',
          flexShrink: 0,
          transition: 'background 0.2s',
        }}
      >
        {isSaving ? (
          <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Spinner size="s" />
          </div>
        ) : (
          <div style={{
            position: 'absolute', top: 3,
            left: day?.enabled ? 19 : 3,
            width: 18, height: 18, borderRadius: '50%',
            background: '#fff', transition: 'left 0.2s',
            boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
          }} />
        )}
      </div>
    )
  }

  const renderRow = (dayOfWeek: number, short: string, isLast: boolean) => {
    const day = days[dayOfWeek]
    if (!day) return null
    const isSaving = !!savingDays[dayOfWeek]
    const hasError = !!errorDays[dayOfWeek]
    return (
      <div key={dayOfWeek}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '11px 16px' }}>
          {renderToggle(dayOfWeek)}
          <span style={{
            flex: 1,
            fontSize: 15,
            fontWeight: 400,
            color: hasError ? '#FF3B30' : 'var(--tgui--text_color)',
            transition: 'color 0.15s',
          }}>
            {short}
          </span>
          {day.enabled ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <input
                type="time" value={day.startTime} disabled={isSaving}
                onChange={e => updateDay(dayOfWeek, { startTime: e.target.value })}
                onBlur={() => handleTimeBlur(dayOfWeek)}
                style={{
                  border: 'none', background: 'var(--tgui--secondary_bg_color)',
                  borderRadius: 8, padding: '4px 8px', fontSize: 14,
                  color: 'var(--tgui--text_color)', fontFamily: 'inherit',
                  outline: 'none', cursor: isSaving ? 'default' : 'pointer',
                  opacity: isSaving ? 0.5 : 1,
                }}
              />
              <span style={{ color: 'var(--tgui--hint_color)', fontSize: 13 }}>—</span>
              <input
                type="time" value={day.endTime} disabled={isSaving}
                onChange={e => updateDay(dayOfWeek, { endTime: e.target.value })}
                onBlur={() => handleTimeBlur(dayOfWeek)}
                style={{
                  border: 'none', background: 'var(--tgui--secondary_bg_color)',
                  borderRadius: 8, padding: '4px 8px', fontSize: 14,
                  color: 'var(--tgui--text_color)', fontFamily: 'inherit',
                  outline: 'none', cursor: isSaving ? 'default' : 'pointer',
                  opacity: isSaving ? 0.5 : 1,
                }}
              />
            </div>
          ) : (
            <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>выходной</span>
          )}
        </div>
        {!isLast && <div style={{ height: 1, background: 'var(--tgui--divider)', margin: '0 16px' }} />}
      </div>
    )
  }

  return (
    <div style={{ flex: 1, overflowY: 'auto', padding: '16px', display: 'flex', flexDirection: 'column', gap: 8 }}>

      {workingDays.length > 0 && (
        <div>
          <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--tgui--hint_color)', textTransform: 'uppercase', letterSpacing: 0.5, marginBottom: 6, paddingLeft: 4 }}>
            Рабочие дни
          </div>
          <div style={{ borderRadius: 16, background: 'var(--tgui--bg_color)', overflow: 'hidden' }}>
            {workingDays.map(({ dayOfWeek, label }, idx) =>
              renderRow(dayOfWeek, label, idx === workingDays.length - 1)
            )}
          </div>
        </div>
      )}

      {daysOff.length > 0 && (
        <div>
          <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--tgui--hint_color)', textTransform: 'uppercase', letterSpacing: 0.5, marginBottom: 6, paddingLeft: 4 }}>
            Выходные
          </div>
          <div style={{ borderRadius: 16, background: 'var(--tgui--bg_color)', overflow: 'hidden' }}>
            {daysOff.map(({ dayOfWeek, label }, idx) =>
              renderRow(dayOfWeek, label, idx === daysOff.length - 1)
            )}
          </div>
        </div>
      )}

    </div>
  )
}

// ─── Вкладка Расписание (расписание + отсутствия) ────────────────────────────

// ─── Календарь отсутствий ─────────────────────────────────────────────────────

const WEEK_DAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

const MONTH_NAMES = [
  'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
  'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
]

function toDateKey(date: Date): string {
  return date.toISOString().slice(0, 10)
}

function MonthCalendar({
  year,
  month,
  unavailabilities,
  scheduleByDay,
  onDayPress,
}: {
  year: number
  month: number   // 0-based
  unavailabilities: UnavailabilityItem[]
  scheduleByDay: Record<number, { startTime: string; endTime: string }>  // JS dayOfWeek → times
  onDayPress: (dateKey: string) => void
}) {
  const today = toDateKey(new Date())

  // Собираем словарь date → 'full' | 'partial'
  const markedDays = React.useMemo(() => {
    const map: Record<string, 'full' | 'partial'> = {}
    for (const u of unavailabilities) {
      if (u.startTime === null && u.endTime === null) {
        map[u.date] = 'full'
      } else {
        // Проверяем, совпадает ли промежуток с рабочим расписанием на этот день
        const jsDay = new Date(u.date).getDay()
        const sched = scheduleByDay[jsDay]
        const coversFullDay = sched
          && u.startTime?.slice(0, 5) === sched.startTime.slice(0, 5)
          && u.endTime?.slice(0, 5) === sched.endTime.slice(0, 5)
        map[u.date] = coversFullDay ? 'full' : 'partial'
      }
    }
    return map
  }, [unavailabilities, scheduleByDay])

  // Первый день месяца (0=Sun…6=Sat), переводим в пн-based (0=Mon…6=Sun)
  const firstDay = new Date(year, month, 1)
  const startOffset = (firstDay.getDay() + 6) % 7
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const cells: (number | null)[] = [
    ...Array(startOffset).fill(null),
    ...Array.from({ length: daysInMonth }, (_, i) => i + 1),
  ]
  // Добиваем до кратного 7
  while (cells.length % 7 !== 0) cells.push(null)

  return (
    <div>
      {/* Заголовок месяца */}
      <p style={{ margin: 0, padding: '10px 12px 8px', fontWeight: 600, fontSize: 14, color: 'var(--tgui--text_color)' }}>
        {MONTH_NAMES[month]} {year}
      </p>

      {/* Дни недели */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(7, 1fr)',
        borderTop: '1px solid var(--tgui--divider)',
        borderBottom: '1px solid var(--tgui--divider)',
        borderLeft: '1px solid var(--tgui--divider)',
      }}>
        {WEEK_DAYS.map((d) => {
          return (
            <div key={d} style={{
              textAlign: 'center',
              fontSize: 11,
              fontWeight: 600,
              color: 'var(--tgui--hint_color)',
              padding: '5px 0',
              borderRight: '1px solid var(--tgui--divider)',
            }}>
              {d}
            </div>
          )
        })}
      </div>

      {/* Сетка дней */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(7, 1fr)',
        borderLeft: '1px solid var(--tgui--divider)',
      }}>
        {cells.map((day, idx) => {
          const dateKey = day
            ? `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
            : null
          const mark = dateKey ? markedDays[dateKey] : undefined
          const isToday = dateKey === today
          const isPast = dateKey ? dateKey < today : false

          // Выходной ли день по расписанию мастера
          const jsDay = day ? new Date(year, month, day).getDay() : -1
          const isOffDay = day ? !(jsDay in scheduleByDay) : false

          // Неактивная ячейка — прошлое или выходной
          const isInactive = isPast || isOffDay

          // Фон ячейки
          const isWorkingDay = day ? jsDay in scheduleByDay : false
          let cellBg = 'transparent'
          if (isOffDay) cellBg = 'rgba(52,199,89,0.12)'
          else if (isWorkingDay) cellBg = 'rgba(0,122,255,0.08)'
          if (mark === 'full') cellBg = 'rgba(255,59,48,0.18)'
          else if (mark === 'partial') cellBg = 'rgba(255,149,0,0.18)'

          // Цвет числа
          let color = 'var(--tgui--text_color)'
          if (isToday) color = 'var(--tgui--button_color)'

          return (
            <div
              key={idx}
              onClick={() => dateKey && !isInactive && onDayPress(dateKey)}
              style={{
                position: 'relative',
                minHeight: 40,
                padding: '4px 5px',
                borderRight: '1px solid var(--tgui--divider)',
                borderBottom: '1px solid var(--tgui--divider)',
                background: cellBg,
                cursor: day && !isInactive ? 'pointer' : 'default',
                WebkitTapHighlightColor: 'transparent',
                opacity: isPast ? 0.35 : 1,
                boxSizing: 'border-box',
              }}
            >
              {day ? (
                <span style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  width: 24,
                  height: 24,
                  borderRadius: '50%',
                  border: isToday ? `1.5px solid ${color}` : 'none',
                  fontSize: 12,
                  fontWeight: isToday || mark ? 600 : 400,
                  color,
                  boxSizing: 'border-box',
                }}>
                  {day}
                </span>
              ) : null}
            </div>
          )
        })}
      </div>
    </div>
  )
}

function UnavailabilitiesBlock({
  unavailabilities,
  scheduleByDay,
  onDayPress,
}: {
  unavailabilities: UnavailabilityItem[]
  scheduleByDay: Record<number, { startTime: string; endTime: string }>
  onDayPress: (dateKey: string) => void
}) {
  const now = new Date()
  const thisYear = now.getFullYear()
  const thisMonth = now.getMonth()

  const nextMonth = (thisMonth + 1) % 12
  const nextYear = thisMonth === 11 ? thisYear + 1 : thisYear

  return (
    <div style={{ background: 'var(--tgui--bg_color)' }}>
      {/* Легенда */}
      <div style={{ display: 'flex', gap: 12, padding: '12px 16px 0', flexWrap: 'wrap' }}>
        {[
          { color: 'rgba(0,122,255,0.35)',   label: 'Рабочий день' },
          { color: 'rgba(52,199,89,0.5)',    label: 'Выходной' },
          { color: 'rgba(255,149,0,0.7)',    label: 'Часть дня' },
          { color: 'rgba(255,59,48,0.7)',    label: 'Весь день' },
        ].map(({ color, label }) => (
          <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
            <div style={{ width: 10, height: 10, borderRadius: 2, background: color, flexShrink: 0 }} />
            <span style={{ fontSize: 11, color: 'var(--tgui--hint_color)' }}>{label}</span>
          </div>
        ))}
      </div>

      <MonthCalendar year={thisYear} month={thisMonth} unavailabilities={unavailabilities} scheduleByDay={scheduleByDay} onDayPress={onDayPress} />

      <div style={{ height: 8, background: 'var(--tgui--secondary_bg_color)', borderTop: '1px solid var(--tgui--divider)', borderBottom: '1px solid var(--tgui--divider)' }} />

      <MonthCalendar year={nextYear} month={nextMonth} unavailabilities={unavailabilities} scheduleByDay={scheduleByDay} onDayPress={onDayPress} />
    </div>
  )
}

// ─── Боттом-шит отсутствия ────────────────────────────────────────────────────

function formatDateLabel(dateKey: string): string {
  const [y, m, d] = dateKey.split('-').map(Number)
  const date = new Date(y, m - 1, d)
  return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', weekday: 'short' })
}

interface UnavailabilitySheetProps {
  dateKey: string
  existing: UnavailabilityItem | undefined
  masterProfileId: string
  onClose: () => void
}

function UnavailabilitySheet({ dateKey, existing, masterProfileId, onClose }: UnavailabilitySheetProps) {
  const queryClient = useQueryClient()
  const [allDay, setAllDay] = useState<boolean>(existing ? existing.startTime === null : true)
  const [startTime, setStartTime] = useState(existing?.startTime?.slice(0, 5) ?? '09:00')
  const [endTime, setEndTime] = useState(existing?.endTime?.slice(0, 5) ?? '18:00')
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    document.body.style.overflow = 'hidden'
    return () => { document.body.style.overflow = '' }
  }, [])

  async function handleSave() {
    setSaving(true)
    try {
      await addUnavailability(
        masterProfileId,
        dateKey,
        allDay ? null : `${startTime}:00`,
        allDay ? null : `${endTime}:00`,
      )
      queryClient.invalidateQueries({ queryKey: ['unavailabilities', masterProfileId] })
      onClose()
    } catch (e) {
      showError(e, 'Не удалось сохранить отсутствие')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!existing) return
    setDeleting(true)
    try {
      await deleteUnavailability(existing.id)
      queryClient.invalidateQueries({ queryKey: ['unavailabilities', masterProfileId] })
      onClose()
    } catch (e) {
      showError(e, 'Не удалось удалить отсутствие')
    } finally {
      setDeleting(false)
    }
  }

  const busy = saving || deleting

  return (
    <>
      <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)', zIndex: 100 }} />
      <div style={{
        position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 101,
        background: 'var(--tgui--bg_color)',
        borderRadius: '16px 16px 0 0',
        padding: '20px 16px 32px',
      }}>
        {/* Handle */}
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 16 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--tgui--divider)' }} />
        </div>

        <p style={{ margin: '0 0 4px', fontWeight: 600, fontSize: 17 }}>Отсутствие</p>
        <p style={{ margin: '0 0 20px', fontSize: 13, color: 'var(--tgui--hint_color)' }}>
          {formatDateLabel(dateKey)}
        </p>

        {/* Переключатель весь день / промежуток */}
        <div style={{
          display: 'flex', marginBottom: 20,
          background: 'var(--tgui--secondary_bg_color)',
          borderRadius: 10, padding: 3,
        }}>
          {(['all', 'partial'] as const).map(mode => {
            const active = mode === 'all' ? allDay : !allDay
            return (
              <button
                key={mode}
                onClick={() => setAllDay(mode === 'all')}
                style={{
                  flex: 1, padding: '7px 0', border: 'none', borderRadius: 8,
                  background: active ? 'var(--tgui--bg_color)' : 'transparent',
                  color: active ? 'var(--tgui--text_color)' : 'var(--tgui--hint_color)',
                  fontWeight: active ? 600 : 400, fontSize: 14,
                  cursor: 'pointer',
                  boxShadow: active ? '0 1px 3px rgba(0,0,0,0.1)' : 'none',
                  transition: 'all 0.15s',
                }}
              >
                {mode === 'all' ? 'Весь день' : 'Промежуток'}
              </button>
            )
          })}
        </div>

        {/* Поля времени */}
        {!allDay && (
          <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
            {[
              { label: 'С', value: startTime, onChange: setStartTime },
              { label: 'До', value: endTime, onChange: setEndTime },
            ].map(({ label, value, onChange }) => (
              <div key={label} style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 6 }}>
                <label style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{label}</label>
                <input
                  type="time" value={value}
                  onChange={e => onChange(e.target.value)}
                  style={{
                    padding: '10px 12px', borderRadius: 10,
                    border: '1px solid var(--tgui--divider)',
                    background: 'var(--tgui--secondary_bg_color)',
                    color: 'var(--tgui--text_color)',
                    fontSize: 15, outline: 'none', width: '100%', boxSizing: 'border-box',
                    fontFamily: 'inherit',
                  }}
                />
              </div>
            ))}
          </div>
        )}

        {/* Кнопки */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <button
            onClick={handleSave} disabled={busy}
            style={{
              width: '100%', padding: '13px 0', borderRadius: 12, border: 'none',
              background: busy ? 'var(--tgui--divider)' : 'var(--tgui--button_color)',
              color: busy ? 'var(--tgui--hint_color)' : '#fff',
              fontSize: 15, fontWeight: 600,
              cursor: busy ? 'not-allowed' : 'pointer',
            }}
          >
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>

          {existing && (
            <button
              onClick={handleDelete} disabled={busy}
              style={{
                width: '100%', padding: '13px 0', borderRadius: 12, border: 'none',
                background: 'transparent',
                color: busy ? 'var(--tgui--hint_color)' : '#FF3B30',
                fontSize: 15, fontWeight: 500,
                cursor: busy ? 'not-allowed' : 'pointer',
              }}
            >
              {deleting ? 'Удаление...' : 'Удалить отсутствие'}
            </button>
          )}
        </div>
      </div>
    </>
  )
}

// ─── Секции вкладки Расписание ────────────────────────────────────────────────

const sectionCardStyle: React.CSSProperties = {
  margin: '0 16px',
  border: '1px solid var(--tgui--divider)',
  borderRadius: 16,
  overflow: 'hidden',
}

const sectionTitleStyle: React.CSSProperties = {
  margin: 0,
  padding: '14px 16px',
  fontWeight: 600,
  fontSize: 15,
  borderBottom: '1px solid var(--tgui--divider)',
  background: 'var(--tgui--bg_color)',
}

function ScheduleTab() {
  const [selectedDate, setSelectedDate] = useState<string | null>(null)

  const { data: profile } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })
  const masterProfileId = profile?.id ?? ''

  const { data: unavailabilities = [] } = useQuery({
    queryKey: ['unavailabilities', masterProfileId],
    queryFn: () => getUnavailabilities(masterProfileId),
    enabled: !!masterProfileId,
  })

  const { data: scheduleData } = useQuery({
    queryKey: ['schedule', masterProfileId],
    queryFn: () => getSchedule(masterProfileId),
    enabled: !!masterProfileId,
  })

  // Map JS dayOfWeek (0=Sun…6=Sat) → { startTime, endTime } из расписания
  const scheduleByDay = React.useMemo(() => {
    const map: Record<number, { startTime: string; endTime: string }> = {}
    for (const s of scheduleData?.schedules ?? []) {
      map[DAY_OF_WEEK[s.dayOfWeek]] = { startTime: s.startTime, endTime: s.endTime }
    }
    return map
  }, [scheduleData])

  const existing = selectedDate
    ? unavailabilities.find(u => u.date === selectedDate)
    : undefined

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16, padding: '16px 0 32px' }}>
      {/* Блок 1 — Расписание */}
      <section style={sectionCardStyle}>
        <p style={sectionTitleStyle}>Расписание</p>
        <ScheduleBlock />
      </section>

      {/* Блок 2 — Отсутствия */}
      <section style={sectionCardStyle}>
        <p style={sectionTitleStyle}>Отсутствия</p>
        <UnavailabilitiesBlock
          unavailabilities={unavailabilities}
          scheduleByDay={scheduleByDay}
          onDayPress={setSelectedDate}
        />
      </section>

      {selectedDate && masterProfileId && (
        <UnavailabilitySheet
          dateKey={selectedDate}
          existing={existing}
          masterProfileId={masterProfileId}
          onClose={() => setSelectedDate(null)}
        />
      )}
    </div>
  )
}

// ─── Услуги ───────────────────────────────────────────────────────────────────

function formatDuration(duration: string): string {
  const [h, m] = duration.split(':').map(Number)
  if (h === 0) return `${m} мин`
  if (m === 0) return `${h} ч`
  return `${h} ч ${m} мин`
}

function ServicesTab({ onEdit }: { onEdit: (service: ServiceItem | null) => void }) {
  const [hoveredId, setHoveredId] = useState<string | null>(null)

  const { data: profile, isLoading: profileLoading } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })

  const masterProfileId = profile?.id ?? ''

  const { data, isLoading, isError } = useQuery({
    queryKey: ['services', masterProfileId],
    queryFn: () => getServices(masterProfileId),
    enabled: !!masterProfileId,
  })

  if (profileLoading || isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
        <Spinner size="l" />
      </div>
    )
  }

  if (isError) {
    return <Placeholder header="Ошибка" description="Не удалось загрузить услуги" />
  }

  const services = data?.services ?? []

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

      {/* Список */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '16px 16px 0' }}>
        {services.length === 0 ? (
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
            <Placeholder
              header="Нет услуг"
              description="Добавьте первую услугу"
            />
          </div>
        ) : (
          <div style={{ borderRadius: 16, background: 'var(--tgui--bg_color)', overflow: 'hidden' }}>
            {services.map((service, idx) => (
              <div key={service.id}>
                {idx > 0 && <div style={{ height: 1, background: 'var(--tgui--divider)', margin: '0 16px' }} />}
                <div
                  onClick={() => onEdit(service)}
                  onMouseEnter={() => setHoveredId(service.id)}
                  onMouseLeave={() => setHoveredId(null)}
                  style={{
                    display: 'flex', alignItems: 'center', padding: '12px 16px', gap: 12,
                    cursor: 'pointer',
                    background: hoveredId === service.id ? 'var(--tgui--secondary_bg_color)' : 'transparent',
                    transition: 'background 0.15s',
                  }}
                >
                  {/* Инфо */}
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontSize: 15, color: 'var(--tgui--text_color)', fontWeight: 500 }}>
                      {service.name}
                    </div>
                    <div style={{ fontSize: 13, color: 'var(--tgui--hint_color)', marginTop: 2 }}>
                      {formatDuration(service.duration)}
                      {service.description && <span> · {service.description}</span>}
                    </div>
                  </div>

                  {/* Цена */}
                  <div style={{ fontSize: 15, fontWeight: 600, color: 'var(--tgui--text_color)', flexShrink: 0 }}>
                    {service.price} ₽
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Кнопка добавить */}
      <div style={{ padding: '12px 16px 20px', flexShrink: 0 }}>
        <button
          onClick={() => onEdit(null)}
          style={{
            width: '100%',
            padding: '14px 0',
            borderRadius: 14,
            border: 'none',
            fontSize: 15,
            fontWeight: 600,
            cursor: 'pointer',
            background: 'var(--tgui--button_color)',
            color: '#fff',
            boxShadow: '0 2px 8px rgba(51,144,236,0.3)',
          }}
        >
          + Добавить услугу
        </button>
      </div>

    </div>
  )
}

// ─── Форма услуги ─────────────────────────────────────────────────────────────

const DURATION_OPTIONS = [
  { value: '00:15:00', label: '15 мин' },
  { value: '00:30:00', label: '30 мин' },
  { value: '00:45:00', label: '45 мин' },
  { value: '01:00:00', label: '1 ч' },
  { value: '01:30:00', label: '1 ч 30 мин' },
  { value: '02:00:00', label: '2 ч' },
]

function ServiceForm({
  service,
  masterProfileId,
  onClose,
}: {
  service: ServiceItem | null
  masterProfileId: string
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const isEditing = service !== null

  const [name, setName] = useState(service?.name ?? '')
  const [description, setDescription] = useState(service?.description ?? '')
  const [price, setPrice] = useState(service ? String(service.price) : '')
  const [duration, setDuration] = useState(
    service ? service.duration.slice(0, 8) : '00:30:00'
  )
  const [saving, setSaving] = useState(false)

  const inputStyle: React.CSSProperties = {
    width: '100%',
    background: 'var(--tgui--secondary_bg_color)',
    border: '1px solid var(--tgui--divider)',
    borderRadius: 10,
    padding: '11px 14px',
    fontSize: 15,
    color: 'var(--tgui--text_color)',
    fontFamily: 'inherit',
    outline: 'none',
    boxSizing: 'border-box',
  }

  async function handleSave() {
    if (!name.trim()) {
      window.Telegram.WebApp.showAlert('Введите название услуги')
      return
    }
    const priceNum = parseInt(price)
    if (!price || isNaN(priceNum) || priceNum <= 0) {
      window.Telegram.WebApp.showAlert('Введите корректную цену')
      return
    }

    setSaving(true)
    try {
      const payload = {
        name: name.trim(),
        description: description.trim() || null,
        price: priceNum,
        duration,
      }
      if (isEditing) {
        await updateService(service.id, payload)
      } else {
        await addService(masterProfileId, payload)
      }
      queryClient.invalidateQueries({ queryKey: ['services', masterProfileId] })
      onClose()
    } catch (error) {
      showError(error, 'Не удалось сохранить услугу')
    } finally {
      setSaving(false)
    }
  }

  function handleDelete() {
    window.Telegram.WebApp.showConfirm(
      `Удалить услугу «${service!.name}»?`,
      async (confirmed) => {
        if (!confirmed) return
        setSaving(true)
        try {
          await deleteService(service!.id)
          queryClient.invalidateQueries({ queryKey: ['services', masterProfileId] })
          onClose()
        } catch (error) {
          showError(error, 'Не удалось удалить услугу')
        } finally {
          setSaving(false)
        }
      }
    )
  }

  return (
    <>
      {/* Затемнение */}
      <div
        onClick={onClose}
        style={{
          position: 'fixed', inset: 0,
          background: 'rgba(0,0,0,0.5)',
          zIndex: 100,
        }}
      />

      {/* Боттом-шит */}
      <div style={{
        position: 'fixed', bottom: 0, left: 0, right: 0,
        background: 'var(--tgui--secondary_bg_color)',
        borderRadius: '20px 20px 0 0',
        padding: '20px 16px 32px',
        zIndex: 101,
        display: 'flex',
        flexDirection: 'column',
        gap: 12,
      }}>

        {/* Шапка */}
        <div style={{ display: 'flex', alignItems: 'center', marginBottom: 4 }}>
          <span style={{ flex: 1, fontSize: 17, fontWeight: 600, color: 'var(--tgui--text_color)' }}>
            {isEditing ? 'Редактировать услугу' : 'Новая услуга'}
          </span>
          <button onClick={onClose} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: 22, color: 'var(--tgui--hint_color)', padding: 0 }}>
            ✕
          </button>
        </div>

        {/* Название */}
        <div>
          <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)', marginBottom: 6, paddingLeft: 4 }}>Название</div>
          <input
            placeholder="Например: Стрижка"
            value={name}
            onChange={e => setName(e.target.value)}
            style={inputStyle}
          />
        </div>

        {/* Описание */}
        <div>
          <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)', marginBottom: 6, paddingLeft: 4 }}>Описание <span style={{ opacity: 0.6 }}>(необязательно)</span></div>
          <textarea
            placeholder="Например: для длинных волос"
            value={description}
            rows={1}
            onChange={e => setDescription(e.target.value)}
            onInput={e => {
              const el = e.currentTarget
              el.style.height = 'auto'
              el.style.height = `${el.scrollHeight}px`
            }}
            style={{
              ...inputStyle,
              resize: 'none',
              overflow: 'hidden',
              lineHeight: '1.5',
            }}
          />
        </div>

        {/* Цена и длительность */}
        <div style={{ display: 'flex', gap: 12 }}>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)', marginBottom: 6, paddingLeft: 4 }}>Цена, ₽</div>
            <input
              placeholder="0"
              value={price}
              onChange={e => setPrice(e.target.value.replace(/\D/g, ''))}
              inputMode="numeric"
              style={inputStyle}
            />
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)', marginBottom: 6, paddingLeft: 4 }}>Длительность</div>
            <select
              value={duration}
              onChange={e => setDuration(e.target.value)}
              style={{ ...inputStyle, cursor: 'pointer', appearance: 'none', textAlign: 'center' }}
            >
              {DURATION_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Кнопки */}
        <div style={{ display: 'flex', gap: 8, marginTop: 4 }}>
          {isEditing && (
            <button
              onClick={handleDelete}
              disabled={saving}
              style={{
                flex: 1, padding: '13px 0', borderRadius: 12, border: 'none',
                background: 'rgba(255,59,48,0.12)', color: '#FF3B30',
                fontSize: 15, fontWeight: 600, cursor: saving ? 'default' : 'pointer',
              }}
            >
              Удалить
            </button>
          )}
          <button
            onClick={handleSave}
            disabled={saving}
            style={{
              flex: 2, padding: '13px 0', borderRadius: 12, border: 'none',
              background: saving ? 'var(--tgui--hint_color)' : 'var(--tgui--button_color)',
              color: '#fff', fontSize: 15, fontWeight: 600,
              cursor: saving ? 'default' : 'pointer',
              boxShadow: saving ? 'none' : '0 2px 8px rgba(51,144,236,0.3)',
            }}
          >
            {saving ? 'Сохраняем...' : 'Сохранить'}
          </button>
        </div>

      </div>
    </>
  )
}

// ─── Заглушка ────────────────────────────────────────────────────────────────

function StubTab({ label }: { label: string }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
      <Placeholder header={label} description="Раздел в разработке" />
    </div>
  )
}

// ─── Профиль ──────────────────────────────────────────────────────────────────

function ProfileTab() {
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { data: profile, isLoading, isError, dataUpdatedAt } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })

  const [firstName, setFirstName] = useState<string>('')
  const [lastName, setLastName] = useState<string>('')
  const [description, setDescription] = useState<string>('')
  const [fieldsReady, setFieldsReady] = useState(false)
  const [photoError, setPhotoError] = useState(false)
  const [photoLoading, setPhotoLoading] = useState(false)

  // Инициализируем поля когда профиль загрузился
  if (profile && !fieldsReady) {
    setFirstName(profile.firstName)
    setLastName(profile.lastName ?? '')
    setDescription(profile.description ?? '')
    setFieldsReady(true)
  }

  const nameMutation = useMutation({
    mutationFn: () => updateUserName(firstName, lastName || null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-master-profile'] }),
    onError: (error) => showError(error, 'Не удалось сохранить имя'),
  })

  const updateMutation = useMutation({
    mutationFn: () => updateMasterProfile(profile!.id, description || null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-master-profile'] }),
    onError: (error) => showError(error, 'Не удалось сохранить описание'),
  })

  const photoMutation = useMutation({
    mutationFn: (file: File) => uploadMasterPhoto(profile!.id, file),
    onSuccess: () => {
      setPhotoError(false)
      setPhotoLoading(true)
      queryClient.invalidateQueries({ queryKey: ['my-master-profile'] })
    },
    onError: (error) => showError(error, 'Не удалось загрузить фото'),
  })

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
        <Spinner size="l" />
      </div>
    )
  }

  if (isError || !profile) {
    return <Placeholder header="Ошибка" description="Не удалось загрузить профиль" />
  }

  const displayFirstName = firstName || profile.firstName
  const photoSrc = profile.photoUrl && !photoError
    ? `${import.meta.env.VITE_API_URL ?? ''}${profile.photoUrl}?v=${dataUpdatedAt}`
    : null

  const nameChanged =
    firstName !== profile.firstName ||
    lastName !== (profile.lastName ?? '')

  return (
    <div style={{ padding: '24px 16px', display: 'flex', flexDirection: 'column', gap: 24 }}>

      {/* Фото */}
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
        <div
          onClick={() => fileInputRef.current?.click()}
          style={{
            width: 96,
            height: 96,
            borderRadius: '50%',
            background: '#3390EC',
            overflow: 'hidden',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            position: 'relative',
            flexShrink: 0,
          }}
        >
          {photoSrc && (
            <img
              src={photoSrc}
              alt={displayFirstName}
              onLoad={() => setPhotoLoading(false)}
              onError={() => { setPhotoError(true); setPhotoLoading(false) }}
              style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover' }}
            />
          )}
          <span style={{ color: '#fff', fontWeight: 700, fontSize: 32 }}>
            {(displayFirstName[0] ?? '') + (lastName?.[0] ?? '')}
          </span>

          {/* Оверлей при загрузке */}
          {(photoMutation.isPending || photoLoading) && (
            <div style={{
              position: 'absolute',
              inset: 0,
              background: 'rgba(0,0,0,0.45)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <Spinner size="m" />
            </div>
          )}
        </div>

        <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)', cursor: 'pointer' }}
          onClick={() => fileInputRef.current?.click()}>
          {photoSrc ? 'Изменить фото' : 'Добавить фото'}
        </span>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          style={{ display: 'none' }}
          onChange={e => {
            const file = e.target.files?.[0]
            if (file) photoMutation.mutate(file)
            e.target.value = ''
          }}
        />
      </div>

      {/* Имя */}
      <div style={{
        borderRadius: 16,
        background: 'var(--tgui--bg_color)',
        padding: '14px 16px',
        display: 'flex',
        flexDirection: 'column',
        gap: 8,
      }}>
        <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)' }}>Имя</div>
        <input
          value={firstName}
          onChange={e => setFirstName(e.target.value)}
          placeholder="Имя"
          style={{
            background: 'transparent',
            border: 'none',
            outline: 'none',
            fontSize: 15,
            color: 'var(--tgui--text_color)',
            fontFamily: 'inherit',
            width: '100%',
            boxSizing: 'border-box',
          }}
        />
        <div style={{ height: 1, background: 'var(--tgui--divider)', margin: '0 0' }} />
        <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)' }}>Фамилия</div>
        <input
          value={lastName}
          onChange={e => setLastName(e.target.value)}
          placeholder="Фамилия (необязательно)"
          style={{
            background: 'transparent',
            border: 'none',
            outline: 'none',
            fontSize: 15,
            color: 'var(--tgui--text_color)',
            fontFamily: 'inherit',
            width: '100%',
            boxSizing: 'border-box',
          }}
        />
        <button
          onClick={() => nameMutation.mutate()}
          disabled={nameMutation.isPending || !firstName.trim() || !nameChanged}
          style={{
            alignSelf: 'flex-end',
            padding: '8px 20px',
            borderRadius: 20,
            border: 'none',
            fontSize: 14,
            fontWeight: 600,
            cursor: nameMutation.isPending || !firstName.trim() || !nameChanged ? 'default' : 'pointer',
            background: nameMutation.isPending || !firstName.trim() || !nameChanged
              ? 'var(--tgui--secondary_bg_color)'
              : 'var(--tgui--button_color)',
            color: nameMutation.isPending || !firstName.trim() || !nameChanged
              ? 'var(--tgui--hint_color)'
              : '#fff',
            transition: 'background 0.15s, color 0.15s',
          }}
        >
          {nameMutation.isPending ? 'Сохраняем...' : 'Сохранить'}
        </button>
      </div>

      {/* Описание */}
      <div style={{
        borderRadius: 16,
        background: 'var(--tgui--bg_color)',
        padding: '14px 16px',
        display: 'flex',
        flexDirection: 'column',
        gap: 8,
      }}>
        <div style={{ fontSize: 12, color: 'var(--tgui--hint_color)' }}>О себе</div>
        <textarea
          value={description}
          onChange={e => setDescription(e.target.value)}
          placeholder="Расскажите о себе, опыте, специализации..."
          rows={4}
          style={{
            background: 'transparent',
            border: 'none',
            outline: 'none',
            resize: 'none',
            fontSize: 15,
            color: 'var(--tgui--text_color)',
            fontFamily: 'inherit',
            lineHeight: 1.5,
            width: '100%',
            boxSizing: 'border-box',
          }}
        />
        <button
          onClick={() => updateMutation.mutate()}
          disabled={updateMutation.isPending || description === (profile.description ?? '')}
          style={{
            alignSelf: 'flex-end',
            padding: '8px 20px',
            borderRadius: 20,
            border: 'none',
            fontSize: 14,
            fontWeight: 600,
            cursor: updateMutation.isPending || description === (profile.description ?? '') ? 'default' : 'pointer',
            background: updateMutation.isPending || description === (profile.description ?? '')
              ? 'var(--tgui--secondary_bg_color)'
              : 'var(--tgui--button_color)',
            color: updateMutation.isPending || description === (profile.description ?? '')
              ? 'var(--tgui--hint_color)'
              : '#fff',
            transition: 'background 0.15s, color 0.15s',
          }}
        >
          {updateMutation.isPending ? 'Сохраняем...' : 'Сохранить'}
        </button>
      </div>

    </div>
  )
}

// ─── Страница ─────────────────────────────────────────────────────────────────

type Tab = 'profile' | 'schedule' | 'services' | 'appointments'

const TABS: { key: Tab; label: string; icon: string }[] = [
  { key: 'profile',      label: 'Профиль',    icon: '👤' },
  { key: 'schedule',     label: 'Расписание', icon: '📅' },
  { key: 'services',     label: 'Услуги',     icon: '✂️' },
  { key: 'appointments', label: 'Записи',     icon: '📋' },
]

export function MasterPage() {
  const [activeTab, setActiveTab] = useState<Tab>('profile')
  const [hoveredTab, setHoveredTab] = useState<Tab | null>(null)
  const [editingService, setEditingService] = useState<ServiceItem | null | undefined>(undefined)
  // undefined = форма закрыта, null = новая услуга, ServiceItem = редактирование

  const { data: profile } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })
  const masterProfileId = profile?.id ?? ''

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div style={{ flex: 1, overflowY: 'auto', minHeight: 0 }}>
        {activeTab === 'profile'      && <ProfileTab />}
        {activeTab === 'schedule'     && <ScheduleTab />}
        {activeTab === 'services'     && <ServicesTab onEdit={s => setEditingService(s)} />}
        {activeTab === 'appointments' && <StubTab label="Записи" />}
      </div>

      {editingService !== undefined && (
        <ServiceForm
          service={editingService}
          masterProfileId={masterProfileId}
          onClose={() => setEditingService(undefined)}
        />
      )}

      <nav style={{
        display: 'flex',
        borderTop: '1px solid var(--tgui--divider)',
        background: 'var(--tgui--secondary_bg_color)',
        flexShrink: 0,
      }}>
        {TABS.map(tab => {
          const isActive = activeTab === tab.key
          const isHovered = hoveredTab === tab.key
          return (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              onMouseEnter={() => setHoveredTab(tab.key)}
              onMouseLeave={() => setHoveredTab(null)}
              style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                gap: 4,
                padding: '10px 0 14px',
                border: 'none',
                background: isActive
                  ? 'rgba(51, 144, 236, 0.12)'
                  : isHovered
                    ? 'var(--tgui--bg_color)'
                    : 'transparent',
                cursor: 'pointer',
                color: isActive
                  ? 'var(--tgui--button_color)'
                  : isHovered
                    ? 'var(--tgui--text_color)'
                    : 'var(--tgui--hint_color)',
                fontSize: 10,
                fontWeight: isActive ? 600 : 400,
                transition: 'color 0.15s, background 0.15s',
              }}
            >
              <span style={{ fontSize: 22 }}>{tab.icon}</span>
              {tab.label}
            </button>
          )
        })}
      </nav>
    </div>
  )
}
