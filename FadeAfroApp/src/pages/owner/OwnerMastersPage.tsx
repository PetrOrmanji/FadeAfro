import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { List, Cell, Section, Spinner, Placeholder } from '@telegram-apps/telegram-ui'
import { getAllUsers, type UserResponse } from '@/api/users'
import { getMasters, type MasterProfile } from '@/api/masters'

// ─── Аватар ──────────────────────────────────────────────────────────────────

function ColoredAvatar({ initials, color }: { initials: string; color: string }) {
  return (
    <div style={{
      width: 40,
      height: 40,
      borderRadius: '50%',
      background: color,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      color: '#fff',
      fontWeight: 600,
      fontSize: 15,
      flexShrink: 0,
    }}>
      {initials}
    </div>
  )
}

// ─── Роли ───────────────────────────────────────────────────────────────────

function getRoleColor(roles: string[]): string {
  const hasOwner = roles.includes('Owner')
  const hasMaster = roles.includes('Master')
  if (hasOwner && hasMaster) return '#8B5CF6'
  if (hasOwner) return '#F5A623'
  if (hasMaster) return '#3390EC'
  return '#8E8E93'
}

function userInitials(user: UserResponse): string {
  const first = user.firstName[0] ?? ''
  const last = user.lastName?.[0] ?? ''
  return (first + last).toUpperCase()
}

function userSubtitle(user: UserResponse): string {
  const parts: string[] = []
  if (user.username) parts.push(`@${user.username}`)
  parts.push(String(user.telegramId))
  return parts.join(' · ')
}

// ─── Пагинация ───────────────────────────────────────────────────────────────

interface PaginationProps {
  page: number
  totalPages: number
  onChange: (page: number) => void
}

function Pagination({ page, totalPages, onChange }: PaginationProps) {
  if (totalPages <= 1) return null

  const range = 2
  const start = Math.max(1, page - range)
  const end = Math.min(totalPages, page + range)
  const pages = Array.from({ length: end - start + 1 }, (_, i) => start + i)

  const btnStyle = (active: boolean): React.CSSProperties => ({
    minWidth: 36,
    height: 36,
    borderRadius: 8,
    border: 'none',
    cursor: active ? 'default' : 'pointer',
    fontWeight: active ? 700 : 400,
    background: active ? 'var(--tgui--button_color)' : 'transparent',
    color: active ? 'var(--tgui--button_text_color)' : 'var(--tgui--text_color)',
    opacity: 1,
  })

  const arrowStyle = (disabled: boolean): React.CSSProperties => ({
    minWidth: 36,
    height: 36,
    borderRadius: 8,
    border: 'none',
    cursor: disabled ? 'default' : 'pointer',
    background: 'transparent',
    color: disabled ? 'var(--tgui--hint_color)' : 'var(--tgui--text_color)',
    fontSize: 18,
  })

  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 4, padding: '12px 16px' }}>
      <button style={arrowStyle(page === 1)} disabled={page === 1} onClick={() => onChange(page - 1)}>
        ‹
      </button>
      {start > 1 && (
        <>
          <button style={btnStyle(false)} onClick={() => onChange(1)}>1</button>
          {start > 2 && <span style={{ color: 'var(--tgui--hint_color)' }}>…</span>}
        </>
      )}
      {pages.map(p => (
        <button key={p} style={btnStyle(p === page)} disabled={p === page} onClick={() => onChange(p)}>
          {p}
        </button>
      ))}
      {end < totalPages && (
        <>
          {end < totalPages - 1 && <span style={{ color: 'var(--tgui--hint_color)' }}>…</span>}
          <button style={btnStyle(false)} onClick={() => onChange(totalPages)}>{totalPages}</button>
        </>
      )}
      <button style={arrowStyle(page === totalPages)} disabled={page === totalPages} onClick={() => onChange(page + 1)}>
        ›
      </button>
    </div>
  )
}

// ─── Вкладка: Пользователи ───────────────────────────────────────────────────

const PAGE_SIZE = 20

function UsersTab() {
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['users', page],
    queryFn: () => getAllUsers(page, PAGE_SIZE),
  })

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60dvh' }}>
        <Spinner size="l" />
      </div>
    )
  }

  if (isError) {
    return <Placeholder header="Ошибка" description="Не удалось загрузить пользователей" />
  }

  const users = data?.items ?? []

  if (users.length === 0) {
    return <Placeholder header="Пользователей нет" />
  }

  return (
    <>
      <List>
        <Section>
          {users.map(user => (
            <Cell
              key={user.id}
              before={<ColoredAvatar initials={userInitials(user)} color={getRoleColor(user.roles)} />}
            >
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start', gap: 2, width: '100%' }}>
                <span>{[user.firstName, user.lastName].filter(Boolean).join(' ')}</span>
                <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{userSubtitle(user)}</span>
              </div>
            </Cell>
          ))}
        </Section>
      </List>
      <Pagination
        page={page}
        totalPages={data?.totalPages ?? 1}
        onChange={setPage}
      />
    </>
  )
}

// ─── Вкладка: Мастера ────────────────────────────────────────────────────────

function masterInitials(master: MasterProfile): string {
  const first = master.firstName[0] ?? ''
  const last = master.lastName?.[0] ?? ''
  return (first + last).toUpperCase()
}

function MastersTab() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['masters'],
    queryFn: getMasters,
  })

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60dvh' }}>
        <Spinner size="l" />
      </div>
    )
  }

  if (isError) {
    return <Placeholder header="Ошибка" description="Не удалось загрузить мастеров" />
  }

  const masters = data?.masters ?? []

  if (masters.length === 0) {
    return <Placeholder header="Мастеров пока нет" description="Добавьте первого мастера" />
  }

  return (
    <List>
      <Section>
        {masters.map(master => (
          <Cell
            key={master.id}
            before={<ColoredAvatar initials={masterInitials(master)} color="#3390EC" />}
          >
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start', gap: 2, width: '100%' }}>
              <span>{[master.firstName, master.lastName].filter(Boolean).join(' ')}</span>
              {master.description && (
                <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>{master.description}</span>
              )}
            </div>
          </Cell>
        ))}
      </Section>
    </List>
  )
}

// ─── Страница ─────────────────────────────────────────────────────────────────

type Tab = 'users' | 'masters'

export function OwnerMastersPage() {
  const [activeTab, setActiveTab] = useState<Tab>('users')

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100dvh' }}>
      <div style={{ flex: 1, overflowY: 'auto' }}>
        {activeTab === 'users' ? <UsersTab /> : <MastersTab />}
      </div>

      <nav style={{
        display: 'flex',
        borderTop: '1px solid var(--tgui--divider)',
        background: 'var(--tgui--secondary_bg_color)',
      }}>
        {([
          { key: 'users', label: 'Пользователи', icon: '👥' },
          { key: 'masters', label: 'Мастера', icon: '✂️' },
        ] as const).map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            style={{
              flex: 1,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: 4,
              padding: '10px 0 14px',
              border: 'none',
              background: 'transparent',
              cursor: 'pointer',
              color: activeTab === tab.key ? 'var(--tgui--button_color)' : 'var(--tgui--hint_color)',
              fontSize: 10,
              fontWeight: activeTab === tab.key ? 600 : 400,
            }}
          >
            <span style={{ fontSize: 22 }}>{tab.icon}</span>
            {tab.label}
          </button>
        ))}
      </nav>
    </div>
  )
}
