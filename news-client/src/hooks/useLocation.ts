import { useState, useEffect } from 'react'

export const useUserLocation = () => {
  const [timezone, setTimezone] = useState('')
  const [locale, setLocale]     = useState('ru-RU')

  useEffect(() => {
    const tz = Intl.DateTimeFormat().resolvedOptions().timeZone
    const lc = navigator.language || 'ru-RU'
    setTimezone(tz)
    setLocale(lc)
  }, [])

  return { timezone, locale }
}