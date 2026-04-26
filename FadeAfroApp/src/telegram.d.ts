interface TelegramWebApp {
  initData: string
  initDataUnsafe: {
    user?: {
      id: number
      first_name: string
      last_name?: string
      username?: string
    }
  }
  colorScheme: 'light' | 'dark'
  themeParams: Record<string, string>
  expand: () => void
  ready: () => void
  onEvent: (eventType: string, callback: () => void) => void
  offEvent: (eventType: string, callback: () => void) => void
  showAlert: (message: string, callback?: () => void) => void
  showConfirm: (message: string, callback: (confirmed: boolean) => void) => void
  MainButton: {
    show: () => void
    hide: () => void
    setText: (text: string) => void
    onClick: (callback: () => void) => void
  }
  BackButton: {
    show: () => void
    hide: () => void
    onClick: (callback: () => void) => void
  }
  HapticFeedback: {
    impactOccurred: (style: string) => void
    notificationOccurred: (type: string) => void
  }
}

interface Window {
  Telegram: {
    WebApp: TelegramWebApp
  }
}
