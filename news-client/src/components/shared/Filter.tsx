import { Search } from 'lucide-react'
import { DateRangePicker } from './DateRangePicker'

interface Props {
  search: string
  dateFrom?: string
  dateTo?: string
  sortBy: string
  onSearchChange: (v: string) => void
  onDateFromChange: (v: string) => void
  onDateToChange: (v: string) => void
  onSortChange: (v: string) => void
  onClearDates: () => void
}

export const Filter = ({
  search, dateFrom, dateTo, sortBy,
  onSearchChange, onDateFromChange, onDateToChange,
  onSortChange, onClearDates
}: Props) => (
  <div className="filter-bar">
    <div className="search-input-wrap">
      <Search size={16} strokeWidth={2} className="search-input-icon" />
      <input
        className="search-input"
        type="text"
        placeholder="Поиск новостей..."
        value={search}
        onChange={e => onSearchChange(e.target.value)}
      />
    </div>
    <DateRangePicker
      dateFrom={dateFrom}
      dateTo={dateTo}
      onDateFromChange={onDateFromChange}
      onDateToChange={onDateToChange}
      onClear={onClearDates}
    />
    <select
      className="sort-select"
      value={sortBy}
      onChange={e => onSortChange(e.target.value)}
    >
      <option value="date">По дате</option>
      <option value="title">По названию</option>
      <option value="views">По популярности</option>
    </select>
  </div>
)