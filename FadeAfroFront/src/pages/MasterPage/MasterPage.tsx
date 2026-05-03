import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getMe, type UserResponse } from '../../api/user'
import { getUnreadNotificationsCount } from '../../api/notifications'
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import styles from './MasterPage.module.css'

const MasterPage = () => {
  const navigate = useNavigate()
  const [user,        setUser]        = useState<UserResponse | null>(null)
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading,     setLoading]     = useState(true)

  useEffect(() => {
    Promise.all([getMe(), getUnreadNotificationsCount()])
      .then(([userData, unreadCountData]) => {
        setUser(userData)
        setUnreadCount(unreadCountData)
      })
      .catch(() => navigate('/error'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      {/* Карточка пользователя */}
      {user && (
        <UserInfoCard
          user={user}
          unreadCount={unreadCount}
          onSettingsClick={() => navigate('/master/settings')}
          onNotificationsClick={() => navigate('/master/notifications')}
        />
      )}

    </div>
  )
}

export default MasterPage
