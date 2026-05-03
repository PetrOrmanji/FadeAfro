import { useEffect, useState } from 'react'
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
      .then(data => {
        const sorted = [...data].sort((a, b) => Number(a.isRead) - Number(b.isRead))
        setNotifications(sorted)
      })
      .finally(() => setLoading(false))
  }, [])

  const handleMarkOne = async (id: string) => {
    await markNotificationAsRead(id)
    setNotifications(prev =>
      prev.map(n => n.id === id ? { ...n, isRead: true } : n)
    )
  }

  const handleMarkAll = async () => {
    setMarkingAll(true)
    try {
      await markAllNotificationsAsRead()
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })))
    } finally {
      setMarkingAll(false)
    }
  }

  const hasUnread = notifications.some(n => !n.isRead)

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
          {hasUnread && (
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
          <p className={styles.emptyText}>У вас нет уведомлений</p>
        </div>
      ) : (
        <div className={styles.list}>
          {notifications.map(n => (
            <NotificationCard
              key={n.id}
              notification={n}
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
  onRead,
}: {
  notification: NotificationDto
  onRead: (id: string) => void
}) => {
  return (
    <div
      className={`${styles.card} ${notification.isRead ? styles.cardRead : styles.cardUnread}`}
      onClick={() => { if (!notification.isRead) onRead(notification.id) }}
    >
      {!notification.isRead && <div className={styles.unreadDot} />}
      <p className={styles.cardText}>{notification.text}</p>
    </div>
  )
}

export default NotificationsPage
