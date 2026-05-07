import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

if (import.meta.env.DEV) {
  const { setupTelegramMock } = await import('./mock/telegramMock')
  setupTelegramMock()
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const tg = (window as any).Telegram?.WebApp
tg?.lockOrientation?.('portrait')

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
