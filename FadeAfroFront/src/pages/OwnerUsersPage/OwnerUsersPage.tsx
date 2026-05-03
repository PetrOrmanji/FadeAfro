import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  getAllUsers, assignMaster, dismissMaster,
  type UserItem,
} from '../../api/user'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './OwnerUsersPage.module.css'

const PAGE_SIZE = 20

const ROLE_LABELS: Record<string, string> = {
  Client: 'Клиент',
  Master: 'Мастер',
  Owner:  'Владелец',
}

const getInitials = (firstName: string, lastName: string | null) =>
  (firstName.charAt(0) + (lastName?.charAt(0) ?? '')).toUpperCase()

// ── Карточка пользователя ──────────────────────────────────────────────────

const UserCard = ({
  user,
  onAssign,
  onDismiss,
}: {
  user: UserItem
  onAssign: (id: string) => void
  onDismiss: (id: string) => void
}) => {
  const [pending, setPending] = useState(false)
  const isMaster = user.roles.includes('Master')
  const isOwner  = user.roles.includes('Owner')

  const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ')
  const roleLabels = user.roles.map(r => ROLE_LABELS[r] ?? r).join(', ')

  const handleAction = async () => {
    if (pending) return
    setPending(true)
    try {
      if (isMaster) await onDismiss(user.id)
      else          await onAssign(user.id)
    } finally {
      setPending(false)
    }
  }

  return (
    <div className={styles.card}>
      <div className={styles.cardAvatar}>
        <span className={styles.cardInitials}>{getInitials(user.firstName, user.lastName)}</span>
      </div>
      <div className={styles.cardInfo}>
        <span className={styles.cardName}>{fullName}</span>
        {user.username && (
          <span className={styles.cardUsername}>@{user.username}</span>
        )}
        <span className={styles.cardRoles}>{roleLabels}</span>
      </div>
      {!isOwner && (
        <button
          className={`${styles.actionBtn} ${isMaster ? styles.actionBtnDismiss : ''}`}
          onClick={handleAction}
          disabled={pending}
        >
          {pending
            ? '...'
            : isMaster ? 'Уволить' : 'Мастер'
          }
        </button>
      )}
    </div>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const OwnerUsersPage = () => {
  useBackButton()
  const navigate = useNavigate()

  const [users,       setUsers]       = useState<UserItem[]>([])
  const [search,      setSearch]      = useState('')
  const [page,        setPage]        = useState(1)
  const [totalPages,  setTotalPages]  = useState(1)
  const [loading,     setLoading]     = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const searchTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  const fetchUsers = async (p: number, q: string, replace: boolean) => {
    try {
      const res = await getAllUsers(p, PAGE_SIZE, q || undefined)
      setUsers(prev => replace ? res.items : [...prev, ...res.items])
      setPage(res.page)
      setTotalPages(res.totalPages)
    } catch {
      navigate('/error', { replace: true })
    }
  }

  // Начальная загрузка
  useEffect(() => {
    setLoading(true)
    fetchUsers(1, '', true).finally(() => setLoading(false))
  }, [])

  // Поиск с дебаунсом
  const handleSearch = (val: string) => {
    setSearch(val)
    if (searchTimer.current) clearTimeout(searchTimer.current)
    searchTimer.current = setTimeout(async () => {
      setLoading(true)
      await fetchUsers(1, val, true)
      setLoading(false)
    }, 400)
  }

  const handleLoadMore = async () => {
    if (loadingMore || page >= totalPages) return
    setLoadingMore(true)
    await fetchUsers(page + 1, search, false)
    setLoadingMore(false)
  }

  const handleAssign = async (userId: string) => {
    await assignMaster(userId)
    await fetchUsers(1, search, true)
  }

  const handleDismiss = async (userId: string) => {
    await dismissMaster(userId)
    await fetchUsers(1, search, true)
  }

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      <div className={styles.header}>
        <div className={styles.logoWrap}>
          <div className={styles.logoPlaceholder}>✂</div>
        </div>
        <h1 className={styles.title}>Пользователи</h1>

        {/* Поиск */}
        <div className={styles.searchWrap}>
          <SearchIcon />
          <input
            className={styles.searchInput}
            placeholder="Имя или username..."
            value={search}
            onChange={e => handleSearch(e.target.value)}
          />
          {search && (
            <button className={styles.searchClear} onClick={() => handleSearch('')}>✕</button>
          )}
        </div>
      </div>

      <div className={styles.list}>
        {users.length === 0 ? (
          <div className={styles.empty}>Пользователи не найдены</div>
        ) : (
          <>
            {users.map(u => (
              <UserCard
                key={u.id}
                user={u}
                onAssign={handleAssign}
                onDismiss={handleDismiss}
              />
            ))}
            {page < totalPages && (
              <button
                className={styles.loadMoreBtn}
                onClick={handleLoadMore}
                disabled={loadingMore}
              >
                {loadingMore ? 'Загрузка...' : 'Загрузить ещё'}
              </button>
            )}
          </>
        )}
      </div>

    </div>
  )
}

// ── Иконка поиска ──────────────────────────────────────────────────────────

const SearchIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
  </svg>
)

export default OwnerUsersPage
