import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { TimeEntry, TimeEntryCorrectionRequestService, TimeEntryService, UserInvitationService, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { TimerService } from '@/services/timer.service';
import { ContributionHeatmap } from '@/components/contribution-heatmap/contribution-heatmap';
import { Reports } from '@/pages/reports/reports';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';
import { formatHours, startOfMonth, startOfWeek, toDateKey } from '@/utils/date.utils';
import { MS_PER_HOUR, netWorkedMs } from '@/utils/time-entry.utils';

/** Landing page: live timer, hour statistics, activity heatmap and (for Superadmins) admin overview tiles. */
@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, ContributionHeatmap, Reports, ZardCardComponent, ZardCardContentComponent],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly timeEntryService = inject(TimeEntryService);
  private readonly correctionRequestService = inject(TimeEntryCorrectionRequestService);
  private readonly userInvitationService = inject(UserInvitationService);
  private readonly userService = inject(UserService);

  protected readonly authService = inject(AuthService);
  protected readonly timerService = inject(TimerService);

  protected readonly todayLabel = new Date().toLocaleDateString('de-DE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });
  protected readonly loading = signal<boolean>(true);
  protected readonly historicalHoursByDate = signal<Record<string, number>>({});

  protected readonly pendingCorrectionsCount = signal<number | null>(null);
  protected readonly pendingInvitationsCount = signal<number | null>(null);
  protected readonly activeUsersCount = signal<number | null>(null);

  /** The running entry's live elapsed time folds into today's bucket, so the heatmap ticks too. */
  protected readonly hoursByDate = computed(() => {
    const byDate = { ...this.historicalHoursByDate() };

    if (this.timerService.isRunning()) {
      const todayKey = toDateKey(new Date());
      byDate[todayKey] = (byDate[todayKey] ?? 0) + this.elapsedToHours(this.timerService.elapsed());
    }

    return byDate;
  });

  protected readonly todayHours = computed(() => this.hoursByDate()[toDateKey(new Date())] ?? 0);
  protected readonly weekHours = computed(() => this.sumSince(this.hoursByDate(), startOfWeek(new Date())));
  protected readonly monthHours = computed(() => this.sumSince(this.hoursByDate(), startOfMonth(new Date())));

  protected readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 5) return 'Noch spät unterwegs';
    if (hour < 11) return 'Guten Morgen';
    if (hour < 18) return 'Guten Tag';
    return 'Guten Abend';
  });

  protected readonly formatHours = formatHours;

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    this.timerService.ensureLoaded();
    this.load();

    if (this.authService.isSuperadmin()) {
      this.loadAdminStats();
    }
  }

  /** Fetches the data shown on this page and updates loading/error state. */
  private async load(): Promise<void> {
    this.loading.set(true);

    try {
      const entries = await this.timeEntryService.apiTimeEntryMineGet$Json();
      this.historicalHoursByDate.set(this.aggregateByDate(entries));
    } catch {
      // Non-critical — the dashboard just shows an empty heatmap/stats.
    } finally {
      this.loading.set(false);
    }
  }

  /** Fetches the counters shown in the admin overview tiles. */
  private async loadAdminStats(): Promise<void> {
    try {
      const [pending, invitations, users] = await Promise.all([
        this.correctionRequestService.apiTimeEntryCorrectionRequestPendingGet$Json(),
        this.userInvitationService.apiUserInvitationGet$Json(),
        this.userService.apiUserGet$Json(),
      ]);
      this.pendingCorrectionsCount.set(pending.length);
      this.pendingInvitationsCount.set(invitations.length);
      this.activeUsersCount.set(users.filter(u => u.isActive).length);
    } catch {
      // Non-critical — the admin tiles just stay hidden.
    }
  }

  /** Sums the net worked hours per local calendar day. */
  private aggregateByDate(entries: TimeEntry[]): Record<string, number> {
    const result: Record<string, number> = {};

    for (const entry of entries) {
      if (!entry.startTime || !entry.endTime) {
        continue; // the currently-running entry has no end yet — handled live via TimerService instead
      }

      const key: string = toDateKey(new Date(entry.startTime));
      result[key] = (result[key] ?? 0) + netWorkedMs(entry) / MS_PER_HOUR;
    }

    return result;
  }

  /** Sums the hours of all days on or after the given date. */
  private sumSince(hoursByDate: Record<string, number>, since: Date): number {
    let sum = 0;
    for (const [key, hours] of Object.entries(hoursByDate)) {
      if (new Date(key) >= since) {
        sum += hours;
      }
    }
    return sum;
  }

  /** Converts an `hh:mm:ss` string into fractional hours. */
  private elapsedToHours(elapsed: string): number {
    const [hours, minutes, seconds] = elapsed.split(':').map(Number);
    return hours + minutes / 60 + seconds / 3600;
  }
}
