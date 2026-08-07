import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { Eye } from 'lucide-react'
import { newsApi, NewsDetail as INewsDetail, getImageUrl } from '../../api/api'
import { getCategoryIcon } from '../../lib/categoryIcons'

export const NewsDetail = () => {
  const { id } = useParams<{ id: string }>()
  const [news, setNews] = useState<INewsDetail | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    newsApi.getNewsById(Number(id))
      .then(res => setNews(res.data))
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return <div className="loading">Загрузка...</div>
  if (!news) return <div className="error-state">Новость не найдена</div>

  const date = new Date(news.publishedDate).toLocaleDateString('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })

  const CategoryIcon = getCategoryIcon(news.categoryName)
  const imgSrc = getImageUrl(news.imageUrl)

  return (
    <div className="news-detail">
      <Link to="/" className="back-link">Назад</Link>
      {imgSrc && (
        <img
          src={imgSrc}
          alt=""
          className="news-detail__image"
          onError={e => { (e.currentTarget as HTMLImageElement).style.display = 'none' }}
        />
      )}
      <div className="news-detail__meta">
        <span className="news-card__category">
          <CategoryIcon size={15} strokeWidth={2} />
          {news.categoryName}
        </span>
        <span className="news-card__source">{news.source}</span>
        <span className="news-card__date">{date}</span>
        <span className="news-card__views">
          <Eye size={15} strokeWidth={2} />
          {news.viewCount}
        </span>
      </div>
      <h1 className="news-detail__title">{news.title}</h1>
      <div className="news-detail__content">
        {news.fullContent || news.content}
      </div>
      <a
        href={news.url}
        target="_blank"
        rel="noopener noreferrer"
        className="btn-primary"
      >
        Читать на {news.source}
      </a>
    </div>
  )
}