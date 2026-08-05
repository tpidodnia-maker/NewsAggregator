
import axios from 'axios'

const BASE_URL = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000'

export const api = axios.create({
  baseURL: BASE_URL + '/api',
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = 'Bearer ' + token
  }
  return config
})

api.interceptors.response.use(
  res => res,
  async error => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true
      const refresh = localStorage.getItem('refreshToken')
      if (refresh) {
        try {
          const { data } = await axios.post(
            BASE_URL + '/api/auth/refresh',
            { refreshToken: refresh }
          )
          localStorage.setItem('accessToken', data.accessToken)
          localStorage.setItem('refreshToken', data.refreshToken)
          original.headers.Authorization = 'Bearer ' + data.accessToken
          return api(original)
        } catch {
          localStorage.clear()
          window.location.href = '/login'
        }
      }
    }
    return Promise.reject(error)
  }
)

export interface NewsItem {
  id: number
  title: string
  content: string
  url: string
  source: string
  publishedDate: string
  categoryName: string
  categoryIcon: string
  categoryId: number
  viewCount: number
  imageUrl?: string
}

export interface NewsDetail extends NewsItem {
  fullContent?: string
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface Category {
  id: number
  name: string
  icon: string
  description?: string
  newsCount: number
}

export interface CurrencyRate {
  code: string
  name: string
  flag: string
  rate: number
  change: number
  updatedAt: string
}

export const newsApi = {
  getNews: (
    page: number = 1,
    categoryId?: number,
    sortBy: string = 'date',
    dateFrom?: string,
    dateTo?: string,
    search?: string
  ) =>
    api.get<PagedResult<NewsItem>>('/news', {
      params: { page, pageSize: 20, categoryId, sortBy, dateFrom, dateTo, search }
    }),

  getNewsById: (id: number) =>
    api.get<NewsDetail>('/news/' + id),

  parseNews: () =>
    api.post('/news/parse')
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<{
      accessToken: string
      refreshToken: string
      username: string
      role: string
    }>('/auth/login', { email, password }),

  register: (username: string, email: string, password: string) =>
    api.post<{
      accessToken: string
      refreshToken: string
      username: string
      role: string
    }>('/auth/register', { username, email, password }),

  me: () =>
    api.get<{
      id: number
      username: string
      email: string
      role: string
    }>('/auth/me'),
    forgotPassword: (email: string) =>
    api.post('/auth/forgot-password', { email }),

  resetPassword: (token: string, newPassword: string) =>
    api.post('/auth/reset-password', { token, newPassword }),

  refresh: (refreshToken: string) =>
    api.post('/auth/refresh', { refreshToken })
}

export const categoriesApi = {
  getAll: () => api.get<Category[]>('/categories')
}

export const currencyApi = {
  getRates: () => api.get<CurrencyRate[]>('/currency')
}

export const recommendationsApi = {
  get: () => api.get<NewsItem[]>('/recommendations'),
  track: (newsId: number, categoryId: number) =>
    api.post('/recommendations/track/' + newsId + '?categoryId=' + categoryId)
}
