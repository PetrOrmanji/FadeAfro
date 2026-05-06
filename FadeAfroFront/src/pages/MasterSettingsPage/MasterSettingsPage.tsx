import logo from '../../assets/logo.png'
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLaunchParams } from '@tma.js/sdk-react'
import axios from 'axios'
import heic2any from 'heic2any'
import { getMe, type UserResponse } from '../../api/user'
import { getMyMasterProfile, getMasterPhotoUrl, uploadMasterPhoto, type MasterProfile } from '../../api/masters'
import { isRateLimitError, showRateLimitAlert } from '../../api/errors'
import { useAuth, type Role } from '../../context/AuthContext'
import useBackButton from '../../hooks/useBackButton'
import styles from './MasterSettingsPage.module.css'

const isHeif = (file: File) => {
  const ext = file.name.split('.').pop()?.toLowerCase()
  return ext === 'heic' || ext === 'heif'
    || file.type === 'image/heic' || file.type === 'image/heif'
}

const convertToJpegIfNeeded = async (file: File): Promise<File> => {
  if (!isHeif(file)) return file
  const blob = await heic2any({ blob: file, toType: 'image/jpeg', quality: 0.92 })
  const result = Array.isArray(blob) ? blob[0] : blob
  const newName = file.name.replace(/\.(heic|heif)$/i, '.jpg')
  return new File([result], newName, { type: 'image/jpeg' })
}

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

const MasterSettingsPage = () => {
  useBackButton()
  const navigate = useNavigate()
  const { roles } = useAuth()
  const launchParams = useLaunchParams()
  const tgUser = launchParams.tgWebAppData?.user

  const [user,          setUser]          = useState<UserResponse | null>(null)
  const [masterProfile, setMasterProfile] = useState<MasterProfile | null>(null)
  const [uploading,     setUploading]     = useState(false)
  const [cacheBust,     setCacheBust]     = useState<number>(Date.now())
  const [uploadError,   setUploadError]   = useState<string | null>(null)

  const fileInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    Promise.all([getMe(), getMyMasterProfile()])
      .then(([userData, profileData]) => {
        setUser(userData)
        setMasterProfile(profileData)
      })
      .catch(() => navigate('/error', { replace: true }))
  }, [navigate])

  const fullName = user
    ? [user.firstName, user.lastName].filter(Boolean).join(' ')
    : '...'

  const username   = tgUser?.username ? `@${tgUser.username}` : null
  const telegramId = tgUser?.id ?? null
  const initials   = user ? getInitials(user.firstName, user.lastName) : '?'

  const hasPhoto = !!masterProfile?.photoUrl

  const handleAvatarClick = () => {
    if (!uploading) fileInputRef.current?.click()
  }

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    e.target.value = ''

    setUploading(true)
    setUploadError(null)
    try {
      const fileToUpload = await convertToJpegIfNeeded(file)
      await uploadMasterPhoto(fileToUpload)
      setMasterProfile(prev => prev ? { ...prev, photoUrl: 'uploaded' } : prev)
      setCacheBust(Date.now())
    } catch (e: unknown) {
      if (isRateLimitError(e)) { showRateLimitAlert(); return }
      if (axios.isAxiosError(e) && e.response && e.response.status < 500) {
        setUploadError(e.response.data?.error ?? 'Не удалось загрузить фото')
        return
      }
      navigate('/error', { replace: true })
    } finally {
      setUploading(false)
    }
  }

  return (
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <img src={logo} className={styles.logo} alt="FadeAfro" />
      </div>

      {/* Заголовок */}
      <h1 className={styles.title}>Настройки профиля</h1>

      {/* Карточка профиля */}
      <div className={styles.profileCard}>

        {/* Аватар с кнопкой замены */}
        <div
          className={`${styles.avatarWrap} ${uploading ? styles.avatarUploading : ''}`}
          onClick={handleAvatarClick}
          title="Сменить фото"
        >
          {hasPhoto
            ? <img
                src={getMasterPhotoUrl(masterProfile!.id, cacheBust)}
                alt={fullName}
                className={styles.avatarImg}
              />
            : <span className={styles.initials}>{initials}</span>
          }
          <div className={styles.avatarOverlay}>
            {uploading ? <SpinnerIcon /> : <CameraIcon />}
          </div>
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className={styles.fileInput}
          onChange={handleFileChange}
        />

        <div className={styles.info}>
          <span className={styles.name}>{fullName}</span>
          <div className={styles.subInfo}>
            {username   && <span className={styles.username}>{username}</span>}
            {telegramId && <span className={styles.userId}>ID: {telegramId}</span>}
          </div>
        </div>
      </div>

      {/* Ошибка загрузки фото */}
      {uploadError && (
        <div className={styles.uploadError}>{uploadError}</div>
      )}

      {/* Роли */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>РОЛИ</span>
        <div className={styles.rolesChips}>
          {roles.map(role => (
            <span key={role} className={`${styles.roleChip} ${styles[`roleChip_${role}`]}`}>
              {ROLE_LABELS[role]}
            </span>
          ))}
        </div>
      </section>

    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const CameraIcon = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z" />
    <circle cx="12" cy="13" r="4" />
  </svg>
)

const SpinnerIcon = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
    <path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83">
      <animateTransform attributeName="transform" type="rotate"
        from="0 12 12" to="360 12 12" dur="0.8s" repeatCount="indefinite" />
    </path>
  </svg>
)

export default MasterSettingsPage
