import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { SDKProvider } from '@tma.js/sdk-react'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SDKProvider acceptCustomStyles debug>
      <App />
    </SDKProvider>
  </StrictMode>,
)
