
import { useEffect, useState, useCallback } from 'react'
import { RefreshCw, Loader2 } from 'lucide-react'
import { newsApi, NewsItem } from '../../api/api'
import { NewsCard } from './NewsCard'
import { RecommendedNews } from './RecommendedNews'
import { Filter } from '../shared/Filter'
import { Sidebar } from '../layout/Sidebar'
import { useAuth } from '../../contexts/AuthContext'

export const NewsList = () => {
  const [news, setNews]             = useState<NewsItem[]>([])
  const [page, setPage]             = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [categoryId, setCategoryId] = useState<number | undefined>()
  const [sortBy, setSortBy]         = useState('date')
  const [search, setSearch]         = useState('')
  const [dateFrom, setDateFrom]     = useState<string | undefined>()
  const [dateTo, setDateTo]         = useState<string | undefined>()
  const [loading, setLoading]       = useState(false)
  const [parsing, setParsing]       = useState(false)
  const { isAdmin }                 = useAuth()

  const loadNews = useCallback(async () => {
    setLoading(true)
    try {
      const { data } = await newsApi.getNews(
        page, categoryId, sortBy, dateFrom, dateTo, search
      )
      setNews(data.items)
      setTotalPages(data.totalPages)
      setTotalCount(data.totalCount)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [page, categoryId, sortBy, dateFrom, dateTo, search])

  useEffect(() => { loadNews() }, [loadNews])

  const handleCategoryChange = (id?: number) => {
    setCategoryId(id)
    setPage(1)
  }
  const handleParse = async () => {
    setParsing(true)
    try {
      await newsApi.parseNews()
      await loadNews()
      alert('Парсинг завершён!')
    } catch {
      alert('Ошибка парсинга')
    } finally {
      setParsing(false)
    }
  }

  return (
    <div className="main-layout">
      <Sidebar
        selectedCategoryId={categoryId}
        onCategoryChange={handleCategoryChange}
      />

      <div className="content-area">
        <div className="content-header">
          <h1 className="page-title">
            {categoryId ? 'Новости' : 'Все новости'}
            <span className="total-count"> ({totalCount})</span>
          </h1>
          {isAdmin && (
            <button
              className="btn-primary"
              onClick={handleParse}
              disabled={parsing}
            >
              {parsing
                ? <><Loader2 size={16} className="spin" /> Парсинг...</>
                : <><RefreshCw size={16} /> Обновить новости</>}
            </button>
          )}
        </div>

        <Filter
          search={search}
          dateFrom={dateFrom}
          dateTo={dateTo}
          sortBy={sortBy}
          onSearchChange={v => { setSearch(v); setPage(1) }}
          onDateFromChange={v => { setDateFrom(v); setPage(1) }}
          onDateToChange={v => { setDateTo(v); setPage(1) }}
          onSortChange={v => { setSortBy(v); setPage(1) }}
          onClearDates={() => { setDateFrom(undefined); setDateTo(undefined) }}
        />

        <RecommendedNews />

        {loading ? (
          <div className="loading">Загрузка новостей...</div>
        ) : news.length === 0 ? (
          <div className="empty-state">
            <p>Новостей не найдено.</p>
            {isAdmin && (
              <button className="btn-primary" onClick={handleParse}>
                Запустить парсинг
              </button>
            )}
          </div>
        ) : (
          <div className="news-grid">
            {news.map(item => (
              <NewsCard key={item.id} news={item} />
            ))}
          </div>
        )}

        {totalPages > 1 && (
          <div className="pagination">
            <button
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1}
            >← Назад</button>
            <span>{page} / {totalPages}</span>
            <button
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
            >Вперёд →</button>
          </div>
        )}
      </div>
    </div>
  )
}
