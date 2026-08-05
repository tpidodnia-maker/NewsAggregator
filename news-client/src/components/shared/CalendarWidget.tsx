
import { useState, useEffect } from 'react'
import { Globe } from 'lucide-react'

export const CalendarWidget = () => {
  const [now, setNow] = useState(new Date())
  const [timezone, setTimezone] = useState('UTC')

  useEffect(() => {
    try {
      const tz = Intl.DateTimeFormat().resolvedOptions().timeZone
      if (tz) setTimezone(tz)
    } catch {
      setTimezone('UTC')
    }

    const timer = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(timer)
  }, [])

  const dateStr = (() => {
    try {
      return now.toLocaleDateString('ru-RU', {
        timeZone: timezone,
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    } catch {
      return now.toLocaleDateString('ru-RU')
    }
  })()

  const timeStr = (() => {
    try {
      return now.toLocaleTimeString('ru-RU', {
        timeZone: timezone,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      })
    } catch {
      return now.toLocaleTimeString('ru-RU')
    }
  })()

  return (
    <div className="calendar-widget">
      <div className="widget-title">Дата и время</div>
      <div className="calendar-date">{dateStr}</div>
      <div className="calendar-time">{timeStr}</div>
      <div className="calendar-tz">
        <Globe size={14} strokeWidth={2} />
        {timezone}
      </div>
    </div>
  )
}
