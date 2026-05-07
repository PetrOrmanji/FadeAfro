import { useEffect, useState } from 'react'
import { AuthProvider } from './context/AuthContext'
import AppRouter from './router/AppRouter'

function App() {
  const isLandscapeNow = () => window.innerWidth > window.innerHeight
  const [isLandscape, setIsLandscape] = useState(isLandscapeNow)

  useEffect(() => {
    const handler = () => setIsLandscape(isLandscapeNow())
    window.addEventListener('resize', handler)
    window.addEventListener('orientationchange', handler)
    return () => {
      window.removeEventListener('resize', handler)
      window.removeEventListener('orientationchange', handler)
    }
  }, [])

  return (
    <AuthProvider>
      {isLandscape ? <RotateOverlay /> : <AppRouter />}
    </AuthProvider>
  )
}

const RotateOverlay = () => (
  <div data-rotate-overlay style={{
    position: 'fixed', inset: 0,
    background: 'var(--tg-secondary-bg)',
    display: 'flex', flexDirection: 'column',
    alignItems: 'center', justifyContent: 'center',
    gap: '16px', zIndex: 9999,
  }}>
    <svg width="52" height="52" viewBox="0 0 24 24" fill="none"
      stroke="var(--tg-hint)" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <rect x="4" y="2" width="16" height="20" rx="2" />
      <line x1="12" y1="18" x2="12.01" y2="18" strokeWidth="2" />
    </svg>
    <p style={{
      fontSize: '15px', fontWeight: 500,
      color: 'var(--tg-hint)', textAlign: 'center',
      padding: '0 32px',
    }}>
      Переверните телефон вертикально
    </p>
  </div>
)

export default App
