import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getMasterServices, type MasterService } from '../../api/services'
import { getMasterPhotoUrl, type MasterProfile } from '../../api/masters'
import { formatDuration, durationToMinutes, minutesToFormatted } from '../../utils/duration'
import styles from './SelectServicePage.module.css'

// ── Карточка мастера ───────────────────────────────────────────────────────

const MasterPreviewCard = ({ master }: { master: MasterProfile }) => {
  const fullName = master.lastName
    ? `${master.firstName} ${master.lastName}`
    : master.firstName

  return (
    <div className={styles.masterCard}>
      <div className={styles.masterAvatar}>
        {master.photoUrl
          ? <img src={getMasterPhotoUrl(master.id)} alt={fullName} />
          : <span>{master.firstName.charAt(0)}</span>
        }
      </div>
      <div className={styles.masterInfo}>
        <span className={styles.masterName}>{fullName}</span>
        <span className={styles.masterSubtitle}>Ваш мастер</span>
      </div>
    </div>
  )
}

// ── Строка услуги ──────────────────────────────────────────────────────────

const ServiceItem = ({
  service,
  selected,
  onToggle,
}: {
  service: MasterService
  selected: boolean
  onToggle: () => void
}) => (
  <div
    className={`${styles.serviceItem} ${selected ? styles.serviceItemSelected : ''}`}
    onClick={onToggle}
  >
    <div className={styles.serviceText}>
      <span className={styles.serviceName}>{service.name}</span>
      <span className={styles.serviceMeta}>
        {formatDuration(service.duration)} · {service.price} ₽
      </span>
    </div>
    <div className={`${styles.radio} ${selected ? styles.radioSelected : ''}`}>
      {selected && <CheckIcon />}
    </div>
  </div>
)

// ── Фиксированная панель снизу ─────────────────────────────────────────────

const BottomPanel = ({
  selectedServices,
  onNext,
}: {
  selectedServices: MasterService[]
  onNext: () => void
}) => {
  const count = selectedServices.length
  const totalPrice = selectedServices.reduce((sum, s) => sum + s.price, 0)
  const totalMinutes = selectedServices.reduce((sum, s) => sum + durationToMinutes(s.duration), 0)

  return (
    <div className={styles.bottomPanel}>
      <div className={styles.summary}>
        <span className={styles.summaryLeft}>
          {count} {count === 1 ? 'Выбрано' : 'Выбрано'} · {minutesToFormatted(totalMinutes)}
        </span>
        <span className={styles.summaryPrice}>{totalPrice} ₽</span>
      </div>
      <button className={styles.nextBtn} onClick={onNext}>
        Далее
      </button>
    </div>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

interface LocationState {
  master: MasterProfile
}

const SelectServicePage = () => {
  const { masterProfileId } = useParams<{ masterProfileId: string }>()
  const master = (history.state?.usr as LocationState)?.master

  const [services, setServices] = useState<MasterService[]>([])
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!masterProfileId) return
    getMasterServices(masterProfileId)
      .then(setServices)
      .finally(() => setLoading(false))
  }, [masterProfileId])

  const toggleService = (id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })
  }

  const selectedServices = services.filter(s => selectedIds.has(s.id))

  const handleNext = () => {
    // TODO: переход к выбору времени
  }

  if (loading) return <div className={styles.loading}>Загрузка...</div>

  return (
    <div className={`${styles.page} ${selectedIds.size > 0 ? styles.pageWithPanel : ''}`}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <div className={styles.logoPlaceholder}>✂</div>
      </div>

      {/* Заголовок */}
      <h1 className={styles.title}>Выберите услугу</h1>

      {/* Карточка мастера */}
      {master && (
        <MasterPreviewCard master={master} />
      )}

      {/* Подзаголовок */}
      <p className={styles.subtitle}>Выберите одну или несколько услуг:</p>

      {/* Список услуг */}
      <div className={styles.servicesList}>
        {services.map(service => (
          <ServiceItem
            key={service.id}
            service={service}
            selected={selectedIds.has(service.id)}
            onToggle={() => toggleService(service.id)}
          />
        ))}
      </div>

      {/* Нижняя панель — появляется при выборе услуг */}
      {selectedIds.size > 0 && (
        <BottomPanel selectedServices={selectedServices} onNext={handleNext} />
      )}
    </div>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const CheckIcon = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12" />
  </svg>
)

export default SelectServicePage
