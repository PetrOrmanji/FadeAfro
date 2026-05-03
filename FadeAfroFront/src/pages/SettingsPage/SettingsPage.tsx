import { useEffect, useState } from 'react'
import { useLaunchParams } from '@tma.js/sdk-react'
import { getMe, type UserResponse } from '../../api/user'
import { useAuth, type Role } from '../../context/AuthContext'
import useBackButton from '../../hooks/useBackButton'
import styles from './SettingsPage.module.css'

const ROLE_LABELS: Record<Role, string> = {
  Client: 'Клиент',
  Master: 'Мастер',
  Owner: 'Владелец',
}

const getInitials = (firstName: string, lastName: string | null) => {
  const first = firstName.charAt(0).toUpperCase()
  const last = lastName ? lastName.charAt(0).toUpperCase() : ''
  return first + last
}

const SettingsPage = () => {
  useBackButton()
  const { roles } = useAuth()
  const launchParams = useLaunchParams()
  const tgUser = launchParams.tgWebAppData?.user

  const [user, setUser] = useState<UserResponse | null>(null)

  useEffect(() => {
    getMe().then(setUser)
  }, [])

  const fullName = user
    ? [user.firstName, user.lastName].filter(Boolean).join(' ')
    : '...'

  const username = tgUser?.username ? `@${tgUser.username}` : null
  const telegramId = tgUser?.id ?? null
  const photoUrl = typeof tgUser?.photoUrl === 'string' ? tgUser.photoUrl : null
  const initials = user ? getInitials(user.firstName, user.lastName) : '?'

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      {/* Заголовок */}
      <h1 className={styles.title}>Настройки профиля</h1>

      {/* Карточка профиля */}
      <div className={styles.profileCard}>
        <div className={styles.avatar}>
          {photoUrl
            ? <img src={photoUrl} alt={fullName} className={styles.avatarImg} />
            : <span className={styles.initials}>{initials}</span>
          }
        </div>
        <div className={styles.info}>
          <span className={styles.name}>{fullName}</span>
          <div className={styles.subInfo}>
            {username   && <span className={styles.username}>{username}</span>}
            {telegramId && <span className={styles.userId}>ID: {telegramId}</span>}
          </div>
        </div>
      </div>

      {/* Роли */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>РОЛИ</span>
        <div className={styles.rolesCard}>
          {roles.map(role => (
            <span key={role} className={styles.roleItem}>
              {ROLE_LABELS[role]}
            </span>
          ))}
        </div>
      </section>

    </div>
  )
}

export default SettingsPage
