import { Calendar, X } from 'lucide-react'

interface Props {
  dateFrom?: string
  dateTo?: string
  onDateFromChange: (date: string) => void
  onDateToChange: (date: string) => void
  onClear: () => void
}

export const DateRangePicker = ({
  dateFrom, dateTo, onDateFromChange, onDateToChange, onClear
}: Props) => (
  <div className="date-range-picker">
    <span className="date-range-label">
      <Calendar size={15} strokeWidth={2} />
      Период:
    </span>
    <input
      type="date"
      value={dateFrom ?? ''}
      onChange={e => onDateFromChange(e.target.value)}
      max={dateTo || undefined}
    />
    <span>—</span>
    <input
      type="date"
      value={dateTo ?? ''}
      onChange={e => onDateToChange(e.target.value)}
      min={dateFrom || undefined}
    />
    {(dateFrom || dateTo) && (
      <button onClick={onClear} className="btn-clear" aria-label="Очистить период">
        <X size={14} strokeWidth={2} />
      </button>
    )}
  </div>
)