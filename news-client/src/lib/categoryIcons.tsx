import {
  Newspaper,
  Landmark,
  TrendingUp,
  Trophy,
  Cpu,
  FlaskConical,
  Drama,
  HeartPulse,
  Briefcase,
  Leaf,
  Clapperboard,
  type LucideIcon,
} from 'lucide-react'

/**
 * Сопоставление категории новости с иконкой lucide-react.
 * Ключ — categoryName, как он приходит с бэкенда (см. ResolveCategoryId в ParserService).
 * Если категория не найдена — используется Newspaper.
 */
const CATEGORY_ICON_MAP: Record<string, LucideIcon> = {
  'Политика': Landmark,
  'Экономика': TrendingUp,
  'Спорт': Trophy,
  'Технологии': Cpu,
  'Наука': FlaskConical,
  'Культура': Drama,
  'Здоровье': HeartPulse,
  'Бизнес': Briefcase,
  'Экология': Leaf,
  'Развлечения': Clapperboard,
}

export const getCategoryIcon = (categoryName: string): LucideIcon => {
  return CATEGORY_ICON_MAP[categoryName] ?? Newspaper
}
