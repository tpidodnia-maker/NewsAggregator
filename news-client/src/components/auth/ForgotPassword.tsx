import { useState, FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { authApi } from '../../api/api'

export const ForgotPassword = () => {
  const [email, setEmail]     = useState('')
  const [sent, setSent]       = useState(false)
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await authApi.forgotPassword(email)
      setSent(true)
    } finally {
      setLoading(false)
    }
  }

  if (sent) return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="success-icon">✉️</div>
        <h2>Письмо отправлено</h2>
        <p>Проверьте почту <strong>{email}</strong> и перейдите по ссылке для сброса пароля.</p>
        <Link to="/login" className="btn-primary" style={{ marginTop: '1rem', display: 'block', textAlign: 'center' }}>
          Вернуться ко входу
        </Link>
      </div>
    </div>
  )

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2>Забыли пароль?</h2>
        <p className="auth-subtitle">Введите email — отправим ссылку для сброса</p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              placeholder="your@email.com"
            />
          </div>
          <button type="submit" disabled={loading} className="btn-primary">
            {loading ? 'Отправка...' : 'Отправить ссылку'}
          </button>
        </form>
        <p className="auth-link"><Link to="/login">← Вернуться ко входу</Link></p>
      </div>
    </div>
  )
}