import { Navigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'

interface Props {
  children: React.ReactNode
  adminOnly?: boolean
}

export const PrivateRoute = ({ children, adminOnly = false }: Props) => {
  const { user, isLoading } = useAuth()

  if (isLoading) return <div className="loading">Загрузка...</div>
  if (!user) return <Navigate to="/login" replace />
  if (adminOnly && user.role !== 'Admin') return <Navigate to="/" replace />

  return <>{children}</>
}