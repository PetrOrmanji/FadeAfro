import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  getAllUsers, assignMaster, dismissMaster, getMe,
  type UserItem,
} from '../../api/user'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './OwnerUsersPage.module.css'

const PAGE_SIZE = 20

const getInitials = (firstName: string, lastName: string | null) =>
  (firstName.charAt(0) + (lastName?.charAt(0) ?? '')).toUpperCase()

// ── Чипы ролей ────────────────────────────────────────────────────────────

const RoleChip = ({ role }: { role: string }) => {
  const map: Record<string, { label: string; cls: string }> = {
    Owner:  { label: 'Владелец', cls: styles.chipOwner },
    Master: { label: 'Мастер',   cls: styles.chipMaster },
    Client: { label: 'Клиент',   cls: styles.chipClient },
  }
  const { label, cls } = map[role] ?? { label: role, cls: '' }
  return <span className={`${styles.chip} ${cls}`}>{label}</span>
}

// ── Карточка пользователя ──────────────────────────────────────────────────

const UserCard = ({
  user,
  isMe,
  onClick,
}: {
  user: UserItem
  isMe: boolean
  onClick: () => void
}) => {
  const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ')

  return (
    <div
      className={styles.card}
      onClick={onClick}
      style={{ cursor: 'pointer' }}
    >
      <div className={styles.cardAvatar}>
        <span className={styles.cardInitials}>{getInitials(user.firstName, user.lastName)}</span>
      </div>
      <div className={styles.cardInfo}>
        <span className={styles.cardName}>{fullName}</span>
        {user.username && (
          <span className={styles.cardUsername}>@{user.username}</span>
        )}
        <div className={styles.chipRow}>
          {isMe && <span className={`${styles.chip} ${styles.chipMe}`}>Вы</span>}
          {user.roles.map(r => <RoleChip key={r} role={r} />)}
        </div>
      </div>
    </div>
  )
}

// ── Нижняя панель ──────────────────────────────────────────────────────────

const UserActionPanel = ({
  user,
  isMe,
  onAssign,
  onDismiss,
  onClose,
}: {
  user: UserItem
  isMe: boolean
  onAssign: (id: string) => Promise<void>
  onDismiss: (id: string) => Promise<void>
  onClose: () => void
}) => {
  const [pending,      setPending]      = useState(false)
  const [confirmMode,  setConfirmMode]  = useState(false)
  const isMaster = user.roles.includes('Master')
  const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ')

  const handleAssign = async () => {
    setPending(true)
    try {
      await onAssign(user.id)
      onClose()
    } finally {
      setPending(false)
    }
  }

  const handleDismissConfirmed = async () => {
    setPending(true)
    try {
      await onDismiss(user.id)
      onClose()
    } finally {
      setPending(false)
    }
  }

  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={styles.bottomPanel}>
        <div className={styles.panelHandle} />

        {confirmMode ? (
          /* ── Экран подтверждения увольнения ── */
          <>
            <div className={styles.panelWarningIcon}>⚠️</div>
            <div className={styles.panelWarningTitle}>
              {isMe ? 'Снять роль мастера?' : `Уволить ${fullName}?`}
            </div>
            <div className={styles.panelWarningList}>
              <div className={styles.panelWarningItem}>
                <span className={styles.panelWarningDot} />
                Все активные записи будут отменены
              </div>
              <div className={styles.panelWarningItem}>
                <span className={styles.panelWarningDot} />
                Клиенты получат уведомления об отмене
              </div>
              <div className={styles.panelWarningItem}>
                <span className={styles.panelWarningDot} />
                {isMe ? 'Вы потеряете роль мастера' : 'Пользователь станет обычным клиентом'}
              </div>
            </div>
            <button
              className={`${styles.panelBtn} ${styles.panelBtnDismiss}`}
              onClick={handleDismissConfirmed}
              disabled={pending}
            >
              {pending ? 'Выполняется...' : 'Подтвердить'}
            </button>
            <button className={styles.panelBtnCancel} onClick={() => setConfirmMode(false)}>
              Назад
            </button>
          </>
        ) : (
          /* ── Главный экран панели ── */
          <>
            <div className={styles.panelUser}>
              <div className={styles.panelAvatar}>
                <span className={styles.cardInitials}>{getInitials(user.firstName, user.lastName)}</span>
              </div>
              <div>
                <div className={styles.panelName}>{fullName}</div>
                {user.username && (
                  <div className={styles.panelUsername}>@{user.username}</div>
                )}
              </div>
            </div>
            {isMaster ? (
              <button
                className={`${styles.panelBtn} ${styles.panelBtnDismiss}`}
                onClick={() => setConfirmMode(true)}
              >
                {isMe ? 'Снять роль мастера' : 'Уволить мастера'}
              </button>
            ) : (
              <button
                className={styles.panelBtn}
                onClick={handleAssign}
                disabled={pending}
              >
                {pending ? '...' : 'Назначить мастером'}
              </button>
            )}
            <button className={styles.panelBtnCancel} onClick={onClose}>
              Отмена
            </button>
          </>
        )}
      </div>
    </>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const OwnerUsersPage = () => {
  useBackButton()
  const navigate = useNavigate()

  const [myId,         setMyId]         = useState<string | null>(null)
  const [users,        setUsers]        = useState<UserItem[]>([])
  const [selectedUser, setSelectedUser] = useState<UserItem | null>(null)
  const [search,       setSearch]       = useState('')
  const [page,         setPage]         = useState(1)
  const [totalPages,   setTotalPages]   = useState(1)
  const [loading,      setLoading]      = useState(true)
  const [loadingMore,  setLoadingMore]  = useState(false)
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

  useEffect(() => {
    setLoading(true)
    Promise.all([
      fetchUsers(1, '', true),
      getMe().then(me => setMyId(me.id)).catch(() => {}),
    ]).finally(() => setLoading(false))
  }, [])

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
                isMe={u.id === myId}
                onClick={() => setSelectedUser(u)}
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

      {selectedUser && (
        <UserActionPanel
          user={selectedUser}
          isMe={selectedUser.id === myId}
          onAssign={handleAssign}
          onDismiss={handleDismiss}
          onClose={() => setSelectedUser(null)}
        />
      )}

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
