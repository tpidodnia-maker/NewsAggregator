import { useState, useEffect } from 'react'

export const CalendarWidget = () => {
  const [now, setNow]           = useState(new Date())
  const [timezone, setTimezone] = useState('')

  useEffect(() => {
    // Определяем часовой пояс пользователя
    const tz = Intl.DateTimeFormat().resolvedOptions().timeZone
    setTimezone(tz)

    // Обновляем время каждую секунду
    const timer = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(timer)
  }, [])

  const options: Intl.DateTimeFormatOptions = {
    timeZone: timezone,
    weekday: 'long', year: 'numeric',
    month: 'long', day: 'numeric'
  }

  const timeOptions: Intl.DateTimeFormatOptions = {
    timeZone: timezone,
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  }

  return (
    <div className="calendar-widget">
      <h3 className="widget-title">📅 Дата и время</h3>
      <div className="calendar-date">
        {now.toLocaleDateString('ru-RU', options)}
      </div>
      <div className="calendar-time">
        {now.toLocaleTimeString('ru-RU', timeOptions)}
      </div>
      <div className="calendar-tz">🌍 {timezone}</div>
    </div>
  )
}