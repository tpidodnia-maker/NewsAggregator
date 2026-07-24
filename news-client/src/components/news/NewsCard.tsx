import { Link } from 'react-router-dom'
import { NewsItem, recommendationsApi } from '../../api/api'

interface Props {
  news: NewsItem
}

export const NewsCard = ({ news }: Props) => {
  const date = new Date(news.publishedDate).toLocaleDateString('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric'
  })

  const handleClick = () => {
    const token = localStorage.getItem('accessToken')
    if (token) {
      recommendationsApi.track(news.id, news.categoryId).catch(() => {})
    }
  }

  return (
    <div className="news-card">
      <div className="news-card__header">
        <span className="news-card__category">
          {news.categoryIcon} {news.categoryName}
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
        <span className="news-card__views">👁 {news.viewCount}</span>
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