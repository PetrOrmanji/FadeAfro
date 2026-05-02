import { useLaunchParams } from '@tma.js/sdk-react'
import type { UserResponse } from '../../api/user'
import styles from './UserInfoCard.module.css'

interface Props {
  user: UserResponse
  onSettingsClick: () => void
}

const getInitials = (firstName: string, lastName: string | null) => {
  const first = firstName.charAt(0).toUpperCase()
  const last = lastName ? lastName.charAt(0).toUpperCase() : ''
  return first + last
}

const UserInfoCard = ({ user, onSettingsClick }: Props) => {
  const launchParams = useLaunchParams()
  const tgUser = launchParams.tgWebAppData?.user

  const fullName = user.lastName
    ? `${user.firstName} ${user.lastName}`
    : user.firstName

  const username = tgUser?.username ? `@${tgUser.username}` : null
  const photoUrl = typeof tgUser?.photoUrl === 'string' ? tgUser.photoUrl : null
  const initials = getInitials(user.firstName, user.lastName)

  return (
    <div className={styles.card}>
      <div className={styles.left}>
        <div className={styles.avatar}>
          {photoUrl
            ? <img src={photoUrl} alt={fullName} className={styles.avatarImg} />
            : <span className={styles.initials}>{initials}</span>
          }
        </div>
        <div className={styles.info}>
          <span className={styles.name}>{fullName}</span>
          {username && <span className={styles.username}>{username}</span>}
        </div>
      </div>
      <button className={styles.settingsBtn} onClick={onSettingsClick} aria-label="Настройки">
        <SettingsIcon />
      </button>
    </div>
  )
}

const SettingsIcon = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="3" />
    <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
  </svg>
)

export default UserInfoCard
