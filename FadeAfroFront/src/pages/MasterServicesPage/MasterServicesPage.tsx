import logo from '../../assets/logo.png'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { isRateLimitError, showRateLimitAlert } from '../../api/errors'
import {
  getMasterServices,
  deleteMyService,
  type MasterService,
} from '../../api/services'
import { getMyMasterProfile } from '../../api/masters'
import LoadingScreen from '../../components/LoadingScreen/LoadingScreen'
import useBackButton from '../../hooks/useBackButton'
import styles from './MasterServicesPage.module.css'

// ── Утилиты ────────────────────────────────────────────────────────────────

const hoursWord = (h: number) => {
  if (h === 1) return 'час'
  if (h >= 2 && h <= 4) return 'часа'
  return 'часов'
}

const formatDuration = (duration: string): string => {
  const parts = duration.split(':')
  const h = parseInt(parts[0], 10)
  const m = parseInt(parts[1], 10)
  if (h === 0) return `${m} мин`
  if (m === 0) return `${h} ${hoursWord(h)}`
  return `${h} ${hoursWord(h)} ${m} мин`
}

const formatPrice = (price: number): string =>
  price.toLocaleString('ru-RU') + ' ₽'

// ── Карточка услуги ────────────────────────────────────────────────────────

const ServiceCard = ({
  service,
  onClick,
}: {
  service: MasterService
  onClick: () => void
}) => (
  <div className={styles.card} onClick={onClick}>
    <div className={styles.cardTop}>
      <span className={styles.cardName}>{service.name}</span>
      <span className={styles.cardPrice}>{formatPrice(service.price)}</span>
    </div>
    {service.description && (
      <span className={styles.cardDesc}>{service.description}</span>
    )}
    <div className={styles.cardBottom}>
      <DurationIcon />
      <span className={styles.cardDuration}>{formatDuration(service.duration)}</span>
    </div>
  </div>
)

// ── Нижняя панель действий ────────────────────────────────────────────────

const ServiceActionPanel = ({
  service,
  onEdit,
  onDelete,
  onClose,
}: {
  service: MasterService
  onEdit: () => void
  onDelete: () => void
  onClose: () => void
}) => {
  const [confirmMode, setConfirmMode] = useState(false)
  const [pending,     setPending]     = useState(false)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const handleDelete = async () => {
    setPending(true)
    setDeleteError(null)
    try {
      await deleteMyService(service.id)
      onDelete()
    } catch (e: unknown) {
      if (isRateLimitError(e)) {
        showRateLimitAlert()
      } else {
        const msg = (e as { response?: { data?: { error?: string } } })?.response?.data?.error
        setDeleteError(msg ?? 'Не удалось удалить услугу')
      }
    } finally {
      setPending(false)
    }
  }

  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={styles.bottomPanel}>
        <div className={styles.panelHandle} />

        {confirmMode ? (
          <>
            <div className={styles.panelWarningIcon}>🗑️</div>
            <div className={styles.panelWarningTitle}>
              Удалить «{service.name}»?
            </div>
            <div className={styles.panelWarningList}>
              <div className={styles.panelWarningItem}>
                <span className={styles.panelWarningDot} />
                Услуга будет удалена из вашего профиля
              </div>
              <div className={styles.panelWarningItem}>
                <span className={styles.panelWarningDot} />
                Услуга не удалится, если на неё есть активные записи
              </div>
            </div>
            {deleteError && (
              <div className={styles.deleteError}>{deleteError}</div>
            )}
            <button
              className={`${styles.panelBtn} ${styles.panelBtnDelete}`}
              onClick={handleDelete}
              disabled={pending}
            >
              {pending ? 'Удаление...' : 'Удалить'}
            </button>
            <button className={styles.panelBtnCancel} onClick={() => { setConfirmMode(false); setDeleteError(null) }}>
              Назад
            </button>
          </>
        ) : (
          <>
            <div className={styles.panelService}>
              <span className={styles.panelServiceName}>{service.name}</span>
              <span className={styles.panelServiceMeta}>
                {formatPrice(service.price)} · {formatDuration(service.duration)}
              </span>
            </div>

            <button className={styles.panelBtn} onClick={onEdit}>
              Редактировать
            </button>
            <button
              className={`${styles.panelBtn} ${styles.panelBtnDelete}`}
              onClick={() => setConfirmMode(true)}
            >
              Удалить
            </button>
            <button className={styles.panelBtnCancel} onClick={onClose}>
              Отмена
            </button>
          </>
        )}
      </div>
    </>
  )
}

// ── Страница ───────────────────────────────────────────────────────────────

const MasterServicesPage = () => {
  useBackButton()
  const navigate = useNavigate()

  const [services,         setServices]         = useState<MasterService[]>([])
  const [masterProfileId,  setMasterProfileId]  = useState<string | null>(null)
  const [selectedService,  setSelectedService]  = useState<MasterService | null>(null)
  const [loading,          setLoading]          = useState(true)

  const loadServices = async (profileId: string) => {
    const data = await getMasterServices(profileId)
    setServices(data)
  }

  useEffect(() => {
    ;(async () => {
      try {
        const profile = await getMyMasterProfile()
        setMasterProfileId(profile.id)
        await loadServices(profile.id)
      } catch {
        navigate('/error', { replace: true })
      } finally {
        setLoading(false)
      }
    })()
  }, [navigate])

  const handleDelete = async () => {
    setSelectedService(null)
    if (masterProfileId) await loadServices(masterProfileId).catch(() => {})
  }

  if (loading) return <LoadingScreen />

  return (
    <>
      <div className={styles.page}>

        {/* Шапка */}
        <div className={styles.header}>
          <div className={styles.logoWrap}>
            <img src={logo} className={styles.logo} alt="FadeAfro" />
          </div>
          <h1 className={styles.title}>Услуги</h1>
        </div>

        {/* Список */}
        <div className={styles.list}>
          {services.length === 0 ? (
            <div className={styles.emptyCard}>
              <div className={styles.emptyIcon}>✂️</div>
              <div className={styles.emptyText}>Услуги ещё не добавлены</div>
              <div className={styles.emptyHint}>Нажмите кнопку ниже, чтобы добавить первую услугу</div>
            </div>
          ) : (
            services.map(service => (
              <ServiceCard
                key={service.id}
                service={service}
                onClick={() => setSelectedService(service)}
              />
            ))
          )}
        </div>

      </div>

      {/* Кнопка добавления */}
      <div className={styles.addPanel}>
        <button
          className={styles.addBtn}
          onClick={() => navigate('/master/services/add')}
        >
          <PlusIcon />
          Добавить услугу
        </button>
      </div>

      {/* Панель действий */}
      {selectedService && (
        <ServiceActionPanel
          service={selectedService}
          onEdit={() => {
            navigate(`/master/services/${selectedService.id}/edit`, {
              state: { service: selectedService },
            })
            setSelectedService(null)
          }}
          onDelete={handleDelete}
          onClose={() => setSelectedService(null)}
        />
      )}
    </>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const PlusIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="5" x2="12" y2="19" />
    <line x1="5" y1="12" x2="19" y2="12" />
  </svg>
)

const DurationIcon = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <polyline points="12 6 12 12 16 14" />
  </svg>
)

export default MasterServicesPage
