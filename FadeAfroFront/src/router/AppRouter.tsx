import { HashRouter, Route, Routes } from 'react-router-dom'
import HomePage from '../pages/HomePage'

const AppRouter = () => {
  return (
    <HashRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
      </Routes>
    </HashRouter>
  )
}

export default AppRouter
