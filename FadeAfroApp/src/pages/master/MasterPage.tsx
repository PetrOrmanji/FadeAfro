import { useState, useRef, useEffect } from 'react'
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
const DAYS: { dayOfWeek: number; label: string }[] = [
  { dayOfWeek: 1, label: 'Понедельник' },
  { dayOfWeek: 2, label: 'Вторник' },
  { dayOfWeek: 3, label: 'Среда' },
  { dayOfWeek: 4, label: 'Четверг' },
  { dayOfWeek: 5, label: 'Пятница' },
  { dayOfWeek: 6, label: 'Суббота' },
  { dayOfWeek: 0, label: 'Воскресенье' },
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

function ScheduleTab() {
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

  return (
    <div style={{ flex: 1, overflowY: 'auto', padding: '16px' }}>

        <div style={{
          borderRadius: 16,
          background: 'var(--tgui--bg_color)',
          overflow: 'hidden',
        }}>
          {DAYS.map(({ dayOfWeek, label }, idx) => {
            const day = days[dayOfWeek]
            if (!day) return null
            const isSaving = !!savingDays[dayOfWeek]
            const hasError = !!errorDays[dayOfWeek]
            return (
              <div key={dayOfWeek}>
                {idx > 0 && (
                  <div style={{ height: 1, background: 'var(--tgui--divider)', margin: '0 16px' }} />
                )}
                <div style={{
                  padding: '12px 16px',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 12,
                  opacity: day.enabled ? 1 : 0.45,
                  transition: 'opacity 0.15s',
                }}>
                  {/* Тогл */}
                  <div
                    onClick={() => handleToggle(dayOfWeek)}
                    style={{
                      width: 40,
                      height: 24,
                      borderRadius: 12,
                      background: day.enabled ? 'var(--tgui--button_color)' : 'var(--tgui--hint_color)',
                      position: 'relative',
                      cursor: isSaving ? 'default' : 'pointer',
                      flexShrink: 0,
                      transition: 'background 0.2s',
                    }}
                  >
                    {isSaving ? (
                      <div style={{
                        position: 'absolute',
                        inset: 0,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                      }}>
                        <Spinner size="s" />
                      </div>
                    ) : (
                      <div style={{
                        position: 'absolute',
                        top: 3,
                        left: day.enabled ? 19 : 3,
                        width: 18,
                        height: 18,
                        borderRadius: '50%',
                        background: '#fff',
                        transition: 'left 0.2s',
                        boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
                      }} />
                    )}
                  </div>

                  {/* Название */}
                  <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <span style={{
                      fontSize: 15,
                      color: 'var(--tgui--text_color)',
                      fontWeight: 400,
                    }}>
                      {label}
                    </span>
                    {hasError && (
                      <span style={{ fontSize: 11, color: '#FF3B30' }}>Не сохранено</span>
                    )}
                  </div>

                  {/* Время */}
                  {day.enabled ? (
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input
                        type="time"
                        value={day.startTime}
                        disabled={isSaving}
                        onChange={e => updateDay(dayOfWeek, { startTime: e.target.value })}
                        onBlur={() => handleTimeBlur(dayOfWeek)}
                        style={{
                          border: 'none',
                          background: 'var(--tgui--secondary_bg_color)',
                          borderRadius: 8,
                          padding: '4px 8px',
                          fontSize: 14,
                          color: 'var(--tgui--text_color)',
                          fontFamily: 'inherit',
                          outline: 'none',
                          cursor: isSaving ? 'default' : 'pointer',
                          opacity: isSaving ? 0.5 : 1,
                        }}
                      />
                      <span style={{ color: 'var(--tgui--hint_color)', fontSize: 13 }}>—</span>
                      <input
                        type="time"
                        value={day.endTime}
                        disabled={isSaving}
                        onChange={e => updateDay(dayOfWeek, { endTime: e.target.value })}
                        onBlur={() => handleTimeBlur(dayOfWeek)}
                        style={{
                          border: 'none',
                          background: 'var(--tgui--secondary_bg_color)',
                          borderRadius: 8,
                          padding: '4px 8px',
                          fontSize: 14,
                          color: 'var(--tgui--text_color)',
                          fontFamily: 'inherit',
                          outline: 'none',
                          cursor: isSaving ? 'default' : 'pointer',
                          opacity: isSaving ? 0.5 : 1,
                        }}
                      />
                    </div>
                  ) : (
                    <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>выходной</span>
                  )}
                </div>
              </div>
            )
          })}
        </div>
    </div>
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

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div style={{ flex: 1, overflowY: 'auto', minHeight: 0 }}>
        {activeTab === 'profile'      && <ProfileTab />}
        {activeTab === 'schedule'     && <ScheduleTab />}
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
