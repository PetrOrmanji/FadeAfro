import { useEffect, useRef, useState } from 'react'
import type { NotificationDto } from '../../api/notifications'
import { getMyNotifications, markAllNotificationsAsRead, markNotificationAsRead } from '../../api/notifications'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './NotificationsPage.module.css'

const NotificationsPage = () => {
  useBackButton()
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [loading, setLoading] = useState(true)
  const [markingAll, setMarkingAll] = useState(false)

  useEffect(() => {
    getMyNotifications()
      .then(setNotifications)
      .finally(() => setLoading(false))
  }, [])

  const [removingIds, setRemovingIds] = useState<Set<string>>(new Set())

  const handleMarkOne = (id: string) => {
    setRemovingIds(prev => new Set(prev).add(id))
    markNotificationAsRead(id)
    setTimeout(() => {
      setNotifications(prev => prev.filter(n => n.id !== id))
      setRemovingIds(prev => { const s = new Set(prev); s.delete(id); return s })
    }, 350)
  }

  const handleMarkAll = async () => {
    setMarkingAll(true)
    markAllNotificationsAsRead()

    // запускаем анимацию на каждой карточке с задержкой
    const ids = notifications.map(n => n.id)
    ids.forEach((id, i) => {
      setTimeout(() => {
        setRemovingIds(prev => new Set(prev).add(id))
      }, i * 60)
    })

    // убираем из стейта после завершения всех анимаций
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
        <div className={styles.empty}>
          <p className={styles.emptyText}>Нет новых уведомлений</p>
        </div>
      ) : (
        <div className={styles.list}>
          {notifications.map(n => (
            <NotificationCard
              key={n.id}
              notification={n}
              removing={removingIds.has(n.id)}
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
  onRead,
}: {
  notification: NotificationDto
  removing: boolean
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
      onClick={() => { if (!removing) onRead(notification.id) }}
    >
      <div className={styles.unreadDot} />
      <p className={styles.cardText}>{notification.text}</p>
    </div>
  )
}

export default NotificationsPage
