import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { backButton, init } from '@tma.js/sdk-react'

const useBackButton = (to?: string) => {
  const navigate = useNavigate()

  useEffect(() => {
    try {
      init()
    } catch {
      // already initialized
    }

    try {
      if (!backButton.isMounted()) backButton.mount()
      backButton.show()
      const unsub = backButton.onClick(() => to ? navigate(to, { replace: true }) : navigate(-1))

      return () => {
        unsub()
        backButton.hide()
      }
    } catch {
      // not in Telegram environment — silently ignore
    }
  }, [navigate])
}

export default useBackButton
