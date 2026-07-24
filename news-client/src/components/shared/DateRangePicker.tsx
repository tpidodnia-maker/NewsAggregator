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
    <span className="date-range-label">📆 Период:</span>
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
      <button onClick={onClear} className="btn-clear">✕</button>
    )}
  </div>
)
