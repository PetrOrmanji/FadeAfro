import { useNavigate } from 'react-router-dom'
import styles from './ErrorPage.module.css'

const ErrorPage = () => {
  const navigate = useNavigate()

  return (
    <div className={styles.page}>
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      <div className={styles.content}>
        <div className={styles.iconWrap}>
          <div className={styles.icon}>
            <AlertIcon />
          </div>
        </div>

        <div className={styles.titleBlock}>
          <h1 className={styles.title}>Что-то пошло не так</h1>
          <p className={styles.subtitle}>Попробуйте обновить страницу или зайдите позже</p>
        </div>
      </div>

      <div className={styles.bottomPanel}>
        <button className={styles.retryBtn} onClick={() => navigate('/')}>
          Обновить
        </button>
      </div>
    </div>
  )
}

const AlertIcon = () => (
  <svg width="40" height="40" viewBox="0 0 24 24" fill="none"
    stroke="#ffffff" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="8" x2="12" y2="12" />
    <line x1="12" y1="16" x2="12.01" y2="16" />
  </svg>
)

export default ErrorPage
