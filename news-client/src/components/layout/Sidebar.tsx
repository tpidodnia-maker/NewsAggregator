import { useEffect, useState } from 'react'
import { categoriesApi, Category } from '../../api/api'
import { CurrencyWidget } from '../shared/CurrencyWidget'
import { CalendarWidget } from '../shared/CalendarWidget'
import { getCategoryIcon } from '../../lib/categoryIcons'

interface Props {
  selectedCategoryId?: number
  onCategoryChange: (id?: number) => void
}

export const Sidebar = ({ selectedCategoryId, onCategoryChange }: Props) => {
  const [categories, setCategories] = useState<Category[]>([])

  useEffect(() => {
    categoriesApi.getAll().then(res => setCategories(res.data))
  }, [])

  return (
    <aside className="sidebar">
      <CalendarWidget />
      <CurrencyWidget />
      <div className="sidebar-categories">
        <h3 className="widget-title">Категории</h3>
        <ul className="category-list">
          <li
            className={'category-item' + (!selectedCategoryId ? ' active' : '')}
            onClick={() => onCategoryChange(undefined)}
          >
            Все новости
          </li>
          {categories.map(cat => {
            const Icon = getCategoryIcon(cat.name)
            return (
              <li
                key={cat.id}
                className={'category-item' + (selectedCategoryId === cat.id ? ' active' : '')}
                onClick={() => onCategoryChange(cat.id)}
              >
                <span className="category-item__label">
                  <Icon size={15} strokeWidth={2} />
                  {cat.name}
                </span>
                <span className="category-count">{cat.newsCount}</span>
              </li>
            )
          })}
        </ul>
      </div>
    </aside>
  )
}