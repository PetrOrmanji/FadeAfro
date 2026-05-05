import logo from '../../assets/logo.png'
import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { addMyService, updateMyService, type MasterService } from '../../api/services'
import { isRateLimitError, showRateLimitAlert } from '../../api/errors'
import useBackButton from '../../hooks/useBackButton'
import styles from './MasterServiceFormPage.module.css'

// ── Пресеты длительности ───────────────────────────────────────────────────

const DURATION_PRESETS: { label: string; value: string }[] = [
  { label: '15 мин',     value: '00:15:00' },
  { label: '30 мин',     value: '00:30:00' },
  { label: '45 мин',     value: '00:45:00' },
  { label: '1 час',      value: '01:00:00' },
  { label: '1 ч 15 мин', value: '01:15:00' },
  { label: '1 ч 30 мин', value: '01:30:00' },
  { label: '1 ч 45 мин', value: '01:45:00' },
  { label: '2 часа',     value: '02:00:00' },
]

const toHHMMSS = (h: number, m: number) =>
  `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`

// ── Страница ───────────────────────────────────────────────────────────────

const MasterServiceFormPage = () => {
  useBackButton()
  const navigate = useNavigate()
  const location = useLocation()

  const editService = (location.state as { service?: MasterService } | null)?.service ?? null
  const isEdit = !!editService

  // Определяем начальный режим: пресет или своё
  const initIsCustom = editService
    ? !DURATION_PRESETS.some(p => p.value === editService.duration)
    : false
  const initH = editService ? parseInt(editService.duration.split(':')[0], 10) : 0
  const initM = editService ? parseInt(editService.duration.split(':')[1], 10) : 30

  const [name,        setName]        = useState(editService?.name        ?? '')
  const [description, setDescription] = useState(editService?.description ?? '')
  const [price,       setPrice]       = useState(editService ? String(editService.price) : '')
  const [duration,    setDuration]    = useState(editService?.duration ?? '00:30:00')
  const [customMode,  setCustomMode]  = useState(initIsCustom)
  const [customH,     setCustomH]     = useState(initIsCustom ? initH : 0)
  const [customM,     setCustomM]     = useState(initIsCustom ? initM : 30)
  const [saving,      setSaving]      = useState(false)
  const [errors,      setErrors]      = useState<Record<string, string>>({})

  const effectiveDuration = customMode ? toHHMMSS(customH, customM) : duration

  const handlePreset = (value: string) => {
    setCustomMode(false)
    setDuration(value)
  }

  const handleCustomH = (val: string) => {
    const n = Math.min(23, Math.max(0, parseInt(val) || 0))
    setCustomH(n)
  }

  const handleCustomM = (val: string) => {
    const n = Math.min(59, Math.max(0, parseInt(val) || 0))
    setCustomM(n)
  }

  const validate = () => {
    const e: Record<string, string> = {}
    if (!name.trim())                 e.name  = 'Введите название'
    if (!price || Number(price) <= 0) e.price = 'Введите корректную цену'
    if (customMode && customH === 0 && customM === 0) e.duration = 'Укажите длительность'
    setErrors(e)
    return Object.keys(e).length === 0
  }

  const handleSave = async () => {
    if (!validate() || saving) return
    setSaving(true)
    try {
      const payload = {
        name: name.trim(),
        description: description.trim() || undefined,
        price: Number(price),
        duration: effectiveDuration,
      }
      if (isEdit) {
        await updateMyService(editService.id, payload)
      } else {
        await addMyService(payload)
      }
      navigate('/master/services', { replace: true })
    } catch (e: unknown) {
      if (isRateLimitError(e)) showRateLimitAlert()
      else navigate('/error', { replace: true })
    } finally {
      setSaving(false)
    }
  }

  const canSave = name.trim().length > 0 && Number(price) > 0
    && !(customMode && customH === 0 && customM === 0)

  return (
    <>
    <div className={styles.page}>

      {/* Логотип */}
      <div className={styles.logoWrap}>
        <img src={logo} className={styles.logo} alt="FadeAfro" />
      </div>

      {/* Заголовок */}
      <h1 className={styles.title}>
        {isEdit ? 'Редактировать услугу' : 'Новая услуга'}
      </h1>

      {/* Название */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>НАЗВАНИЕ</span>
        <div className={`${styles.fieldCard} ${errors.name ? styles.fieldCardError : ''}`}>
          <input
            className={styles.fieldInput}
            placeholder="Стрижка машинкой"
            value={name}
            onChange={e => { setName(e.target.value); setErrors(p => ({ ...p, name: '' })) }}
            maxLength={100}
          />
        </div>
        {errors.name && <span className={styles.errorText}>{errors.name}</span>}
      </section>

      {/* Описание */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>
          ОПИСАНИЕ <span className={styles.optional}>(необязательно)</span>
        </span>
        <div className={styles.fieldCard}>
          <textarea
            className={styles.fieldTextarea}
            placeholder="Коротко об услуге..."
            value={description}
            onChange={e => setDescription(e.target.value)}
            rows={3}
            maxLength={300}
          />
        </div>
      </section>

      {/* Цена */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>ЦЕНА</span>
        <div className={`${styles.fieldCard} ${styles.fieldCardRow} ${errors.price ? styles.fieldCardError : ''}`}>
          <input
            className={styles.fieldInput}
            type="number"
            inputMode="numeric"
            placeholder="0"
            value={price}
            onChange={e => { setPrice(e.target.value); setErrors(p => ({ ...p, price: '' })) }}
            min={1}
          />
          <span className={styles.fieldUnit}>₽</span>
        </div>
        {errors.price && <span className={styles.errorText}>{errors.price}</span>}
      </section>

      {/* Длительность */}
      <section className={styles.section}>
        <span className={styles.sectionLabel}>ДЛИТЕЛЬНОСТЬ</span>

        <div className={styles.durationGrid}>
          {DURATION_PRESETS.map(opt => (
            <button
              key={opt.value}
              className={`${styles.durationChip} ${!customMode && duration === opt.value ? styles.durationChipActive : ''}`}
              onClick={() => handlePreset(opt.value)}
            >
              {opt.label}
            </button>
          ))}

          {/* Кнопка своего времени */}
          <button
            className={`${styles.durationChip} ${styles.durationChipCustom} ${customMode ? styles.durationChipActive : ''}`}
            onClick={() => setCustomMode(true)}
          >
            <PencilIcon />
            Своё
          </button>
        </div>

        {/* Кастомный ввод */}
        {customMode && (
          <div className={styles.customRow}>
            <div className={styles.customField}>
              <input
                className={styles.customInput}
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                value={customH === 0 ? '' : String(customH)}
                placeholder="0"
                onChange={e => handleCustomH(e.target.value.replace(/\D/g, ''))}
              />
              <span className={styles.customUnit}>ч</span>
            </div>
            <span className={styles.customSep}>:</span>
            <div className={styles.customField}>
              <input
                className={styles.customInput}
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                value={customM === 0 ? '' : String(customM)}
                placeholder="0"
                onChange={e => handleCustomM(e.target.value.replace(/\D/g, ''))}
              />
              <span className={styles.customUnit}>мин</span>
            </div>
          </div>
        )}

        {errors.duration && <span className={styles.errorText}>{errors.duration}</span>}
      </section>

    </div>

    {/* Панель с кнопкой сохранения */}
    <div className={styles.bottomPanel}>
      <button
        className={styles.saveBtn}
        onClick={handleSave}
        disabled={!canSave || saving}
      >
        {saving ? 'Сохранение...' : isEdit ? 'Сохранить изменения' : 'Добавить услугу'}
      </button>
    </div>
    </>
  )
}

// ── Иконки ─────────────────────────────────────────────────────────────────

const PencilIcon = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
)

export default MasterServiceFormPage
