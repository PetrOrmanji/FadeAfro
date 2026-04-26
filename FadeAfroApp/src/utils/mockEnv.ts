/**
 * Мок Telegram окружения для разработки в браузере.
 * В продакшне (внутри Telegram) этот файл не нужен — SDK получает данные напрямую.
 */
export function mockTelegramEnv() {
  if (typeof window === 'undefined') return

  const isDev = import.meta.env.DEV
  const isTelegram = 'Telegram' in window

  if (isDev && !isTelegram) {
    // Минимальный мок initData для разработки
    const initData = [
      'user=%7B%22id%22%3A123456789%2C%22first_name%22%3A%22Ivan%22%2C%22last_name%22%3A%22Petrov%22%2C%22username%22%3A%22ivanp%22%7D',
      'auth_date=9999999999',
      'hash=mockhash',
    ].join('&')

    Object.defineProperty(window, 'Telegram', {
      value: {
        WebApp: {
          initData,
          initDataUnsafe: {
            user: {
              id: 123456789,
              first_name: 'Ivan',
              last_name: 'Petrov',
              username: 'ivanp',
            },
          },
          colorScheme: 'light',
          themeParams: {},
          expand: () => {},
          ready: () => {},
          onEvent: () => {},
          offEvent: () => {},
          MainButton: { show: () => {}, hide: () => {}, setText: () => {}, onClick: () => {} },
          BackButton: { show: () => {}, hide: () => {}, onClick: () => {} },
          HapticFeedback: {
            impactOccurred: () => {},
            notificationOccurred: () => {},
          },
          showAlert: (message: string, callback?: () => void) => {
            window.alert(message)
            callback?.()
          },
          showConfirm: (message: string, callback: (confirmed: boolean) => void) => {
            callback(window.confirm(message))
          },
        },
      },
      writable: true,
    })
  }
}
