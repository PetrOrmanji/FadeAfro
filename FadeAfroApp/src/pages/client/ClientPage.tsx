import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner } from '@telegram-apps/telegram-ui'
import { getCurrentUser, updateUserName } from '@/api/users'
import { getClientAppointments, type AppointmentItem, type AppointmentStatus } from '@/api/appointments'

// ─── Avatar ──────────────────────────────────────────────────────────────────

function getInitials(firstName: string, lastName?: string | null) {
  const f = firstName.trim()[0]?.toUpperCase() ?? ''
  const l = lastName?.trim()[0]?.toUpperCase() ?? ''
  return f + l
}

function Avatar({ firstName, lastName }: { firstName: string; lastName?: string | null }) {
  const initials = getInitials(firstName, lastName)
  return (
    <div
      style={{
        width: 48,
        height: 48,
        borderRadius: '50%',
        background: 'var(--tgui--button_color)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: 18,
        fontWeight: 700,
        color: '#fff',
        flexShrink: 0,
      }}
    >
      {initials}
    </div>
  )
}

// ─── Edit Bottom Sheet ────────────────────────────────────────────────────────

function EditNameSheet({ firstName, lastName, onClose }: {
  firstName: string
  lastName: string | null
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [first, setFirst] = useState(firstName)
  const [last, setLast] = useState(lastName ?? '')

  const mutation = useMutation({
    mutationFn: () => updateUserName(first.trim(), last.trim() || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-user'] })
      onClose()
    },
    onError: () => {
      window.Telegram.WebApp.showAlert('Не удалось сохранить имя. Попробуйте ещё раз.')
    },
  })

  useEffect(() => {
    document.body.style.overflow = 'hidden'
    return () => { document.body.style.overflow = '' }
  }, [])

  const canSave = first.trim().length > 0 && !mutation.isPending

  return (
    <>
      <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)', zIndex: 100 }} />
      <div style={{ position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 101, background: 'var(--tgui--bg_color)', borderRadius: '16px 16px 0 0', padding: '20px 16px 32px' }}>
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 20 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--tgui--divider)' }} />
        </div>
        <p style={{ margin: '0 0 20px', fontWeight: 600, fontSize: 17 }}>Редактировать имя</p>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16, marginBottom: 24 }}>
          {[
            { label: 'Имя *', value: first, onChange: setFirst, placeholder: 'Имя', autoFocus: true },
            { label: 'Фамилия', value: last, onChange: setLast, placeholder: 'Фамилия (необязательно)', autoFocus: false },
          ].map(({ label, value, onChange, placeholder, autoFocus }) => (
            <div key={label} style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              <label style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{label}</label>
              <input
                autoFocus={autoFocus}
                value={value}
                onChange={e => onChange(e.target.value)}
                placeholder={placeholder}
                style={{ padding: '10px 12px', borderRadius: 10, border: '1px solid var(--tgui--divider)', background: 'var(--tgui--secondary_bg_color)', color: 'var(--tgui--text_color)', fontSize: 15, outline: 'none', width: '100%', boxSizing: 'border-box' }}
              />
            </div>
          ))}
        </div>
        <button
          onClick={() => mutation.mutate()}
          disabled={!canSave}
          style={{ width: '100%', padding: '13px 0', borderRadius: 12, border: 'none', background: canSave ? 'var(--tgui--button_color)' : 'var(--tgui--divider)', color: canSave ? '#fff' : 'var(--tgui--hint_color)', fontSize: 15, fontWeight: 600, cursor: canSave ? 'pointer' : 'not-allowed' }}
        >
          {mutation.isPending ? 'Сохранение...' : 'Сохранить'}
        </button>
      </div>
    </>
  )
}

// ─── Appointment Card ─────────────────────────────────────────────────────────

const STATUS_LABEL: Record<AppointmentStatus, string> = {
  Pending:           'Ожидает',
  Confirmed:         'Подтверждена',
  CancelledByClient: 'Отменена вами',
  CancelledByMaster: 'Отменена мастером',
  Completed:         'Завершена',
}

const STATUS_COLOR: Record<AppointmentStatus, string> = {
  Pending:           '#FF9500',
  Confirmed:         '#34C759',
  CancelledByClient: '#8E8E93',
  CancelledByMaster: '#8E8E93',
  Completed:         '#8E8E93',
}

function formatDateTime(iso: string) {
  const date = new Date(iso)
  return {
    date: date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' }),
    time: date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }),
  }
}

function AppointmentCard({ item }: { item: AppointmentItem }) {
  const { date, time } = formatDateTime(item.startTime)
  const { time: timeEnd } = formatDateTime(item.endTime)

  return (
    <div style={{ padding: '14px 16px', borderBottom: '1px solid var(--tgui--divider)' }}>
      {/* Дата и статус */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
        <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--tgui--text_color)' }}>
          {date}, {time}–{timeEnd}
        </span>
        <span style={{ fontSize: 12, color: STATUS_COLOR[item.status], fontWeight: 500 }}>
          {STATUS_LABEL[item.status]}
        </span>
      </div>

      {/* Мастер */}
      <p style={{ margin: '0 0 2px', fontSize: 14, color: 'var(--tgui--text_color)' }}>
        {item.masterName}
      </p>

      {/* Услуга и цена */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{item.serviceName}</span>
        <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{item.servicePrice} ₽</span>
      </div>
    </div>
  )
}

function AppointmentsSection({ title, items }: { title: string; items: AppointmentItem[] }) {
  return (
    <div style={{ marginBottom: 16 }}>
      <p style={{ margin: '0 0 8px 0', fontWeight: 600, fontSize: 15, padding: '0 16px' }}>{title}</p>
      <div style={{ background: 'var(--tgui--bg_color)', borderRadius: 12, overflow: 'hidden' }}>
        {items.length === 0 ? (
          <p style={{ margin: 0, padding: '24px 16px', textAlign: 'center', fontSize: 14, color: 'var(--tgui--hint_color)' }}>
            Нет записей
          </p>
        ) : (
          items.map((item, idx) => (
            <div key={item.id} style={{ borderBottom: idx < items.length - 1 ? undefined : 'none' }}>
              <AppointmentCard item={item} />
            </div>
          ))
        )}
      </div>
    </div>
  )
}

// ─── Client Page ──────────────────────────────────────────────────────────────

const UPCOMING_STATUSES: AppointmentStatus[] = ['Pending', 'Confirmed']

export function ClientPage() {
  const [editOpen, setEditOpen] = useState(false)

  const { data: user, isLoading: userLoading } = useQuery({
    queryKey: ['current-user'],
    queryFn: getCurrentUser,
  })

  const { data: appointmentsData, isLoading: apptLoading } = useQuery({
    queryKey: ['client-appointments', user?.id],
    queryFn: () => getClientAppointments(user!.id),
    enabled: !!user?.id,
  })

  if (userLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100dvh' }}>
        <Spinner size="l" />
      </div>
    )
  }

  const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(' ')
  const allAppointments = appointmentsData?.items ?? []

  const upcoming = allAppointments.filter(a => UPCOMING_STATUSES.includes(a.status))
  const history  = allAppointments.filter(a => !UPCOMING_STATUSES.includes(a.status))

  return (
    <div style={{ minHeight: '100dvh', background: 'var(--tgui--secondary_bg_color)' }}>
      {/* Шапка */}
      <div
        onClick={() => setEditOpen(true)}
        style={{ background: 'var(--tgui--bg_color)', padding: '12px 16px', display: 'flex', alignItems: 'center', gap: 12, cursor: 'pointer', WebkitTapHighlightColor: 'transparent', marginBottom: 16 }}
      >
        <Avatar firstName={user?.firstName ?? '?'} lastName={user?.lastName} />
        <div style={{ flex: 1, minWidth: 0 }}>
          <p style={{ margin: 0, fontWeight: 600, fontSize: 16, lineHeight: 1.3 }}>{fullName}</p>
          <p style={{ margin: '2px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)' }}>Клиент</p>
        </div>
      </div>

      {/* Записи */}
      {apptLoading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 32 }}>
          <Spinner size="m" />
        </div>
      ) : (
        <div style={{ padding: '0 0 32px' }}>
          {upcoming.length > 0 && <AppointmentsSection title="Предстоящие" items={upcoming} />}
          {history.length > 0 && <AppointmentsSection title="История" items={history} />}
          {upcoming.length === 0 && history.length === 0 && (
            <p style={{ textAlign: 'center', color: 'var(--tgui--hint_color)', fontSize: 14, padding: '32px 16px' }}>
              У вас пока нет записей
            </p>
          )}
        </div>
      )}

      {/* Боттом-шит редактирования */}
      {editOpen && user && (
        <EditNameSheet
          firstName={user.firstName}
          lastName={user.lastName}
          onClose={() => setEditOpen(false)}
        />
      )}
    </div>
  )
}
