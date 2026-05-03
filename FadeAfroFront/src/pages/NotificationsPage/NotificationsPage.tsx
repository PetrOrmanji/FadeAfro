import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { NotificationDto } from '../../api/notifications'
import { getMyNotifications, markAllNotificationsAsRead, markNotificationAsRead } from '../../api/notifications'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './NotificationsPage.module.css'

const NotificationsPage = () => {
  useBackButton()
  const navigate = useNavigate()
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [loading, setLoading] = useState(true)
  const [markingAll, setMarkingAll] = useState(false)

  useEffect(() => {
    getMyNotifications()
      .then(setNotifications)
      .catch(() => navigate('/error'))
      .finally(() => setLoading(false))
  }, [navigate])

  const [removingIds, setRemovingIds] = useState<Set<string>>(new Set())
  const [pendingIds,  setPendingIds]  = useState<Set<string>>(new Set())

  const handleMarkOne = async (id: string) => {
    setPendingIds(prev => new Set(prev).add(id))

    try {
      await markNotificationAsRead(id)
    } catch {
      setPendingIds(prev => { const s = new Set(prev); s.delete(id); return s })
      navigate('/error', { replace: true })
      return
    }

    // API успешен — запускаем анимацию
    setPendingIds(prev => { const s = new Set(prev); s.delete(id); return s })
    setRemovingIds(prev => new Set(prev).add(id))
    setTimeout(() => {
      setNotifications(prev => prev.filter(n => n.id !== id))
      setRemovingIds(prev => { const s = new Set(prev); s.delete(id); return s })
    }, 350)
  }

  const handleMarkAll = async () => {
    setMarkingAll(true)

    try {
      await markAllNotificationsAsRead()
    } catch {
      setMarkingAll(false)
      navigate('/error', { replace: true })
      return
    }

    // API успешен — запускаем стаггер-анимацию
    const ids = notifications.map(n => n.id)
    ids.forEach((id, i) => {
      setTimeout(() => {
        setRemovingIds(prev => new Set(prev).add(id))
      }, i * 60)
    })

    setTimeout(() => {
      setNotifications([])
      setRemovingIds(new Set())
      setMarkingAll(false)
    }, ids.length * 60 + 350)
  }

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Шапка — не листается */}
      <div className={styles.header}>
        <div className={styles.logoWrap}>
          <div className={styles.logoPlaceholder}>✂</div>
        </div>
        <div className={styles.titleRow}>
          <h1 className={styles.title}>Уведомления</h1>
          {notifications.length > 0 && (
            <button
              className={styles.markAllBtn}
              onClick={handleMarkAll}
              disabled={markingAll}
            >
              {markingAll ? 'Читаем...' : 'Прочитать все'}
            </button>
          )}
        </div>
      </div>

      {/* Список — листается */}
      {notifications.length === 0 ? (
        <div className={styles.emptyCard}>
          <EmptyBellIllustration />
          <p className={styles.emptyTitle}>Всё прочитано</p>
          <p className={styles.emptyText}>Новых уведомлений нет</p>
        </div>
      ) : (
        <div className={styles.list}>
          {notifications.map(n => (
            <NotificationCard
              key={n.id}
              notification={n}
              removing={removingIds.has(n.id)}
              pending={pendingIds.has(n.id)}
              onRead={handleMarkOne}
            />
          ))}
        </div>
      )}

    </div>
  )
}

// ── Карточка уведомления ────────────────────────────────────────────────────

const NotificationCard = ({
  notification,
  removing,
  pending,
  onRead,
}: {
  notification: NotificationDto
  removing: boolean
  pending: boolean
  onRead: (id: string) => void
}) => {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!removing || !ref.current) return
    const el = ref.current
    const height = el.offsetHeight
    el.style.height = `${height}px`
    el.style.overflow = 'hidden'
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        el.style.transition = 'height 0.3s ease, padding 0.3s ease, margin 0.3s ease, opacity 0.2s ease, transform 0.2s ease'
        el.style.height = '0'
        el.style.paddingTop = '0'
        el.style.paddingBottom = '0'
        el.style.marginBottom = '0'
        el.style.opacity = '0'
        el.style.transform = 'translateX(50px)'
      })
    })
  }, [removing])

  return (
    <div
      ref={ref}
      className={styles.card}
      onClick={() => { if (!removing && !pending) onRead(notification.id) }}
    >
      <div className={styles.unreadDot} />
      <p className={styles.cardText}>{notification.text}</p>
    </div>
  )
}

const EmptyBellIllustration = () => (
  <svg width="140" height="140" viewBox="0 0 140 140" fill="none" xmlns="http://www.w3.org/2000/svg">
    {/* Фоновый круг */}
    <circle cx="70" cy="70" r="56" fill="var(--tg-button)" opacity="0.08"/>
    {/* Колокольчик */}
    <path d="M70 32C70 32 46 46 46 72V90H94V72C94 46 70 32 70 32Z"
      fill="var(--tg-button)" opacity="0.18"
      stroke="var(--tg-button)" strokeWidth="3.5" strokeLinejoin="round"/>
    {/* Полоска снизу колокольчика */}
    <rect x="38" y="88" width="64" height="8" rx="4" fill="var(--tg-button)" opacity="0.22"/>
    {/* Дужка */}
    <path d="M60 96C60 101.5 64.5 106 70 106C75.5 106 80 101.5 80 96"
      stroke="var(--tg-button)" strokeWidth="3.5" strokeLinecap="round" fill="none"/>
    {/* Точка сверху */}
    <circle cx="70" cy="32" r="5" fill="var(--tg-button)" opacity="0.5"/>
    {/* Диагональная линия-зачёркивание */}
    <line x1="42" y1="42" x2="98" y2="98" stroke="var(--tg-secondary-bg)" strokeWidth="7" strokeLinecap="round"/>
    <line x1="42" y1="42" x2="98" y2="98" stroke="var(--tg-button)" strokeWidth="4" strokeLinecap="round" opacity="0.7"/>
  </svg>
)

export default NotificationsPage
