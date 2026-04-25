import { useState, useEffect, useRef } from 'react'
import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { List, Cell, Section, Spinner, Placeholder } from '@telegram-apps/telegram-ui'
import { getAllUsers, type UserResponse } from '@/api/users'
import { getMasters, type MasterProfile } from '@/api/masters'

// ─── Аватар ──────────────────────────────────────────────────────────────────

function ColoredAvatar({ initials, color }: { initials: string; color: string }) {
  return (
    <div style={{
      width: 44,
      height: 44,
      borderRadius: '50%',
      background: color,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      color: '#fff',
      fontWeight: 600,
      fontSize: 16,
      flexShrink: 0,
    }}>
      {initials}
    </div>
  )
}

// ─── Бейдж роли ──────────────────────────────────────────────────────────────

const ROLE_CONFIG: Record<string, { label: string; color: string }> = {
  Owner:  { label: 'Владелец', color: '#F5A623' },
  Master: { label: 'Мастер',   color: '#3390EC' },
  Client: { label: 'Клиент',   color: '#8E8E93' },
}

function RoleBadge({ role }: { role: string }) {
  const cfg = ROLE_CONFIG[role] ?? { label: role, color: '#8E8E93' }
  return (
    <span style={{
      display: 'inline-block',
      fontSize: 11,
      fontWeight: 600,
      padding: '2px 8px',
      borderRadius: 10,
      background: cfg.color + '22',
      color: cfg.color,
      lineHeight: 1.5,
      whiteSpace: 'nowrap',
    }}>
      {cfg.label}
    </span>
  )
}

// ─── Вспомогательные функции ─────────────────────────────────────────────────

function getRoleColor(roles: string[]): string {
  const hasOwner  = roles.includes('Owner')
  const hasMaster = roles.includes('Master')
  if (hasOwner && hasMaster) return '#8B5CF6'
  if (hasOwner)  return '#F5A623'
  if (hasMaster) return '#3390EC'
  return '#8E8E93'
}

function userInitials(user: UserResponse): string {
  const first = user.firstName[0] ?? ''
  const last  = user.lastName?.[0] ?? ''
  return (first + last).toUpperCase()
}

function userSubtitle(user: UserResponse): string {
  const parts: string[] = []
  if (user.username) parts.push(`@${user.username}`)
  parts.push(String(user.telegramId))
  return parts.join(' · ')
}

function masterInitials(master: MasterProfile): string {
  const first = master.firstName[0] ?? ''
  const last  = master.lastName?.[0] ?? ''
  return (first + last).toUpperCase()
}

// ─── Дебаунс ─────────────────────────────────────────────────────────────────

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState<T>(value)
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])
  return debounced
}

// ─── Фильтр по роли ──────────────────────────────────────────────────────────

type RoleFilter = 'all' | 'Owner' | 'Master' | 'Client'

const ROLE_FILTERS: { key: RoleFilter; label: string }[] = [
  { key: 'all',    label: 'Все'       },
  { key: 'Owner',  label: 'Владельцы' },
  { key: 'Master', label: 'Мастера'   },
  { key: 'Client', label: 'Клиенты'   },
]

// ─── Вкладка: Пользователи ───────────────────────────────────────────────────

const PAGE_SIZE = 20

function UsersTab() {
  const sentinelRef = useRef<HTMLDivElement>(null)
  const [search,     setSearch]     = useState('')
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('all')
  const debouncedSearch = useDebounce(search, 300)

  const { data, isLoading, isError, fetchNextPage, hasNextPage, isFetchingNextPage } =
    useInfiniteQuery({
      queryKey: ['users', debouncedSearch],
      queryFn:  ({ pageParam }) => getAllUsers(pageParam, PAGE_SIZE, debouncedSearch || undefined),
      initialPageParam: 1,
      getNextPageParam: (lastPage) =>
        lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
    })

  useEffect(() => {
    const sentinel = sentinelRef.current
    if (!sentinel) return
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) fetchNextPage()
      },
      { threshold: 0.1 },
    )
    observer.observe(sentinel)
    return () => observer.disconnect()
  }, [hasNextPage, isFetchingNextPage, fetchNextPage])

  const allUsers = data?.pages.flatMap(p => p.items) ?? []
  const users    = roleFilter === 'all'
    ? allUsers
    : allUsers.filter(u => u.roles.includes(roleFilter))

  const totalCount = data?.pages[0]?.totalCount ?? 0

  const chipCounts: Record<RoleFilter, number> = {
    all:    totalCount,
    Owner:  allUsers.filter(u => u.roles.includes('Owner')).length,
    Master: allUsers.filter(u => u.roles.includes('Master')).length,
    Client: allUsers.filter(u => u.roles.includes('Client')).length,
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column' }}>

      {/* ── Sticky шапка ── */}
      <div style={{
        position: 'sticky',
        top: 0,
        zIndex: 10,
        background: 'var(--tgui--secondary_bg_color)',
      }}>
        {/* Поиск */}
        <div style={{ padding: '10px 16px 0' }}>
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            background: 'var(--tgui--bg_color)',
            borderRadius: 12,
            padding: '9px 14px',
            border: '1.5px solid var(--tgui--divider)',
          }}>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" style={{ flexShrink: 0, opacity: 0.4 }}>
              <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="2.2" />
              <path d="M16.5 16.5L21 21" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" />
            </svg>
            <input
              placeholder="Имя, фамилия или @username"
              value={search}
              onChange={e => setSearch(e.target.value)}
              style={{
                flex: 1,
                background: 'transparent',
                border: 'none',
                outline: 'none',
                fontSize: 16,
                color: 'var(--tgui--text_color)',
              }}
            />
            {search && (
              <button
                onClick={() => setSearch('')}
                style={{
                  background: 'var(--tgui--hint_color)',
                  border: 'none',
                  borderRadius: '50%',
                  width: 18,
                  height: 18,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  cursor: 'pointer',
                  flexShrink: 0,
                  padding: 0,
                  opacity: 0.5,
                }}
              >
                <svg width="10" height="10" viewBox="0 0 10 10" fill="none">
                  <path d="M2 2L8 8M8 2L2 8" stroke="#fff" strokeWidth="1.8" strokeLinecap="round" />
                </svg>
              </button>
            )}
          </div>
        </div>

        {/* Чипсы-фильтры со счётчиком */}
        <div style={{
          display: 'flex',
          gap: 8,
          padding: '8px 16px 10px',
          overflowX: 'auto',
          scrollbarWidth: 'none',
        }}>
          {ROLE_FILTERS.map(f => {
            const active = roleFilter === f.key
            const count  = chipCounts[f.key]
            return (
              <button
                key={f.key}
                onClick={() => setRoleFilter(f.key)}
                style={{
                  flexShrink: 0,
                  padding: '5px 13px',
                  borderRadius: 20,
                  border: active ? 'none' : '1px solid var(--tgui--divider)',
                  cursor: 'pointer',
                  fontSize: 13,
                  fontWeight: active ? 600 : 400,
                  background: active ? 'var(--tgui--button_color)' : 'transparent',
                  color: active ? '#fff' : 'var(--tgui--hint_color)',
                  transition: 'background 0.15s, color 0.15s, border-color 0.15s',
                  lineHeight: 1.4,
                }}
              >
                {f.label}
                {!isLoading && count > 0 && (
                  <span style={{
                    marginLeft: 5,
                    fontSize: 11,
                    fontWeight: active ? 700 : 500,
                    opacity: active ? 0.85 : 0.6,
                  }}>
                    {count}
                  </span>
                )}
              </button>
            )
          })}
        </div>

        <div style={{ height: 1, background: 'var(--tgui--divider)' }} />
      </div>

      {/* ── Контент ── */}
      {isLoading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 32 }}>
          <Spinner size="l" />
        </div>
      ) : isError ? (
        <Placeholder header="Ошибка" description="Не удалось загрузить пользователей" />
      ) : users.length === 0 ? (
        <Placeholder header="Ничего не найдено" />
      ) : (
        <List>
          <Section>
            {users.map(user => (
              <Cell
                key={user.id}
                before={
                  <ColoredAvatar
                    initials={userInitials(user)}
                    color={getRoleColor(user.roles)}
                  />
                }
              >
                <div style={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'flex-start',
                  gap: 4,
                  width: '100%',
                }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
                    <span style={{ fontWeight: 500 }}>
                      {[user.firstName, user.lastName].filter(Boolean).join(' ')}
                    </span>
                    <div style={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                      {user.roles.map(role => <RoleBadge key={role} role={role} />)}
                    </div>
                  </div>
                  <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>
                    {userSubtitle(user)}
                  </span>
                </div>
              </Cell>
            ))}
          </Section>
        </List>
      )}

      <div ref={sentinelRef} style={{ height: 1 }} />
      {isFetchingNextPage && (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 16 }}>
          <Spinner size="m" />
        </div>
      )}
    </div>
  )
}

// ─── Вкладка: Мастера ────────────────────────────────────────────────────────

function MastersTab() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['masters'],
    queryFn:  getMasters,
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
            <div style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'flex-start',
              gap: 4,
              width: '100%',
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span style={{ fontWeight: 500 }}>
                  {[master.firstName, master.lastName].filter(Boolean).join(' ')}
                </span>
                <RoleBadge role="Master" />
              </div>
              {master.description && (
                <span style={{ fontSize: 13, color: 'var(--tgui--hint_color)' }}>
                  {master.description}
                </span>
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
          { key: 'users',   label: 'Пользователи', icon: '👥' },
          { key: 'masters', label: 'Мастера',       icon: '✂️' },
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
