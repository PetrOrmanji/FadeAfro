import type { MasterProfile } from '../../api/masters'
import { getMasterPhotoUrl } from '../../api/masters'
import styles from './MasterCard.module.css'

interface Props {
  master: MasterProfile
  onSelect: (master: MasterProfile) => void
}

const MasterCard = ({ master, onSelect }: Props) => {
  const fullName = master.lastName
    ? `${master.firstName} ${master.lastName}`
    : master.firstName

  return (
    <div className={styles.card}>
      <div className={styles.photo}>
        {master.photoUrl
          ? <img src={getMasterPhotoUrl(master.id)} alt={fullName} />
          : <div className={styles.noPhoto}>{master.firstName.charAt(0)}</div>
        }
      </div>
      <div className={styles.footer}>
        <span className={styles.name}>{fullName}</span>
        <button className={styles.selectBtn} onClick={() => onSelect(master)}>
          Выбрать
        </button>
      </div>
    </div>
  )
}

export default MasterCard
