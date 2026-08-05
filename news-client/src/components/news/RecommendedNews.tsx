
import { useEffect, useState } from 'react'
import { Star } from 'lucide-react'
import { recommendationsApi, NewsItem } from '../../api/api'
import { NewsCard } from './NewsCard'

export const RecommendedNews = () => {
  const [news, setNews]       = useState<NewsItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    recommendationsApi.get()
      .then(res => setNews(res.data))
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Загрузка рекомендаций...</div>
  if (!news.length) return null

  return (
    <div className="recommended-section">
      <h2 className="section-title">
        <Star size={18} strokeWidth={2} />
        Рекомендовано для вас
      </h2>
      <div className="news-grid">
        {news.map(item => (
          <NewsCard key={item.id} news={item} />
        ))}
      </div>
    </div>
  )
}
