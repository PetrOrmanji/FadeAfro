import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAllMasters, type MasterProfile } from '../../api/masters'
import { getMe, type UserResponse } from '../../api/user'
import MasterCard from '../../components/MasterCard/MasterCard'
import UserInfoCard from '../../components/UserInfoCard/UserInfoCard'
import styles from './ClientPage.module.css'

// Пока нет логотипа — показываем плейсхолдер.
// Когда будет файл: заменить на <img src="/logo.png" ... />
const Logo = () => (
  <div className={styles.logoPlaceholder}>
    ✂
  </div>
)

const ClientPage = () => {
  const [user, setUser] = useState<UserResponse | null>(null)
  const [masters, setMasters] = useState<MasterProfile[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([getMe(), getAllMasters()])
      .then(([userData, mastersData]) => {
        setUser(userData)
        setMasters(mastersData)
      })
      .finally(() => setLoading(false))
  }, [])

  const handleSelectMaster = (master: MasterProfile) => {
    // TODO: переход к выбору услуги/времени
    console.log('selected master:', master)
  }

  const navigate = useNavigate()

  const handleSettingsClick = () => {
    navigate('/client/settings')
  }

  if (loading) {
    return <div className={styles.loading}>Загрузка...</div>
  }

  return (
    <div className={styles.page}>
      {/* Логотип */}
      <div className={styles.logoWrap}>
        <Logo />
      </div>

      {/* Карточка пользователя */}
      {user && (
        <UserInfoCard user={user} onSettingsClick={handleSettingsClick} />
      )}

      {/* Список мастеров */}
      <section className={styles.mastersSection}>
        <h2 className={styles.sectionTitle}>Выберите мастера:</h2>
        <div className={styles.mastersGrid}>
          {masters.map(master => (
            <MasterCard
              key={master.id}
              master={master}
              onSelect={handleSelectMaster}
            />
          ))}
        </div>
      </section>
    </div>
  )
}

export default ClientPage
