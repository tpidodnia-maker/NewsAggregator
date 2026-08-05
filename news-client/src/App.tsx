
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { ThemeProvider } from './contexts/ThemeContext'
import { NewsList }       from './components/news/NewsList'
import { NewsDetail }     from './components/news/NewsDetail'
import { Login }          from './components/auth/Login'
import { Register }       from './components/auth/Register'
import { ForgotPassword } from './components/auth/ForgotPassword'
import { ResetPassword }  from './components/auth/ResetPassword'
import { PrivateRoute }   from './components/shared/PrivateRoute'
import { Navbar }         from './components/layout/Navbar'
import './App.css'
import './styles/theme.css'
import './styles/news-card.css'

const App = () => (
  <ThemeProvider>
    <AuthProvider>
      <BrowserRouter>
        <Navbar />
        <div className="app-layout">
          <Routes>
            <Route path="/" element={
              <PrivateRoute><NewsList /></PrivateRoute>
            } />
            <Route path="/news/:id" element={
              <PrivateRoute><NewsDetail /></PrivateRoute>
            } />
            <Route path="/login"           element={<Login />} />
            <Route path="/register"        element={<Register />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password"  element={<ResetPassword />} />
            <Route path="*"                element={<Navigate to="/" />} />
          </Routes>
        </div>
      </BrowserRouter>
    </AuthProvider>
  </ThemeProvider>
)

export default App
