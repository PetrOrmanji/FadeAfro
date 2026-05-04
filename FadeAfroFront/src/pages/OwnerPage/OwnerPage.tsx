import logo from '../../assets/logo.png'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { hasRole, useAuth } from '../../context/AuthContext'
import { getMe, type UserResponse } from '../../api/user'
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import styles from './OwnerPage.module.css'

type Tab = 'owner' | 'master'

const OwnerPage = () => {
  const navigate   = useNavigate()
  const { roles }  = useAuth()
  const isMaster   = hasRole(roles, 'Master')

  const [tab,          setTab]          = useState<Tab>('owner')
  const [user,         setUser]         = useState<UserResponse | null>(null)
  const [loading,      setLoading]      = useState(true)

  useEffect(() => {
    getMe()
      .then(userData => {
        setUser(userData)
      })
      .catch(() => navigate('/error'))
      .finally(() => setLoading(false))
  }, [navigate])

  const handleMasterTab = () => {
    navigate('/master', { state: { from: 'owner' } })
  }

  if (loading) return <LoadingScreen />

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <img src={logo} className={styles.logo} alt="FadeAfro" />
      </div>

      {/* Карточка пользователя */}
      {user && (
        <UserInfoCard
          user={user}
          onSettingsClick={() => navigate('/owner/settings')}
        />
      )}

      {/* Таб-свитчер — только если владелец тоже мастер */}
      {isMaster && (
        <div className={styles.tabSwitcher}>
          <button
            className={`${styles.tabBtn} ${tab === 'owner' ? styles.tabBtnActive : ''}`}
            onClick={() => setTab('owner')}
          >
            Владелец
          </button>
          <button
            className={`${styles.tabBtn} ${tab === 'master' ? styles.tabBtnActive : ''}`}
            onClick={handleMasterTab}
          >
            Мастер
          </button>
        </div>
      )}

      {/* Контент владельца */}
      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Управление</h2>
        <div className={styles.card} onClick={() => navigate('/owner/users')}>
          <div className={styles.cardLeft}>
            <UsersIcon />
            <span className={styles.cardLabel}>Пользователи</span>
          </div>
          <ChevronRightIcon />
        </div>
      </div>

    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const UsersIcon = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
    <circle cx="12" cy="7" r="4" />
  </svg>
)

const ChevronRightIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="9 18 15 12 9 6" />
  </svg>
)

export default OwnerPage
