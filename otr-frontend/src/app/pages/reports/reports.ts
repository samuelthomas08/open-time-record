import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { Project, ProjectService, TimeEntry, TimeEntryService, User, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { BarChart, type BarChartDatum } from '@/components/bar-chart/bar-chart';
import { TrendChart, type TrendPoint } from '@/components/trend-chart/trend-chart';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';

type Period = 7 | 30 | 90 | 365;

const PERIODS: ReadonlyArray<{ value: Period; label: string }> = [
  { value: 7, label: '7 Tage' },
  { value: 30, label: '30 Tage' },
  { value: 90, label: '90 Tage' },
  { value: 365, label: 'Jahr' },
];

/** Bucket by week rather than by day once a period gets long, so the trend chart doesn't plot hundreds of points. */
const WEEKLY_BUCKET_THRESHOLD_DAYS = 60;

function formatHours(hours: number): string {
  const totalMinutes = Math.round(hours * 60);
  const h = Math.floor(totalMinutes / 60);
  const m = totalMinutes % 60;
  return `${h}:${m.toString().padStart(2, '0')} Std.`;
}

function toDateKey(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function startOfWeek(date: Date): Date {
  const result = new Date(date);
  const mondayIndex = (result.getDay() + 6) % 7;
  result.setDate(result.getDate() - mondayIndex);
  result.setHours(0, 0, 0, 0);
  return result;
}

function isoWeekNumber(date: Date): number {
  const target = new Date(date);
  target.setHours(0, 0, 0, 0);
  target.setDate(target.getDate() + 3 - ((target.getDay() + 6) % 7));
  const firstThursday = new Date(target.getFullYear(), 0, 4);
  const diff = target.getTime() - firstThursday.getTime();
  return 1 + Math.round(diff / (7 * 24 * 3600 * 1000));
}

function netHours(entry: TimeEntry): number {
  if (!entry.startTime || !entry.endTime) {
    return 0;
  }

  const breakMs = (entry.breaks ?? []).reduce((sum, b) => {
    if (!b.startTime || !b.endTime) {
      return sum;
    }
    return sum + (new Date(b.endTime).getTime() - new Date(b.startTime).getTime());
  }, 0);

  const grossMs = new Date(entry.endTime).getTime() - new Date(entry.startTime).getTime();
  return Math.max(0, grossMs - breakMs) / 3_600_000;
}

@Component({
  selector: 'app-reports',
  imports: [BarChart, TrendChart, ZardButtonComponent, ZardCardComponent, ZardCardContentComponent],
  templateUrl: './reports.html',
})
export class Reports implements OnInit {
  private readonly userService = inject(UserService);
  private readonly timeEntryService = inject(TimeEntryService);
  private readonly projectService = inject(ProjectService);

  protected readonly authService = inject(AuthService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly managedUsers = signal<User[]>([]);
  protected readonly entries = signal<TimeEntry[]>([]);
  protected readonly projects = signal<Project[]>([]);

  protected readonly period = signal<Period>(30);
  protected readonly PERIODS = PERIODS;
  protected readonly formatHours = formatHours;
  protected readonly trendValueFormatter = (value: number): string => formatHours(value);

  private readonly cutoff = computed(() => {
    const date = new Date();
    date.setDate(date.getDate() - this.period());
    date.setHours(0, 0, 0, 0);
    return date;
  });

  private readonly completedEntriesInPeriod = computed(() =>
    this.entries().filter(e => e.startTime && e.endTime && new Date(e.startTime) >= this.cutoff()),
  );

  protected readonly totalHours = computed(() => this.completedEntriesInPeriod().reduce((sum, e) => sum + netHours(e), 0));
  protected readonly employeeCount = computed(() => this.managedUsers().length);
  protected readonly averagePerEmployee = computed(() => {
    const count = this.employeeCount();
    return count === 0 ? 0 : this.totalHours() / count;
  });

  protected readonly employeeBarData = computed<BarChartDatum[]>(() => {
    const byUser = new Map<number, number>();
    for (const entry of this.completedEntriesInPeriod()) {
      const userId = Number(entry.userId);
      byUser.set(userId, (byUser.get(userId) ?? 0) + netHours(entry));
    }

    return this.managedUsers()
      .map(user => ({ label: `${user.firstName} ${user.lastName}`, value: byUser.get(Number(user.id)) ?? 0 }))
      .sort((a, b) => b.value - a.value)
      .map(item => ({ ...item, displayValue: formatHours(item.value) }));
  });

  protected readonly projectBarData = computed<BarChartDatum[]>(() => {
    const byProject = new Map<string, number>();
    for (const entry of this.completedEntriesInPeriod()) {
      const key = entry.projectId != null ? String(entry.projectId) : 'none';
      byProject.set(key, (byProject.get(key) ?? 0) + netHours(entry));
    }

    const projectName = (key: string): string => {
      if (key === 'none') {
        return 'Ohne Projekt';
      }
      return this.projects().find(p => String(p.id) === key)?.projectDisplayName ?? 'Unbekannt';
    };

    return Array.from(byProject.entries())
      .map(([key, value]) => ({ label: projectName(key), value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 8)
      .map(item => ({ ...item, displayValue: formatHours(item.value) }));
  });

  protected readonly trendData = computed<TrendPoint[]>(() => {
    const weekly = this.period() > WEEKLY_BUCKET_THRESHOLD_DAYS;
    const byBucket = new Map<string, number>();

    for (const entry of this.completedEntriesInPeriod()) {
      const date = new Date(entry.startTime!);
      const key = weekly ? toDateKey(startOfWeek(date)) : toDateKey(date);
      byBucket.set(key, (byBucket.get(key) ?? 0) + netHours(entry));
    }

    // Walk every day/week in the selected period — not just the ones with entries — so a
    // sparse period reads as "N days, most of them empty" instead of collapsing to a couple
    // of points that visually compress the whole window down to almost nothing.
    const step = weekly ? 7 : 1;
    const bucketDates: Date[] = [];
    let cursor = weekly ? startOfWeek(this.cutoff()) : new Date(this.cutoff());
    const end = weekly ? startOfWeek(new Date()) : new Date(new Date().setHours(0, 0, 0, 0));
    while (cursor <= end) {
      bucketDates.push(new Date(cursor));
      cursor = new Date(cursor);
      cursor.setDate(cursor.getDate() + step);
    }

    return bucketDates.map(date => {
      const key = toDateKey(date);
      const value = byBucket.get(key) ?? 0;
      const label = weekly ? `KW ${isoWeekNumber(date)}` : date.toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit' });
      return { label, value, title: `${label}: ${formatHours(value)}` };
    });
  });

  ngOnInit(): void {
    this.load();
  }

  protected setPeriod(value: Period): void {
    this.period.set(value);
  }

  private async load(): Promise<void> {
    this.loading.set(true);

    try {
      const [managedUsers, entries, projects] = await Promise.all([
        this.userService.apiUserManagedGet$Json(),
        this.timeEntryService.apiTimeEntryTeamGet$Json(),
        this.projectService.apiProjectGet$Json(),
      ]);
      this.managedUsers.set(managedUsers);
      this.entries.set(entries);
      this.projects.set(projects);
    } catch {
      this.error.set('Auswertungen konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
