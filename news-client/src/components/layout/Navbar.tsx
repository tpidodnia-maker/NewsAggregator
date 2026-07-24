import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'

export const Navbar = () => {
  const { user, logout } = useAuth()
  const navigate         = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar">
      <Link to="/" className="navbar__logo">
        📰 NewsAggregator
      </Link>

      <div className="navbar__links">
        {user ? (
          <>
            <span className="navbar__user">
              👤 {user.username}
              {user.role === 'Admin' && (
                <span className="badge-admin">Admin</span>
              )}
            </span>
            <button onClick={handleLogout} className="btn-outline">
              Выйти
            </button>
          </>
        ) : (
          <>
            <Link to="/login">Войти</Link>
            <Link to="/register" className="btn-primary">
              Регистрация
            </Link>
          </>
        )}
      </div>
    </nav>
  )
}