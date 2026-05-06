// "HH:MM:SS" → минуты
export const durationToMinutes = (duration: string): number => {
  const [h, m] = duration.split(':').map(Number)
  return h * 60 + m
}

const pluralHours = (h: number): string => {
  const mod10 = h % 10
  const mod100 = h % 100
  if (mod100 >= 11 && mod100 <= 19) return `${h} часов`
  if (mod10 === 1) return `${h} час`
  if (mod10 >= 2 && mod10 <= 4) return `${h} часа`
  return `${h} часов`
}

// минуты → "1 час 30 мин", "30 мин", "2 часа"
export const minutesToFormatted = (minutes: number): string => {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h > 0 && m > 0) return `${pluralHours(h)} ${m} мин`
  if (h > 0) return pluralHours(h)
  return `${m} мин`
}

// "HH:MM:SS" → "1 ч 30 мин"
export const formatDuration = (duration: string): string =>
  minutesToFormatted(durationToMinutes(duration))
