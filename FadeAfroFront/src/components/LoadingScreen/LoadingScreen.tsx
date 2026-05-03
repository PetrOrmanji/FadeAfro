import styles from './LoadingScreen.module.css'

const LoadingScreen = () => (
  <div className={styles.page}>
    <div className={styles.logo}>✂</div>
    <div className={styles.dotsWrap}>
      <span className={styles.dot} />
      <span className={styles.dot} />
      <span className={styles.dot} />
    </div>
  </div>
)

export default LoadingScreen
