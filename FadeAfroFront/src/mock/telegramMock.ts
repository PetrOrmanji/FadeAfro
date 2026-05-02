/**
 * Мок Telegram окружения для разработки в браузере.
 * В продакшне (внутри Telegram) этот файл не нужен — SDK получает данные напрямую.
 *
 * Переключение пользователя: меняй MOCK_USER_ID на нужный id ниже.
 *
 * 777777777 — Petr Masterov   (Master)
 * 100000001 — Oleg Ownerow    (Owner)
 * 100000002 — Artem Masterov  (Master)
 * 100000003 — Vasya Clientov  (Client)
 */

export const MOCK_USER_ID = 100000003

// ─────────────────────────────────────────────────────────────────────────────

const MOCK_USERS: Record<number, {
  id: number
  first_name: string
  last_name: string
  username: string
  role: 'Client' | 'Master' | 'Owner'
}> = {
  777777777: { id: 777777777, first_name: 'Petr',  last_name: 'Masterov', username: 'petr_masterov',  role: 'Master' },
  100000001: { id: 100000001, first_name: 'Oleg',  last_name: 'Ownerow',  username: 'oleg_ownerow',  role: 'Owner'  },
  100000002: { id: 100000002, first_name: 'Artem', last_name: 'Masterov', username: 'artem_masterov', role: 'Master' },
  100000003: { id: 100000003, first_name: 'Vasya', last_name: 'Clientov', username: 'vasya_clientov', role: 'Client' },
}

export const getMockUser = () => MOCK_USERS[MOCK_USER_ID]

// Инжектируем window.Telegram.WebApp чтобы SDK не падал при инициализации
export const setupTelegramMock = () => {
  const user = MOCK_USERS[MOCK_USER_ID]
  const userJson = JSON.stringify(user)
  const initData = `user=${encodeURIComponent(userJson)}&auth_date=9999999999&hash=mockhash`

  // @ts-expect-error — мок для dev окружения
  window.Telegram = {
    WebApp: {
      initData,
      initDataUnsafe: {
        user,
        auth_date: 9999999999,
        hash: 'mockhash',
      },
      version: '6.0',
      platform: 'web',
      colorScheme: 'light',
      themeParams: {},
      isExpanded: true,
      viewportHeight: window.innerHeight,
      viewportStableHeight: window.innerHeight,
      headerColor: '#ffffff',
      backgroundColor: '#ffffff',
      isClosingConfirmationEnabled: false,
      ready: () => {},
      expand: () => {},
      close: () => {},
      MainButton: { isVisible: false },
      BackButton: { isVisible: false },
    },
  }
}
