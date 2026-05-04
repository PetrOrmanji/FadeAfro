export class RateLimitError extends Error {
  constructor() {
    super('Rate limit exceeded')
    this.name = 'RateLimitError'
  }
}

export const isRateLimitError = (e: unknown): e is RateLimitError =>
  e instanceof RateLimitError

export const showRateLimitAlert = () => {
  const msg = 'Слишком много запросов. Попробуйте через минуту.'
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const tg = (window as any).Telegram?.WebApp
  if (typeof tg?.showAlert === 'function') {
    tg.showAlert(msg)
  } else {
    alert(msg)
  }
}
