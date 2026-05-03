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
      .then(setNotifications)
      .finally(() => setLoading(false))
  }, [])

  const handleMarkOne = async (id: string) => {
    await markNotificationAsRead(id)
    setNotifications(prev => prev.filter(n => n.id !== id))
  }

  const handleMarkAll = async () => {
    setMarkingAll(true)
    try {
      await markAllNotificationsAsRead()
      setNotifications([])
    } finally {
      setMarkingAll(false)
    }
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
    <div className={styles.card} onClick={() => onRead(notification.id)}>
      <div className={styles.unreadDot} />
      <p className={styles.cardText}>{notification.text}</p>
    </div>
  )
}

export default NotificationsPage
