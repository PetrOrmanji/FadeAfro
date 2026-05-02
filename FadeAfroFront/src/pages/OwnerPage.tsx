import { hasRole, useAuth } from '../context/AuthContext'

const OwnerPage = () => {
  const { roles } = useAuth()
  const isMaster = hasRole(roles, 'Master')

  return (
    <div>
      <div>Owner Page</div>
      {isMaster && <div>Master tab (Owner is also Master)</div>}
    </div>
  )
}

export default OwnerPage
