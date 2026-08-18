import { Component, computed, input } from '@angular/core';

interface HeatmapDay {
  dateKey: string;
  date: Date;
  hours: number;
  level: 0 | 1 | 2 | 3 | 4;
  isPadding: boolean;
}

interface HeatmapWeek {
  days: HeatmapDay[];
}

interface MonthLabel {
  weekIndex: number;
  label: string;
}

const WEEKS_OF_HISTORY = 53;
const DAY_LABELS = ['Mo', '', 'Mi', '', 'Fr', '', ''];
const MONTH_NAMES = [
  'Jan',
  'Feb',
  'Mär',
  'Apr',
  'Mai',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Okt',
  'Nov',
  'Dez',
];
const LEVEL_CLASSES = [
  'bg-muted',
  'bg-green-500/25',
  'bg-green-500/45',
  'bg-green-500/70',
  'bg-green-600 dark:bg-green-500',
];

function toDateKey(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

function startOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);
  return result;
}

/** Monday-indexed day of week: Monday = 0 ... Sunday = 6. */
function mondayIndex(date: Date): number {
  return (date.getDay() + 6) % 7;
}

function levelFor(hours: number): 0 | 1 | 2 | 3 | 4 {
  if (hours <= 0) {
    return 0;
  }
  if (hours < 2) {
    return 1;
  }
  if (hours < 4) {
    return 2;
  }
  if (hours < 6) {
    return 3;
  }
  return 4;
}

@Component({
  selector: 'app-contribution-heatmap',
  templateUrl: './contribution-heatmap.html',
})
export class ContributionHeatmap {
  readonly hoursByDate = input.required<Record<string, number>>();

  protected readonly dayLabels = DAY_LABELS;
  protected readonly legendLevels = [0, 1, 2, 3, 4] as const;

  protected readonly weeks = computed<HeatmapWeek[]>(() => {
    const hoursByDate = this.hoursByDate();
    const today = startOfDay(new Date());
    const rangeStart = addDays(today, -(WEEKS_OF_HISTORY * 7 - 1));
    const gridStart = addDays(rangeStart, -mondayIndex(rangeStart));

    const weeks: HeatmapWeek[] = [];
    let week: HeatmapDay[] = [];
    let cursor = gridStart;

    while (cursor <= today) {
      const dateKey = toDateKey(cursor);
      const hours = hoursByDate[dateKey] ?? 0;
      week.push({ dateKey, date: cursor, hours, level: levelFor(hours), isPadding: false });

      if (week.length === 7) {
        weeks.push({ days: week });
        week = [];
      }
      cursor = addDays(cursor, 1);
    }

    if (week.length > 0) {
      while (week.length < 7) {
        week.push({ dateKey: toDateKey(cursor), date: cursor, hours: 0, level: 0, isPadding: true });
        cursor = addDays(cursor, 1);
      }
      weeks.push({ days: week });
    }

    return weeks;
  });

  protected readonly monthLabels = computed<MonthLabel[]>(() => {
    const labels: MonthLabel[] = [];
    let lastMonth = -1;

    this.weeks().forEach((week, weekIndex) => {
      const firstRealDay = week.days.find(day => !day.isPadding);
      if (!firstRealDay) {
        return;
      }

      const month = firstRealDay.date.getMonth();
      if (month !== lastMonth) {
        labels.push({ weekIndex, label: MONTH_NAMES[month] });
        lastMonth = month;
      }
    });

    return labels;
  });

  protected readonly totalHoursLabel = computed(() => {
    const total = Object.values(this.hoursByDate()).reduce((sum, hours) => sum + hours, 0);
    return total.toLocaleString('de-DE', { minimumFractionDigits: 0, maximumFractionDigits: 1 });
  });

  protected readonly activeDays = computed(() => Object.values(this.hoursByDate()).filter(hours => hours > 0).length);

  protected monthLabelAt(weekIndex: number): string {
    return this.monthLabels().find(label => label.weekIndex === weekIndex)?.label ?? '';
  }

  protected cellClass(day: HeatmapDay): string {
    return day.isPadding ? 'invisible' : LEVEL_CLASSES[day.level];
  }

  protected legendClass(level: number): string {
    return LEVEL_CLASSES[level];
  }

  protected cellTitle(day: HeatmapDay): string | null {
    if (day.isPadding) {
      return null;
    }

    const formattedDate = day.date.toLocaleDateString('de-DE', { day: 'numeric', month: 'long', year: 'numeric' });
    if (day.hours <= 0) {
      return `Keine Arbeitszeit am ${formattedDate}`;
    }

    const hours = Math.floor(day.hours);
    const minutes = Math.round((day.hours - hours) * 60);
    return `${hours}:${minutes.toString().padStart(2, '0')} Std. am ${formattedDate}`;
  }
}
