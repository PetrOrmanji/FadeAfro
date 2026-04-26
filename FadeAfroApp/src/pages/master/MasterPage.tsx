import { useState, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Spinner, Placeholder } from '@telegram-apps/telegram-ui'
import {
  getMyMasterProfile,
  updateMasterProfile,
  uploadMasterPhoto,
} from '@/api/masters'
import { updateUserName } from '@/api/users'

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

  const { data: profile, isLoading, isError } = useQuery({
    queryKey: ['my-master-profile'],
    queryFn: getMyMasterProfile,
  })

  const [firstName, setFirstName] = useState<string>('')
  const [lastName, setLastName] = useState<string>('')
  const [description, setDescription] = useState<string>('')
  const [fieldsReady, setFieldsReady] = useState(false)
  const [photoError, setPhotoError] = useState(false)

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
  })

  const updateMutation = useMutation({
    mutationFn: () => updateMasterProfile(profile!.id, description || null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-master-profile'] }),
  })

  const photoMutation = useMutation({
    mutationFn: (file: File) => uploadMasterPhoto(profile!.id, file),
    onSuccess: () => {
      setPhotoError(false)
      queryClient.invalidateQueries({ queryKey: ['my-master-profile'] })
    },
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
    ? `${import.meta.env.VITE_API_URL ?? ''}${profile.photoUrl}`
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
              onError={() => setPhotoError(true)}
              style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover' }}
            />
          )}
          <span style={{ color: '#fff', fontWeight: 700, fontSize: 32 }}>
            {(displayFirstName[0] ?? '') + (lastName?.[0] ?? '')}
          </span>

          {/* Оверлей при загрузке */}
          {photoMutation.isPending && (
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

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div style={{ flex: 1, overflowY: 'auto', minHeight: 0 }}>
        {activeTab === 'profile'      && <ProfileTab />}
        {activeTab === 'schedule'     && <StubTab label="Расписание" />}
        {activeTab === 'services'     && <StubTab label="Услуги" />}
        {activeTab === 'appointments' && <StubTab label="Записи" />}
      </div>

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
