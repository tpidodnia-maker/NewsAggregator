import { Link, useNavigate } from 'react-router-dom'
import { Newspaper, User } from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'
import { ThemeToggle } from '../theme/ThemeToggle'

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
        <Newspaper size={20} strokeWidth={2} />
        NewsAggregator
      </Link>

      <div className="navbar__links">
        <ThemeToggle />
        {user ? (
          <>
            <span className="navbar__user">
              <User size={16} strokeWidth={2} />
              {user.username}
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