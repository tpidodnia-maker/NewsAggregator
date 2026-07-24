import { useEffect, useState } from 'react'
import { currencyApi, CurrencyRate } from '../../api/api'

export const CurrencyWidget = () => {
  const [rates, setRates]     = useState<CurrencyRate[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    currencyApi.getRates()
      .then(res => setRates(res.data))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="currency-loading">Загрузка курсов...</div>

  return (
    <div className="currency-widget">
      <h3 className="widget-title">💱 Курс валют</h3>
      <div className="currency-list">
        {rates.map(rate => (
          <div key={rate.code} className="currency-item">
            <span className="currency-flag">{rate.flag}</span>
            <span className="currency-code">{rate.code}</span>
            <span className="currency-rate">{rate.rate.toFixed(4)}</span>
          </div>
        ))}
      </div>
      <p className="currency-updated">
        Обновлено: {new Date(rates[0]?.updatedAt).toLocaleTimeString('ru-RU')}
      </p>
    </div>
  )
}