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
    <input
      className="search-input"
      type="text"
      placeholder="🔍 Поиск новостей..."
      value={search}
      onChange={e => onSearchChange(e.target.value)}
    />
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