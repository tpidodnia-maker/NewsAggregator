import { useState, FormEvent } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import { authApi } from '../../api/api'

export const ResetPassword = () => {
  const [searchParams]        = useSearchParams()
  const [password, setPassword]     = useState('')
  const [confirm, setConfirm]       = useState('')
  const [error, setError]           = useState('')
  const [loading, setLoading]       = useState(false)
  const [success, setSuccess]       = useState(false)
  const navigate                    = useNavigate()
  const token                       = searchParams.get('token') ?? ''

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    if (password !== confirm) {
      setError('Пароли не совпадают')
      return
    }
    if (password.length < 6) {
      setError('Минимум 6 символов')
      return
    }

    setLoading(true)
    try {
      await authApi.resetPassword(token, password)
      setSuccess(true)
      setTimeout(() => navigate('/login'), 3000)
    } catch {
      setError('Ссылка недействительна или истекла')
    } finally {
      setLoading(false)
    }
  }

  if (success) return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="success-icon">✅</div>
        <h2>Пароль изменён!</h2>
        <p>Перенаправляем на страницу входа...</p>
      </div>
    </div>
  )

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2>Новый пароль</h2>
        {error && <div className="error-message">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Новый пароль</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              minLength={6}
              placeholder="Минимум 6 символов"
            />
          </div>
          <div className="form-group">
            <label>Повторите пароль</label>
            <input
              type="password"
              value={confirm}
              onChange={e => setConfirm(e.target.value)}
              required
              placeholder="Повторите пароль"
            />
          </div>
          <button type="submit" disabled={loading} className="btn-primary">
            {loading ? 'Сохранение...' : 'Сохранить пароль'}
          </button>
        </form>
        <p className="auth-link">
          <Link to="/login">← Вернуться ко входу</Link>
        </p>
      </div>
    </div>
  )
}