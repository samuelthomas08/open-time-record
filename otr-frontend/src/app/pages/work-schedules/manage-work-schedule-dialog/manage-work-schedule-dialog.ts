import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { User, WorkSchedule, WorkScheduleService } from '@/api-client';
import { weeklyTotal } from '@/pages/work-schedules/work-schedules';
import { WorkSchedulePeriodDialog } from '@/pages/work-schedules/work-schedule-period-dialog/work-schedule-period-dialog';
import { ZardButtonComponent } from '@/shared/components/button';
import { injectDialogData, ZardDialogRef, ZardDialogService } from '@/shared/components/dialog';
import { ZardTableImports } from '@/shared/components/table';

export interface ManageWorkScheduleDialogData {
  user: User;
  schedules: WorkSchedule[];
  /** True when managing the caller's own schedule (only possible when they have no manager). */
  own?: boolean;
  onSuccess: () => void;
}

@Component({
  selector: 'app-manage-work-schedule-dialog',
  imports: [DatePipe, ZardButtonComponent, ZardTableImports],
  templateUrl: './manage-work-schedule-dialog.html',
})
export class ManageWorkScheduleDialog {
  private readonly workScheduleService = inject(WorkScheduleService);
  private readonly dialogRef = inject(ZardDialogRef<ManageWorkScheduleDialog>);
  private readonly dialogService = inject(ZardDialogService);
  private readonly data = injectDialogData<ManageWorkScheduleDialogData>();

  protected readonly schedules = signal<WorkSchedule[]>(this.data.schedules);
  protected readonly error = signal<string | null>(null);
  protected readonly weeklyTotal = weeklyTotal;

  protected openPeriodDialog(existing?: WorkSchedule): void {
    this.dialogService.create({
      zTitle: existing ? 'Zeitraum bearbeiten' : 'Neuer Zeitraum',
      zContent: WorkSchedulePeriodDialog,
      zHideFooter: true,
      zWidth: '40rem',
      zData: { userId: Number(this.data.user.id), existing, onSuccess: () => this.reload() },
    });
  }

  protected async remove(schedule: WorkSchedule): Promise<void> {
    try {
      await this.workScheduleService.apiWorkScheduleIdDelete({ id: Number(schedule.id) });
      await this.reload();
    } catch {
      this.error.set('Der Zeitraum konnte nicht gelöscht werden.');
    }
  }

  protected close(): void {
    this.dialogRef.close();
  }

  private async reload(): Promise<void> {
    // /managed never includes the caller's own schedules, so self-management has to reload via /mine.
    const all = this.data.own
      ? await this.workScheduleService.apiWorkScheduleMineGet$Json()
      : await this.workScheduleService.apiWorkScheduleManagedGet$Json();
    const userId = Number(this.data.user.id);
    this.schedules.set(
      all.filter(s => Number(s.userId) === userId).sort((a, b) => new Date(b.effectiveFrom!).getTime() - new Date(a.effectiveFrom!).getTime()),
    );
    this.data.onSuccess();
  }
}
