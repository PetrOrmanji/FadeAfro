// "HH:MM:SS" → минуты
export const durationToMinutes = (duration: string): number => {
  const [h, m] = duration.split(':').map(Number)
  return h * 60 + m
}

// минуты → "1 ч 30 мин", "30 мин", "1 ч"
export const minutesToFormatted = (minutes: number): string => {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h > 0 && m > 0) return `${h} ч ${m} мин`
  if (h > 0) return `${h} ч`
  return `${m} мин`
}

// "HH:MM:SS" → "1 ч 30 мин"
export const formatDuration = (duration: string): string =>
  minutesToFormatted(durationToMinutes(duration))
