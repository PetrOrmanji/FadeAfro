import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner } from '@telegram-apps/telegram-ui'
import { getMasters, type MasterProfile } from '@/api/masters'
import { getServices, type ServiceItem } from '@/api/services'
import { getMasterAvailability } from '@/api/slots'
import { createAppointment } from '@/api/appointments'

// ─── Типы ─────────────────────────────────────────────────────────────────────

export interface BookingState {
  masterId: string | null
  masterName: string | null
  masterPhotoUrl: string | null
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
  masterPhotoUrl: null,
  serviceId: null,
  serviceName: null,
  servicePrice: null,
  serviceDuration: null,
  date: null,
  time: null,
}

type Step = 1 | 2 | 3 | 4

const STEP_TITLE: Record<Step, string> = {
  1: 'Выбор мастера',
  2: 'Выбор услуги',
  3: 'Выбор даты и времени',
  4: 'Подтверждение',
}

// ─── Шапка шага ───────────────────────────────────────────────────────────────

const TOTAL_STEPS = 4

function StepHeader({ step, onBack }: { step: Step; onBack: () => void }) {
  const progress = (step / TOTAL_STEPS) * 100

  return (
    <div style={{ background: 'var(--tgui--bg_color)', flexShrink: 0 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '8px 16px', minHeight: 34 }}>
        <button
          onClick={onBack}
          style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 0, width: 34, height: 34, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--tgui--button_color)', fontSize: 22, lineHeight: 1, flexShrink: 0 }}
        >
          ‹
        </button>
        <span style={{ fontWeight: 700, fontSize: 16 }}>{STEP_TITLE[step]}</span>
      </div>
      {/* Прогресс-бар */}
      <div style={{ height: 3, background: 'var(--tgui--divider)' }}>
        <div style={{ height: '100%', width: `${progress}%`, background: 'var(--tgui--button_color)', borderRadius: '0 2px 2px 0', transition: 'width 0.3s ease' }} />
      </div>
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

  const hourLabel = h === 1 ? 'час' : h < 5 ? 'часа' : 'часов'

  if (h > 0 && m > 0) return `${h} ${hourLabel} ${m} мин`
  if (h > 0) return `${h} ${hourLabel}`
  return `${m} мин`
}

// ─── Шаг 1: Список мастеров ───────────────────────────────────────────────────

function MasterAvatar({ name, photoUrl, size = 64 }: { name: string; photoUrl: string | null; size?: number }) {
  const initials = name.split(' ').map(w => w[0]?.toUpperCase() ?? '').join('').slice(0, 2)
  const fontSize = Math.round(size * 0.34)
  if (photoUrl) {
    return <img src={photoUrl} alt={name} style={{ width: size, height: size, borderRadius: '50%', objectFit: 'cover', flexShrink: 0 }} />
  }
  return (
    <div style={{ width: size, height: size, borderRadius: '50%', background: 'var(--tgui--button_color)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize, fontWeight: 700, color: '#fff', flexShrink: 0 }}>
      {initials}
    </div>
  )
}

function MasterDetailSheet({ master, fullName, onClose, onSelect }: {
  master: MasterProfile
  fullName: string
  onClose: () => void
  onSelect: () => void
}) {
  return (
    <>
      <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', zIndex: 300 }} />
      <div style={{ position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 301, background: 'var(--tgui--bg_color)', borderRadius: '16px 16px 0 0', padding: '20px 16px 40px', maxHeight: '80dvh', overflowY: 'auto' }}>
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 16 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--tgui--divider)' }} />
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
          <MasterAvatar name={fullName} photoUrl={master.photoUrl} />
          <p style={{ margin: 0, fontWeight: 700, fontSize: 17 }}>{fullName}</p>
        </div>
        {master.description && (
          <p style={{ margin: '0 0 24px', fontSize: 14, color: 'var(--tgui--text_color)', lineHeight: 1.6 }}>{master.description}</p>
        )}
        <button
          onClick={onSelect}
          style={{ width: '100%', padding: '13px 0', borderRadius: 12, border: 'none', background: 'var(--tgui--button_color)', color: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' }}
        >
          Выбрать мастера
        </button>
      </div>
    </>
  )
}

function MasterRow({ master, onSelect }: { master: MasterProfile; onSelect: () => void }) {
  const [showDetail, setShowDetail] = useState(false)
  const fullName = [master.firstName, master.lastName].filter(Boolean).join(' ')

  return (
    <>
      <div
        onClick={() => setShowDetail(true)}
        className="master-row"
        style={{ display: 'flex', alignItems: 'center', gap: 14, padding: '12px 16px', cursor: 'pointer', WebkitTapHighlightColor: 'transparent' }}
      >
        <MasterAvatar name={fullName} photoUrl={master.photoUrl} />
        <div style={{ flex: 1, minWidth: 0 }}>
          <p style={{ margin: 0, fontWeight: 600, fontSize: 15, color: 'var(--tgui--text_color)' }}>{fullName}</p>
          {master.description && (
            <>
              <p style={{ margin: '3px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                {master.description}
              </p>
              <span className="master-detail-link" style={{ fontSize: 13, color: 'var(--tgui--button_color)', fontWeight: 500, cursor: 'pointer', transition: 'opacity 0.15s' }}>
                Подробнее
              </span>
            </>
          )}
        </div>
        <button
          onClick={e => { e.stopPropagation(); onSelect() }}
          className="master-row-select-btn"
          style={{ flexShrink: 0, padding: '7px 14px', borderRadius: 10, border: 'none', background: 'var(--tgui--button_color)', color: '#fff', fontSize: 13, fontWeight: 600, cursor: 'pointer', transition: 'opacity 0.15s' }}
        >
          Выбрать
        </button>
      </div>

      {showDetail && (
        <MasterDetailSheet
          master={master}
          fullName={fullName}
          onClose={() => setShowDetail(false)}
          onSelect={() => { setShowDetail(false); onSelect() }}
        />
      )}
    </>
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
        {data.masters.map((master, idx) => (
          <div key={master.id} style={{ borderBottom: idx < data.masters.length - 1 ? '1px solid var(--tgui--divider)' : 'none' }}>
            <MasterRow master={master} onSelect={() => onSelect(master)} />
          </div>
        ))}
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
    <div style={{ padding: 12, display: 'flex', flexDirection: 'column', gap: 10 }}>
      {data.services.map(service => (
        <div
          key={service.id}
          onClick={() => onSelect(service)}
          className="service-row"
          style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, padding: '14px 16px', cursor: 'pointer', WebkitTapHighlightColor: 'transparent' }}
        >
          <div style={{ display: 'flex', gap: 12, alignItems: 'stretch' }}>
            {/* Левый столбик: название + описание */}
            <div style={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column', gap: 4 }}>
              <p style={{ margin: 0, fontWeight: 600, fontSize: 15, color: 'var(--tgui--text_color)' }}>{service.name}</p>
              {service.description && (
                <p style={{ margin: 0, fontSize: 13, color: 'var(--tgui--hint_color)' }}>{service.description}</p>
              )}
            </div>
            {/* Правый столбик: время + цена */}
            <div style={{ flexShrink: 0, display: 'flex', flexDirection: 'column', alignItems: 'flex-end', justifyContent: 'space-between', gap: 6 }}>
              <span style={{ padding: '4px 0', borderRadius: 20, background: 'rgba(142,142,147,0.15)', fontSize: 12, color: 'var(--tgui--hint_color)', fontWeight: 500, minWidth: 72, textAlign: 'center' }}>
                {formatDuration(service.duration)}
              </span>
              <span style={{ padding: '4px 12px', borderRadius: 20, background: 'var(--tgui--button_color)', color: '#fff', fontSize: 13, fontWeight: 600 }}>
                {service.price} ₽
              </span>
            </div>
          </div>
        </div>
      ))}
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

// ─── Месячный календарь (без навигации) ──────────────────────────────────────

function MonthGrid({ year, month, selectedDate, availableDates, onSelect }: {
  year: number
  month: number
  selectedDate: string | null
  availableDates: Set<string> | null  // null = ещё грузится
  onSelect: (date: string) => void
}) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  const firstDay = new Date(year, month, 1)
  const startOffset = (firstDay.getDay() + 6) % 7
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const rows = Math.ceil((startOffset + daysInMonth) / 7)

  return (
    <div style={{ padding: '0 16px 16px' }}>
      <p style={{ margin: '0 0 8px', fontWeight: 600, fontSize: 15 }}>
        {MONTH_NAMES[month]} {year}
      </p>
      {/* Дни недели */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', marginBottom: 4 }}>
        {WEEK_DAYS_SHORT.map(d => (
          <div key={d} style={{ textAlign: 'center', fontSize: 11, color: 'var(--tgui--hint_color)', padding: '2px 0', fontWeight: 500 }}>{d}</div>
        ))}
      </div>
      {/* Ячейки */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', border: '1px solid var(--tgui--divider)', borderRadius: 8, overflow: 'hidden' }}>
        {Array.from({ length: rows * 7 }).map((_, i) => {
          const dayNum = i - startOffset + 1
          if (dayNum < 1 || dayNum > daysInMonth) return <div key={i} />

          const cellDate = new Date(year, month, dayNum)
          const isoDate = dateToISO(year, month, dayNum)
          const isPast = cellDate < today
          const isSelected = selectedDate === isoDate
          // null = loading — не подсвечиваем ничего пока
          const hasSlots = availableDates !== null && availableDates.has(isoDate)
          const inactive = isPast || !hasSlots

          let bg = 'rgba(142,142,147,0.12)'
          if (isSelected) bg = 'var(--tgui--button_color)'
          else if (!isPast && hasSlots) bg = 'rgba(0,122,255,0.10)'

          return (
            <div
              key={i}
              onClick={() => !inactive && onSelect(isoDate)}
              style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '10px 0', cursor: inactive ? 'default' : 'pointer', WebkitTapHighlightColor: 'transparent', borderRight: '1px solid var(--tgui--divider)', borderBottom: '1px solid var(--tgui--divider)', background: bg }}
            >
              <span style={{ fontSize: 13, fontWeight: isSelected ? 700 : 400, color: isSelected ? '#fff' : 'var(--tgui--text_color)' }}>
                {dayNum}
              </span>
            </div>
          )
        })}
      </div>
    </div>
  )
}

// ─── Слоты времени (bottom sheet) ─────────────────────────────────────────────

function TimeSlotsSheet({ date, slots, onSelect, onClose }: {
  date: string
  slots: string[]
  onSelect: (time: string) => void
  onClose: () => void
}) {
  const displayDate = new Date(date + 'T00:00:00').toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })

  return (
    <>
      <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)', zIndex: 210 }} />
      <div style={{ position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 211, background: 'var(--tgui--bg_color)', borderRadius: '16px 16px 0 0', padding: '20px 16px 40px', maxHeight: '60dvh', overflowY: 'auto' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
          <p style={{ margin: 0, fontWeight: 600, fontSize: 16 }}>Время — {displayDate}</p>
          <button
            onClick={onClose}
            style={{ background: 'var(--tgui--secondary_bg_color)', border: 'none', cursor: 'pointer', fontSize: 16, color: 'var(--tgui--hint_color)', width: 32, height: 32, borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          >
            ✕
          </button>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 8 }}>
          {slots.map(slot => (
            <div
              key={slot}
              onClick={() => onSelect(slot)}
              style={{ padding: '12px 4px', borderRadius: 10, textAlign: 'center', background: 'var(--tgui--secondary_bg_color)', color: 'var(--tgui--text_color)', fontSize: 14, fontWeight: 500, cursor: 'pointer', WebkitTapHighlightColor: 'transparent', border: '1px solid var(--tgui--divider)' }}
            >
              {slot.slice(0, 5)}
            </div>
          ))}
        </div>
      </div>
    </>
  )
}

// ─── Шаг 3: Дата + время ─────────────────────────────────────────────────────

function Step3DateAndTime({ masterId, serviceId, selectedDate, onSelectDateTime }: {
  masterId: string
  serviceId: string
  selectedDate: string | null
  onSelectDateTime: (date: string, time: string) => void
}) {
  const [pickedDate, setPickedDate] = useState<string | null>(selectedDate)

  const today = new Date()
  const nextMonthDate = new Date(today.getFullYear(), today.getMonth() + 1, 1)

  const { data: availabilityData, isLoading } = useQuery({
    queryKey: ['availability', masterId, serviceId],
    queryFn: () => getMasterAvailability(masterId, serviceId),
  })

  // Множество дат с доступными слотами
  const availableDates = useMemo(() => {
    if (!availabilityData) return null
    return new Set(availabilityData.days.map(d => d.date))
  }, [availabilityData])

  // Карта дата → слоты
  const slotsByDate = useMemo(() => {
    const map = new Map<string, string[]>()
    if (availabilityData) {
      for (const d of availabilityData.days) {
        map.set(d.date, d.slots)
      }
    }
    return map
  }, [availabilityData])

  if (isLoading) return <CenterSpinner />

  const pickedSlots = pickedDate ? (slotsByDate.get(pickedDate) ?? []) : []

  return (
    <div style={{ paddingTop: 12, position: 'relative' }}>
      {/* Легенда */}
      <div style={{ display: 'flex', gap: 12, padding: '0 16px 12px', flexWrap: 'wrap' }}>
        {[
          { bg: 'rgba(0,122,255,0.10)', border: '1px solid rgba(0,122,255,0.3)', label: 'Есть запись' },
          { bg: 'var(--tgui--button_color)', border: 'none', label: 'Выбрано' },
          { bg: 'rgba(142,142,147,0.12)', border: 'none', label: 'Недоступно' },
        ].map(({ bg, border, label }) => (
          <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
            <div style={{ width: 14, height: 14, borderRadius: 4, background: bg, border: border ?? 'none', flexShrink: 0 }} />
            <span style={{ fontSize: 12, color: 'var(--tgui--hint_color)' }}>{label}</span>
          </div>
        ))}
      </div>

      {/* Текущий месяц */}
      <MonthGrid
        year={today.getFullYear()}
        month={today.getMonth()}
        selectedDate={pickedDate}
        availableDates={availableDates}
        onSelect={d => setPickedDate(d)}
      />

      {/* Подсказка */}
      <p style={{ margin: '0 16px 12px', fontSize: 13, color: 'var(--tgui--hint_color)', textAlign: 'center' }}>
        Нажмите на доступный день, чтобы увидеть свободное время
      </p>

      {/* Следующий месяц */}
      <MonthGrid
        year={nextMonthDate.getFullYear()}
        month={nextMonthDate.getMonth()}
        selectedDate={pickedDate}
        availableDates={availableDates}
        onSelect={d => setPickedDate(d)}
      />

      {/* Слоты времени — поверх календаря */}
      {pickedDate && (
        <TimeSlotsSheet
          date={pickedDate}
          slots={pickedSlots}
          onSelect={time => onSelectDateTime(pickedDate, time)}
          onClose={() => setPickedDate(null)}
        />
      )}
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
    ? `${booking.date}T${booking.time}Z`
    : null

  const displayDate = booking.date
    ? new Date(booking.date + 'T00:00:00').toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' })
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

  const serviceRows: { label: string; value: string }[] = [
    { label: 'Стоимость', value: `${booking.servicePrice} ₽` },
    { label: 'Длительность', value: booking.serviceDuration ? formatDuration(booking.serviceDuration) : '' },
  ]

  return (
    <div style={{ padding: '0 0 32px' }}>
      {/* Hero-блок мастера */}
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', padding: '28px 16px 24px', gap: 14 }}>
        <MasterAvatar name={booking.masterName ?? ''} photoUrl={booking.masterPhotoUrl} size={88} />
        <div style={{ textAlign: 'center' }}>
          <p style={{ margin: 0, fontWeight: 700, fontSize: 20, color: 'var(--tgui--text_color)' }}>{booking.masterName}</p>
          <p style={{ margin: '5px 0 0', fontSize: 15, color: 'var(--tgui--hint_color)' }}>{booking.serviceName}</p>
        </div>
      </div>

      {/* Стоимость и длительность */}
      <div style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, overflow: 'hidden', margin: '0 16px 12px' }}>
        {serviceRows.map((row, idx) => (
          <div
            key={row.label}
            style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '13px 16px', borderBottom: idx < serviceRows.length - 1 ? '1px solid var(--tgui--divider)' : 'none' }}
          >
            <span style={{ fontSize: 14, color: 'var(--tgui--hint_color)' }}>{row.label}</span>
            <span style={{ fontSize: 14, fontWeight: 600, color: 'var(--tgui--text_color)' }}>{row.value}</span>
          </div>
        ))}
      </div>

      {/* Акцентный блок даты и времени */}
      <div style={{ margin: '0 16px 24px', borderRadius: 12, background: 'rgba(0,122,255,0.08)', border: '1px solid rgba(0,122,255,0.18)', padding: '16px 20px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <p style={{ margin: 0, fontSize: 12, color: 'var(--tgui--button_color)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5, marginBottom: 4 }}>Дата и время</p>
          <p style={{ margin: 0, fontSize: 17, fontWeight: 700, color: 'var(--tgui--text_color)' }}>{displayDate}</p>
        </div>
        <p style={{ margin: 0, fontSize: 28, fontWeight: 700, color: 'var(--tgui--button_color)' }}>{displayTime}</p>
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
    setBooking(b => ({ ...b, masterId: master.id, masterName: fullName, masterPhotoUrl: master.photoUrl ?? null, serviceId: null, serviceName: null, servicePrice: null, serviceDuration: null, date: null, time: null }))
    setStep(2)
  }

  function selectService(service: ServiceItem) {
    setBooking(b => ({ ...b, serviceId: service.id, serviceName: service.name, servicePrice: service.price, serviceDuration: service.duration, date: null, time: null }))
    setStep(3)
  }

  function selectDateTime(date: string, time: string) {
    setBooking(b => ({ ...b, date, time }))
    setStep(4)
  }

  return (
    <div style={{ position: 'fixed', inset: 0, zIndex: 200, background: 'var(--tgui--secondary_bg_color)', display: 'flex', flexDirection: 'column' }}>
      <StepHeader step={step} onBack={goBack} />

      <div style={{ flex: 1, overflowY: 'auto' }}>
        {step === 1 && <Step1Masters onSelect={selectMaster} />}
        {step === 2 && booking.masterId && (
          <Step2Services masterId={booking.masterId} onSelect={selectService} />
        )}
        {step === 3 && booking.masterId && booking.serviceId && (
          <Step3DateAndTime
            masterId={booking.masterId}
            serviceId={booking.serviceId}
            selectedDate={booking.date}
            onSelectDateTime={selectDateTime}
          />
        )}
        {step === 4 && (
          <Step5Confirm
            booking={booking}
            clientId={clientId}
            onSuccess={onClose}
          />
        )}
      </div>
    </div>
  )
}
