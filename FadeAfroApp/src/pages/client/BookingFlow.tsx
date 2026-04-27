import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner } from '@telegram-apps/telegram-ui'
import { getMasters, type MasterProfile } from '@/api/masters'
import { getServices, type ServiceItem } from '@/api/services'
import { getSchedule } from '@/api/schedules'
import { getAvailableSlots } from '@/api/slots'
import { createAppointment } from '@/api/appointments'

// ─── Типы ─────────────────────────────────────────────────────────────────────

export interface BookingState {
  masterId: string | null
  masterName: string | null
  serviceId: string | null
  serviceName: string | null
  servicePrice: number | null
  serviceDuration: string | null
  date: string | null       // "YYYY-MM-DD"
  time: string | null       // "HH:mm:ss"
}

const INITIAL_STATE: BookingState = {
  masterId: null,
  masterName: null,
  serviceId: null,
  serviceName: null,
  servicePrice: null,
  serviceDuration: null,
  date: null,
  time: null,
}

type Step = 1 | 2 | 3 | 4 | 5

const STEP_TITLE: Record<Step, string> = {
  1: 'Выбор мастера',
  2: 'Выбор услуги',
  3: 'Выбор даты',
  4: 'Выбор времени',
  5: 'Подтверждение',
}

// ─── Шапка шага ───────────────────────────────────────────────────────────────

function StepHeader({ step, onBack }: { step: Step; onBack: () => void }) {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      gap: 12,
      padding: '14px 16px',
      borderBottom: '1px solid var(--tgui--divider)',
      background: 'var(--tgui--bg_color)',
      flexShrink: 0,
    }}>
      <button
        onClick={onBack}
        style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 4, color: 'var(--tgui--button_color)', fontSize: 22, lineHeight: 1, display: 'flex', alignItems: 'center' }}
      >
        ‹
      </button>
      <span style={{ fontWeight: 600, fontSize: 16 }}>{STEP_TITLE[step]}</span>
      <span style={{ marginLeft: 'auto', fontSize: 12, color: 'var(--tgui--hint_color)' }}>
        {step} / 5
      </span>
    </div>
  )
}

// ─── Вспомогательные ──────────────────────────────────────────────────────────

function CenterSpinner() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flex: 1 }}>
      <Spinner size="l" />
    </div>
  )
}

function CenterText({ text, error }: { text: string; error?: boolean }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flex: 1, color: error ? 'var(--tgui--destructive_text_color)' : 'var(--tgui--hint_color)', fontSize: 14, padding: '32px 16px', textAlign: 'center' }}>
      {text}
    </div>
  )
}

function formatDuration(iso: string): string {
  const parts = iso.split(':')
  const h = parseInt(parts[0] ?? '0', 10)
  const m = parseInt(parts[1] ?? '0', 10)
  if (h > 0 && m > 0) return `${h} ч ${m} мин`
  if (h > 0) return `${h} ч`
  return `${m} мин`
}

// C# DayOfWeek → JS getDay()
const CS_DAY_TO_JS: Record<string, number> = {
  Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
  Thursday: 4, Friday: 5, Saturday: 6,
}

// ─── Шаг 1: Список мастеров ───────────────────────────────────────────────────

function MasterAvatar({ name, photoUrl }: { name: string; photoUrl: string | null }) {
  const initials = name.split(' ').map(w => w[0]?.toUpperCase() ?? '').join('').slice(0, 2)
  if (photoUrl) {
    return <img src={photoUrl} alt={name} style={{ width: 52, height: 52, borderRadius: '50%', objectFit: 'cover', flexShrink: 0 }} />
  }
  return (
    <div style={{ width: 52, height: 52, borderRadius: '50%', background: 'var(--tgui--button_color)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 18, fontWeight: 700, color: '#fff', flexShrink: 0 }}>
      {initials}
    </div>
  )
}

function Step1Masters({ onSelect }: { onSelect: (master: MasterProfile) => void }) {
  const { data, isLoading, isError } = useQuery({ queryKey: ['masters'], queryFn: getMasters })

  if (isLoading) return <CenterSpinner />
  if (isError || !data) return <CenterText text="Не удалось загрузить список мастеров" error />
  if (data.masters.length === 0) return <CenterText text="Мастера пока не добавлены" />

  return (
    <div style={{ padding: '12px 0' }}>
      <div style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, overflow: 'hidden', margin: '0 16px' }}>
        {data.masters.map((master, idx) => {
          const fullName = [master.firstName, master.lastName].filter(Boolean).join(' ')
          return (
            <div
              key={master.id}
              onClick={() => onSelect(master)}
              style={{ display: 'flex', alignItems: 'center', gap: 14, padding: '14px 16px', borderBottom: idx < data.masters.length - 1 ? '1px solid var(--tgui--divider)' : 'none', cursor: 'pointer', WebkitTapHighlightColor: 'transparent' }}
            >
              <MasterAvatar name={fullName} photoUrl={master.photoUrl} />
              <div style={{ flex: 1, minWidth: 0 }}>
                <p style={{ margin: 0, fontWeight: 600, fontSize: 15, color: 'var(--tgui--text_color)' }}>{fullName}</p>
                {master.description && (
                  <p style={{ margin: '2px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{master.description}</p>
                )}
              </div>
              <span style={{ fontSize: 18, color: 'var(--tgui--hint_color)', flexShrink: 0 }}>›</span>
            </div>
          )
        })}
      </div>
    </div>
  )
}

// ─── Шаг 2: Услуги ────────────────────────────────────────────────────────────

function Step2Services({ masterId, onSelect }: { masterId: string; onSelect: (service: ServiceItem) => void }) {
  const { data, isLoading, isError } = useQuery({ queryKey: ['services', masterId], queryFn: () => getServices(masterId) })

  if (isLoading) return <CenterSpinner />
  if (isError || !data) return <CenterText text="Не удалось загрузить услуги" error />
  if (data.services.length === 0) return <CenterText text="У мастера пока нет услуг" />

  return (
    <div style={{ padding: '12px 0' }}>
      <div style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, overflow: 'hidden', margin: '0 16px' }}>
        {data.services.map((service, idx) => (
          <div
            key={service.id}
            onClick={() => onSelect(service)}
            style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '14px 16px', borderBottom: idx < data.services.length - 1 ? '1px solid var(--tgui--divider)' : 'none', cursor: 'pointer', WebkitTapHighlightColor: 'transparent' }}
          >
            <div style={{ flex: 1, minWidth: 0 }}>
              <p style={{ margin: 0, fontWeight: 600, fontSize: 15, color: 'var(--tgui--text_color)' }}>{service.name}</p>
              {service.description && <p style={{ margin: '2px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)' }}>{service.description}</p>}
              <p style={{ margin: '4px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)' }}>{formatDuration(service.duration)}</p>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexShrink: 0 }}>
              <span style={{ fontWeight: 600, fontSize: 15, color: 'var(--tgui--text_color)' }}>{service.price} ₽</span>
              <span style={{ fontSize: 18, color: 'var(--tgui--hint_color)' }}>›</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

// ─── Шаг 3: Выбор даты ────────────────────────────────────────────────────────

const WEEK_DAYS_SHORT = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']
const MONTH_NAMES = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь']

function toPad(n: number) { return String(n).padStart(2, '0') }

function dateToISO(year: number, month: number, day: number): string {
  return `${year}-${toPad(month + 1)}-${toPad(day)}`
}

function BookingCalendar({ masterId, selectedDate, onSelect, workingJsDays }: {
  masterId: string
  selectedDate: string | null
  onSelect: (date: string) => void
  workingJsDays: Set<number>
}) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  const [viewYear, setViewYear] = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const cells = useMemo(() => {
    const firstDay = new Date(viewYear, viewMonth, 1)
    // Monday-based: JS Sun=0 → index 6, Mon=1 → index 0
    const startOffset = (firstDay.getDay() + 6) % 7
    const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate()
    return { startOffset, daysInMonth }
  }, [viewYear, viewMonth])

  function prevMonth() {
    if (viewMonth === 0) { setViewYear(y => y - 1); setViewMonth(11) }
    else setViewMonth(m => m - 1)
  }

  function nextMonth() {
    if (viewMonth === 11) { setViewYear(y => y + 1); setViewMonth(0) }
    else setViewMonth(m => m + 1)
  }

  // Disable prev if already at current month
  const canGoPrev = viewYear > today.getFullYear() || viewMonth > today.getMonth()
  // Limit to 2 months ahead
  const maxDate = new Date(today.getFullYear(), today.getMonth() + 2, 0)
  const canGoNext = new Date(viewYear, viewMonth + 1, 0) < maxDate

  const totalCells = cells.startOffset + cells.daysInMonth
  const rows = Math.ceil(totalCells / 7)

  return (
    <div style={{ padding: '12px 16px' }}>
      {/* Навигация по месяцам */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
        <button
          onClick={prevMonth}
          disabled={!canGoPrev}
          style={{ background: 'none', border: 'none', cursor: canGoPrev ? 'pointer' : 'default', fontSize: 22, color: canGoPrev ? 'var(--tgui--button_color)' : 'var(--tgui--divider)', padding: '4px 8px' }}
        >‹</button>
        <span style={{ fontWeight: 600, fontSize: 15 }}>{MONTH_NAMES[viewMonth]} {viewYear}</span>
        <button
          onClick={nextMonth}
          disabled={!canGoNext}
          style={{ background: 'none', border: 'none', cursor: canGoNext ? 'pointer' : 'default', fontSize: 22, color: canGoNext ? 'var(--tgui--button_color)' : 'var(--tgui--divider)', padding: '4px 8px' }}
        >›</button>
      </div>

      {/* Дни недели */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', marginBottom: 4 }}>
        {WEEK_DAYS_SHORT.map(d => (
          <div key={d} style={{ textAlign: 'center', fontSize: 11, color: 'var(--tgui--hint_color)', padding: '2px 0', fontWeight: 500 }}>{d}</div>
        ))}
      </div>

      {/* Ячейки */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 2 }}>
        {Array.from({ length: rows * 7 }).map((_, i) => {
          const dayNum = i - cells.startOffset + 1
          if (dayNum < 1 || dayNum > cells.daysInMonth) {
            return <div key={i} />
          }

          const cellDate = new Date(viewYear, viewMonth, dayNum)
          const jsDay = cellDate.getDay()
          const isoDate = dateToISO(viewYear, viewMonth, dayNum)
          const isPast = cellDate < today
          const isWorking = workingJsDays.has(jsDay)
          const isSelected = selectedDate === isoDate

          const inactive = isPast || !isWorking
          let bg = 'transparent'
          if (isSelected) bg = 'var(--tgui--button_color)'
          else if (!isPast && isWorking) bg = 'rgba(0,122,255,0.10)'

          return (
            <div
              key={i}
              onClick={() => !inactive && onSelect(isoDate)}
              style={{
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                aspectRatio: '1',
                borderRadius: 8,
                background: bg,
                opacity: inactive ? 0.3 : 1,
                cursor: inactive ? 'default' : 'pointer',
                WebkitTapHighlightColor: 'transparent',
              }}
            >
              <span style={{ fontSize: 14, fontWeight: isSelected ? 700 : 400, color: isSelected ? '#fff' : 'var(--tgui--text_color)' }}>
                {dayNum}
              </span>
            </div>
          )
        })}
      </div>
    </div>
  )
}

function Step3Date({ masterId, selectedDate, onSelect }: {
  masterId: string
  selectedDate: string | null
  onSelect: (date: string) => void
}) {
  const { data: scheduleData, isLoading } = useQuery({
    queryKey: ['schedule', masterId],
    queryFn: () => getSchedule(masterId),
  })

  const workingJsDays = useMemo(() => {
    if (!scheduleData) return new Set<number>()
    return new Set(scheduleData.schedules.map(s => CS_DAY_TO_JS[s.dayOfWeek] ?? -1).filter(d => d >= 0))
  }, [scheduleData])

  if (isLoading) return <CenterSpinner />

  return (
    <div>
      <BookingCalendar
        masterId={masterId}
        selectedDate={selectedDate}
        onSelect={onSelect}
        workingJsDays={workingJsDays}
      />
      {selectedDate && (
        <p style={{ textAlign: 'center', fontSize: 13, color: 'var(--tgui--hint_color)', marginTop: 4 }}>
          Выбрано: {new Date(selectedDate + 'T00:00:00').toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })}
        </p>
      )}
    </div>
  )
}

// ─── Шаг 4: Выбор времени ─────────────────────────────────────────────────────

function Step4Slots({ masterId, serviceId, date, selectedTime, onSelect }: {
  masterId: string
  serviceId: string
  date: string
  selectedTime: string | null
  onSelect: (time: string) => void
}) {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['slots', masterId, serviceId, date],
    queryFn: () => getAvailableSlots(masterId, serviceId, date),
  })

  if (isLoading) return <CenterSpinner />
  if (isError || !data) return <CenterText text="Не удалось загрузить доступное время" error />
  if (data.slots.length === 0) return <CenterText text="Нет свободного времени на эту дату" />

  return (
    <div style={{ padding: '12px 16px' }}>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 10 }}>
        {data.slots.map(slot => {
          const timeLabel = slot.start.slice(0, 5)  // "HH:mm"
          const isSelected = selectedTime === slot.start
          return (
            <div
              key={slot.start}
              onClick={() => onSelect(slot.start)}
              style={{
                padding: '12px 8px',
                borderRadius: 10,
                textAlign: 'center',
                background: isSelected ? 'var(--tgui--button_color)' : 'var(--tgui--bg_color)',
                color: isSelected ? '#fff' : 'var(--tgui--text_color)',
                fontWeight: isSelected ? 600 : 400,
                fontSize: 15,
                cursor: 'pointer',
                WebkitTapHighlightColor: 'transparent',
                border: isSelected ? 'none' : '1px solid var(--tgui--divider)',
              }}
            >
              {timeLabel}
            </div>
          )
        })}
      </div>
    </div>
  )
}

// ─── Шаг 5: Подтверждение ─────────────────────────────────────────────────────

function Step5Confirm({ booking, clientId, onSuccess }: {
  booking: BookingState
  clientId: string
  onSuccess: () => void
}) {
  const queryClient = useQueryClient()

  const startTime = booking.date && booking.time
    ? `${booking.date}T${booking.time}`
    : null

  const displayDate = booking.date
    ? new Date(booking.date + 'T00:00:00').toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })
    : ''
  const displayTime = booking.time ? booking.time.slice(0, 5) : ''

  const mutation = useMutation({
    mutationFn: () => createAppointment({
      clientId,
      masterProfileId: booking.masterId!,
      serviceId: booking.serviceId!,
      startTime: startTime!,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['client-appointments'] })
      onSuccess()
    },
    onError: () => {
      window.Telegram.WebApp.showAlert('Не удалось создать запись. Попробуйте ещё раз.')
    },
  })

  const rows: { label: string; value: string }[] = [
    { label: 'Мастер', value: booking.masterName ?? '' },
    { label: 'Услуга', value: booking.serviceName ?? '' },
    { label: 'Стоимость', value: `${booking.servicePrice} ₽` },
    { label: 'Длительность', value: booking.serviceDuration ? formatDuration(booking.serviceDuration) : '' },
    { label: 'Дата', value: displayDate },
    { label: 'Время', value: displayTime },
  ]

  return (
    <div style={{ padding: '12px 0 32px' }}>
      <div style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, overflow: 'hidden', margin: '0 16px 24px' }}>
        {rows.map((row, idx) => (
          <div
            key={row.label}
            style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '13px 16px', borderBottom: idx < rows.length - 1 ? '1px solid var(--tgui--divider)' : 'none' }}
          >
            <span style={{ fontSize: 14, color: 'var(--tgui--hint_color)' }}>{row.label}</span>
            <span style={{ fontSize: 14, fontWeight: 600, color: 'var(--tgui--text_color)' }}>{row.value}</span>
          </div>
        ))}
      </div>

      <div style={{ padding: '0 16px' }}>
        <button
          onClick={() => mutation.mutate()}
          disabled={mutation.isPending}
          style={{ width: '100%', padding: '13px 0', borderRadius: 12, border: 'none', background: mutation.isPending ? 'var(--tgui--divider)' : 'var(--tgui--button_color)', color: mutation.isPending ? 'var(--tgui--hint_color)' : '#fff', fontSize: 15, fontWeight: 600, cursor: mutation.isPending ? 'not-allowed' : 'pointer' }}
        >
          {mutation.isPending ? 'Записываемся...' : 'Подтвердить запись'}
        </button>
      </div>
    </div>
  )
}

// ─── Booking Flow ─────────────────────────────────────────────────────────────

interface BookingFlowProps {
  onClose: () => void
  clientId: string
}

export function BookingFlow({ onClose, clientId }: BookingFlowProps) {
  const [step, setStep] = useState<Step>(1)
  const [booking, setBooking] = useState<BookingState>(INITIAL_STATE)

  function goBack() {
    if (step === 1) {
      onClose()
    } else {
      setStep(s => (s - 1) as Step)
    }
  }

  function selectMaster(master: MasterProfile) {
    const fullName = [master.firstName, master.lastName].filter(Boolean).join(' ')
    setBooking(b => ({ ...b, masterId: master.id, masterName: fullName, serviceId: null, serviceName: null, servicePrice: null, serviceDuration: null, date: null, time: null }))
    setStep(2)
  }

  function selectService(service: ServiceItem) {
    setBooking(b => ({ ...b, serviceId: service.id, serviceName: service.name, servicePrice: service.price, serviceDuration: service.duration, date: null, time: null }))
    setStep(3)
  }

  function selectDate(date: string) {
    setBooking(b => ({ ...b, date, time: null }))
  }

  function selectTime(time: string) {
    setBooking(b => ({ ...b, time }))
  }

  // "Далее" button shows on steps 3 and 4 when something is selected
  const canProceedStep3 = step === 3 && booking.date !== null
  const canProceedStep4 = step === 4 && booking.time !== null
  const showNextBtn = canProceedStep3 || canProceedStep4

  return (
    <div style={{ position: 'fixed', inset: 0, zIndex: 200, background: 'var(--tgui--secondary_bg_color)', display: 'flex', flexDirection: 'column' }}>
      <StepHeader step={step} onBack={goBack} />

      <div style={{ flex: 1, overflowY: 'auto' }}>
        {step === 1 && <Step1Masters onSelect={selectMaster} />}
        {step === 2 && booking.masterId && (
          <Step2Services masterId={booking.masterId} onSelect={selectService} />
        )}
        {step === 3 && booking.masterId && (
          <Step3Date masterId={booking.masterId} selectedDate={booking.date} onSelect={selectDate} />
        )}
        {step === 4 && booking.masterId && booking.serviceId && booking.date && (
          <Step4Slots
            masterId={booking.masterId}
            serviceId={booking.serviceId}
            date={booking.date}
            selectedTime={booking.time}
            onSelect={selectTime}
          />
        )}
        {step === 5 && (
          <Step5Confirm
            booking={booking}
            clientId={clientId}
            onSuccess={onClose}
          />
        )}
      </div>

      {showNextBtn && (
        <div style={{ padding: '12px 16px', background: 'var(--tgui--bg_color)', borderTop: '1px solid var(--tgui--divider)', flexShrink: 0 }}>
          <button
            onClick={() => setStep(s => (s + 1) as Step)}
            style={{ width: '100%', padding: '13px 0', borderRadius: 12, border: 'none', background: 'var(--tgui--button_color)', color: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' }}
          >
            Далее
          </button>
        </div>
      )}
    </div>
  )
}
