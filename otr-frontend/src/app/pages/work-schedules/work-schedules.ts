import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { User, UserService, WorkSchedule, WorkScheduleService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardTableImports } from '@/shared/components/table';
import { ManageWorkScheduleDialog } from './manage-work-schedule-dialog/manage-work-schedule-dialog';

/** Truncates to the local calendar day so a schedule created later today (e.g. 14:32) still counts as "from today", not "from the future". */
function toDateOnly(date: Date): number {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
}

function isCurrentSchedule(schedule: WorkSchedule): boolean {
  const today = toDateOnly(new Date());
  const from = toDateOnly(new Date(schedule.effectiveFrom!));
  const to = schedule.effectiveTo ? toDateOnly(new Date(schedule.effectiveTo)) : null;
  return from <= today && (to === null || to >= today);
}

interface DayColumn {
  key: 'mondayHours' | 'tuesdayHours' | 'wednesdayHours' | 'thursdayHours' | 'fridayHours' | 'saturdayHours' | 'sundayHours';
  label: string;
  /** JS Date.getDay() convention (0 = Sunday) — used to highlight the current weekday. */
  dayIndex: number;
}

const DAY_COLUMNS: DayColumn[] = [
  { key: 'mondayHours', label: 'Mo', dayIndex: 1 },
  { key: 'tuesdayHours', label: 'Di', dayIndex: 2 },
  { key: 'wednesdayHours', label: 'Mi', dayIndex: 3 },
  { key: 'thursdayHours', label: 'Do', dayIndex: 4 },
  { key: 'fridayHours', label: 'Fr', dayIndex: 5 },
  { key: 'saturdayHours', label: 'Sa', dayIndex: 6 },
  { key: 'sundayHours', label: 'So', dayIndex: 0 },
];

export function weeklyTotal(schedule: WorkSchedule): number {
  return (
    Number(schedule.mondayHours ?? 0) +
    Number(schedule.tuesdayHours ?? 0) +
    Number(schedule.wednesdayHours ?? 0) +
    Number(schedule.thursdayHours ?? 0) +
    Number(schedule.fridayHours ?? 0) +
    Number(schedule.saturdayHours ?? 0) +
    Number(schedule.sundayHours ?? 0)
  );
}

@Component({
  selector: 'app-work-schedules',
  imports: [DatePipe, ZardButtonComponent, ZardCardComponent, ZardCardContentComponent, ZardTableImports],
  templateUrl: './work-schedules.html',
})
export class WorkSchedules implements OnInit {
  private readonly userService = inject(UserService);
  private readonly workScheduleService = inject(WorkScheduleService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly authService = inject(AuthService);
  protected readonly managedUsers = signal<User[]>([]);
  protected readonly schedules = signal<WorkSchedule[]>([]);
  protected readonly mySchedules = signal<WorkSchedule[]>([]);
  protected readonly myUser = signal<User | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly dayColumns = DAY_COLUMNS;

  protected readonly myCurrentSchedule = computed(() => this.mySchedules().find(isCurrentSchedule) ?? null);
  /** Mirrors the backend: nobody manages someone without a manager, so they may set their own schedule. */
  protected readonly canManageOwnSchedule = computed(() => this.authService.isSuperadmin() || this.myUser()?.managerId == null);

  private readonly schedulesByUserId = computed(() => {
    const map = new Map<number, WorkSchedule[]>();
    for (const schedule of this.schedules()) {
      const userId = Number(schedule.userId);
      const list = map.get(userId) ?? [];
      list.push(schedule);
      map.set(userId, list);
    }
    for (const list of map.values()) {
      list.sort((a, b) => new Date(b.effectiveFrom!).getTime() - new Date(a.effectiveFrom!).getTime());
    }
    return map;
  });

  ngOnInit(): void {
    this.load();
  }

  protected schedulesFor(user: User): WorkSchedule[] {
    return this.schedulesByUserId().get(Number(user.id)) ?? [];
  }

  protected currentScheduleFor(user: User): WorkSchedule | null {
    return this.schedulesFor(user).find(isCurrentSchedule) ?? null;
  }

  protected readonly weeklyTotal = weeklyTotal;

  protected dayValue(schedule: WorkSchedule, column: DayColumn): number {
    return Number(schedule[column.key] ?? 0);
  }

  protected workingDaysCount(schedule: WorkSchedule): number {
    return this.dayColumns.filter(col => this.dayValue(schedule, col) > 0).length;
  }

  protected isToday(column: DayColumn): boolean {
    return new Date().getDay() === column.dayIndex;
  }

  protected dayTileClasses(schedule: WorkSchedule, column: DayColumn): string {
    const base = 'flex flex-col items-center gap-1 rounded-lg border py-3';
    const fill = this.dayValue(schedule, column) > 0 ? 'border-border bg-muted/30' : 'border-dashed border-border/60';
    const today = this.isToday(column) ? 'ring-2 ring-primary/50 ring-offset-2 ring-offset-background' : '';
    return `${base} ${fill} ${today}`.trim();
  }

  protected manage(user: User, own = false): void {
    this.dialogService.create({
      zTitle: `Arbeitszeit — ${user.firstName} ${user.lastName}`,
      zContent: ManageWorkScheduleDialog,
      zHideFooter: true,
      zWidth: '30rem',
      zData: { user, schedules: own ? this.mySchedules() : this.schedulesFor(user), own, onSuccess: () => this.load() },
    });
  }

  private async load(): Promise<void> {
    this.loading.set(true);

    try {
      const [managedUsers, schedules, mySchedules, myUser] = await Promise.all([
        this.userService.apiUserManagedGet$Json(),
        this.workScheduleService.apiWorkScheduleManagedGet$Json(),
        this.workScheduleService.apiWorkScheduleMineGet$Json(),
        this.userService.apiUserIdGet$Json({ id: this.authService.currentUser()!.userId }),
      ]);
      this.managedUsers.set(managedUsers);
      this.schedules.set(schedules);
      this.mySchedules.set(mySchedules);
      this.myUser.set(myUser);
    } catch {
      this.error.set('Arbeitszeiten konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
