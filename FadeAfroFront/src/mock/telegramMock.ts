/**
 * Мок Telegram окружения для разработки в браузере.
 * В продакшне (внутри Telegram) этот файл не нужен — SDK получает данные напрямую.
 *
 * Переключение пользователя: меняй MOCK_USER_ID на нужный id ниже.
 *
 * 777777777 — Petr Masterov   (Master)
 * 100000001 — Oleg Ownerow    (Owner + Master)
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
  role: 'Client' | 'Master' | 'Owner' | 'OwnerMaster'
}> = {
  777777777: { id: 777777777, first_name: 'Petr',  last_name: 'Masterov', username: 'petr_masterov',  role: 'Master'      },
  100000001: { id: 100000001, first_name: 'Oleg',  last_name: 'Ownerow',  username: 'oleg_ownerow',  role: 'OwnerMaster' },
  100000002: { id: 100000002, first_name: 'Artem', last_name: 'Masterov', username: 'artem_masterov', role: 'Master'      },
  100000003: { id: 100000003, first_name: 'Vasya', last_name: 'Clientov', username: 'vasya_clientov', role: 'Client'      },
}

export const getMockUser = () => MOCK_USERS[MOCK_USER_ID]

// ─────────────────────────────────────────────────────────────────────────────

export const setupTelegramMock = async () => {
  const { mockTelegramEnv } = await import('@tma.js/sdk-react')
  const user = MOCK_USERS[MOCK_USER_ID]

  // signature — обязательное поле схемы initData в tma.js v3
  const rawInitData = `user=${JSON.stringify(user)}&auth_date=9999999999&hash=mockhash&signature=mocksignature`

  mockTelegramEnv({
    launchParams: {
      tgWebAppPlatform: 'web',
      tgWebAppVersion: '8.0',
      tgWebAppThemeParams: {},
      tgWebAppData: rawInitData,
    },
  })
}
