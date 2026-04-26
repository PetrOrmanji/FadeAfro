import { useState, useRef, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner } from '@telegram-apps/telegram-ui'
import { getCurrentUser, updateUserName } from '@/api/users'

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
        width: 80,
        height: 80,
        borderRadius: '50%',
        background: 'var(--tgui--button_color)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: 28,
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

interface EditNameSheetProps {
  firstName: string
  lastName: string | null
  onClose: () => void
}

function EditNameSheet({ firstName, lastName, onClose }: EditNameSheetProps) {
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

  // Prevent scroll behind sheet
  useEffect(() => {
    document.body.style.overflow = 'hidden'
    return () => { document.body.style.overflow = '' }
  }, [])

  const canSave = first.trim().length > 0 && !mutation.isPending

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0,0,0,0.4)',
          zIndex: 100,
        }}
      />

      {/* Sheet */}
      <div
        style={{
          position: 'fixed',
          bottom: 0,
          left: 0,
          right: 0,
          zIndex: 101,
          background: 'var(--tgui--bg_color)',
          borderRadius: '16px 16px 0 0',
          padding: '20px 16px 32px',
        }}
      >
        {/* Handle */}
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 20 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--tgui--divider)' }} />
        </div>

        <p style={{ margin: '0 0 20px', fontWeight: 600, fontSize: 17 }}>Редактировать имя</p>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 16, marginBottom: 24 }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <label style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>Имя *</label>
            <input
              autoFocus
              value={first}
              onChange={e => setFirst(e.target.value)}
              placeholder="Имя"
              style={{
                padding: '10px 12px',
                borderRadius: 10,
                border: '1px solid var(--tgui--divider)',
                background: 'var(--tgui--secondary_bg_color)',
                color: 'var(--tgui--text_color)',
                fontSize: 15,
                outline: 'none',
                width: '100%',
                boxSizing: 'border-box',
              }}
            />
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <label style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>Фамилия</label>
            <input
              value={last}
              onChange={e => setLast(e.target.value)}
              placeholder="Фамилия (необязательно)"
              style={{
                padding: '10px 12px',
                borderRadius: 10,
                border: '1px solid var(--tgui--divider)',
                background: 'var(--tgui--secondary_bg_color)',
                color: 'var(--tgui--text_color)',
                fontSize: 15,
                outline: 'none',
                width: '100%',
                boxSizing: 'border-box',
              }}
            />
          </div>
        </div>

        <button
          onClick={() => mutation.mutate()}
          disabled={!canSave}
          style={{
            width: '100%',
            padding: '13px 0',
            borderRadius: 12,
            border: 'none',
            background: canSave ? 'var(--tgui--button_color)' : 'var(--tgui--divider)',
            color: canSave ? '#fff' : 'var(--tgui--hint_color)',
            fontSize: 15,
            fontWeight: 600,
            cursor: canSave ? 'pointer' : 'not-allowed',
            transition: 'background 0.15s',
          }}
        >
          {mutation.isPending ? 'Сохранение...' : 'Сохранить'}
        </button>
      </div>
    </>
  )
}

// ─── Client Page ──────────────────────────────────────────────────────────────

export function ClientPage() {
  const [editOpen, setEditOpen] = useState(false)

  const { data: user, isLoading } = useQuery({
    queryKey: ['current-user'],
    queryFn: getCurrentUser,
  })

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100dvh' }}>
        <Spinner size="l" />
      </div>
    )
  }

  const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(' ')

  return (
    <div style={{ minHeight: '100dvh', background: 'var(--tgui--secondary_bg_color)' }}>
      {/* Header card */}
      <div
        style={{
          background: 'var(--tgui--bg_color)',
          padding: '24px 16px 20px',
          display: 'flex',
          alignItems: 'center',
          gap: 16,
        }}
      >
        <Avatar firstName={user?.firstName ?? '?'} lastName={user?.lastName} />

        <div style={{ flex: 1, minWidth: 0 }}>
          <p style={{ margin: 0, fontWeight: 700, fontSize: 18, lineHeight: 1.3 }}>{fullName}</p>
          <p style={{ margin: '2px 0 0', fontSize: 13, color: 'var(--tgui--hint_color)' }}>Клиент</p>
        </div>

        <button
          onClick={() => setEditOpen(true)}
          style={{
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            padding: 8,
            borderRadius: 8,
            color: 'var(--tgui--hint_color)',
            fontSize: 20,
            lineHeight: 1,
            flexShrink: 0,
          }}
          aria-label="Редактировать имя"
        >
          ✏️
        </button>
      </div>

      {/* Appointments placeholder */}
      <div style={{ padding: '16px 16px 0' }}>
        <p style={{ margin: '0 0 12px', fontWeight: 600, fontSize: 16 }}>Мои записи</p>
        <div
          style={{
            background: 'var(--tgui--bg_color)',
            borderRadius: 12,
            padding: '32px 16px',
            textAlign: 'center',
            color: 'var(--tgui--hint_color)',
            fontSize: 14,
          }}
        >
          Здесь появятся ваши записи
        </div>
      </div>

      {/* Edit name sheet */}
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
