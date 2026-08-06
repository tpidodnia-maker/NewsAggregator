import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Eye, ImageOff } from 'lucide-react'
import { NewsItem, recommendationsApi, getImageUrl } from '../../api/api'
import { getCategoryIcon } from '../../lib/categoryIcons'

interface Props { news: NewsItem }

export const NewsCard = ({ news }: Props) => {
  const [imgError, setImgError] = useState(false)
  const src = getImageUrl(news.imageUrl)

  const date = new Date(news.publishedDate).toLocaleDateString('ru-RU', {
    day: '2-digit', month: 'long', year: 'numeric'
  })
  const CategoryIcon = getCategoryIcon(news.categoryName)

  const handleClick = () => {
    const token = localStorage.getItem('accessToken')
    if (token) recommendationsApi.track(news.id, news.categoryId).catch(() => {})
  }

  return (
    <div className="news-card">
      <Link to={'/news/' + news.id} onClick={handleClick} className="news-card__media">
        {src && !imgError ? (
          <img
            src={src}
            alt=""
            loading="lazy"
            className="news-card__image"
            onError={() => setImgError(true)}
          />
        ) : (
          <div className="news-card__image news-card__image--placeholder">
            <ImageOff size={22} strokeWidth={1.5} />
          </div>
        )}
      </Link>

      <div className="news-card__header">
        <span className="news-card__category">
          <CategoryIcon size={14} strokeWidth={2} />
          {news.categoryName}
        </span>
        <span className="news-card__source">{news.source}</span>
      </div>
      <h3 className="news-card__title">
        <Link to={'/news/' + news.id} onClick={handleClick}>
          {news.title}
        </Link>
      </h3>
      <p className="news-card__content">
        {news.content.substring(0, 120)}
        {news.content.length > 120 ? '...' : ''}
      </p>
      <div className="news-card__footer">
        <span className="news-card__date">{date}</span>
        <span className="news-card__views">
          <Eye size={14} strokeWidth={2} />
          {news.viewCount}
        </span>
        <a
          href={news.url}
          target="_blank"
          rel="noopener noreferrer"
          className="news-card__link"
        >
          Оригинал
        </a>
      </div>
    </div>
  )
}
