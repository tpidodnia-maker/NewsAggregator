import { useState, useCallback } from 'react'
import { newsApi, NewsItem, PagedResult } from '../api/api'

export const useNews = () => {
  const [data, setData]       = useState<PagedResult<NewsItem> | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError]     = useState<string | null>(null)

  const fetchNews = useCallback(async (
    page = 1,
    categoryId?: number,
    sortBy = 'date',
    dateFrom?: string,
    dateTo?: string,
    search?: string
  ) => {
    setLoading(true)
    setError(null)
    try {
      const res = await newsApi.getNews(page, categoryId, sortBy, dateFrom, dateTo, search)
      setData(res.data)
    } catch {
      setError('Ошибка загрузки новостей')
    } finally {
      setLoading(false)
    }
  }, [])

  return { data, loading, error, fetchNews }
}