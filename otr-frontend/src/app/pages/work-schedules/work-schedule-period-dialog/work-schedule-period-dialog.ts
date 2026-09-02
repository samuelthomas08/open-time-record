import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { WorkSchedule, WorkScheduleService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDatePickerComponent } from '@/shared/components/date-picker';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

export interface WorkSchedulePeriodDialogData {
  userId: number;
  existing?: WorkSchedule;
  onSuccess: () => void;
}

const MAX_HOURS_PER_DAY = 24;

interface DayField {
  key: 'mondayHours' | 'tuesdayHours' | 'wednesdayHours' | 'thursdayHours' | 'fridayHours' | 'saturdayHours' | 'sundayHours';
  label: string;
  defaultHours: number;
}

const DAY_FIELDS: DayField[] = [
  { key: 'mondayHours', label: 'Mo', defaultHours: 8 },
  { key: 'tuesdayHours', label: 'Di', defaultHours: 8 },
  { key: 'wednesdayHours', label: 'Mi', defaultHours: 8 },
  { key: 'thursdayHours', label: 'Do', defaultHours: 8 },
  { key: 'fridayHours', label: 'Fr', defaultHours: 8 },
  { key: 'saturdayHours', label: 'Sa', defaultHours: 0 },
  { key: 'sundayHours', label: 'So', defaultHours: 0 },
];

@Component({
  selector: 'app-work-schedule-period-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardDatePickerComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './work-schedule-period-dialog.html',
})
export class WorkSchedulePeriodDialog {
  private readonly workScheduleService = inject(WorkScheduleService);
  private readonly dialogRef = inject(ZardDialogRef<WorkSchedulePeriodDialog>);
  private readonly data = injectDialogData<WorkSchedulePeriodDialogData>();

  protected readonly dayFields = DAY_FIELDS;
  protected readonly maxHoursPerDay = MAX_HOURS_PER_DAY;
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly isEditing = this.data.existing != null;

  protected mondayHours = 0;
  protected tuesdayHours = 0;
  protected wednesdayHours = 0;
  protected thursdayHours = 0;
  protected fridayHours = 0;
  protected saturdayHours = 0;
  protected sundayHours = 0;
  protected effectiveFrom: Date | null = new Date();
  protected effectiveTo: Date | null = null;

  constructor() {
    const existing = this.data.existing;
    if (existing) {
      for (const field of this.dayFields) {
        this.setDayValue(field, Number(existing[field.key] ?? 0));
      }
      this.effectiveFrom = new Date(existing.effectiveFrom!);
      this.effectiveTo = existing.effectiveTo ? new Date(existing.effectiveTo) : null;
    } else {
      for (const field of this.dayFields) {
        this.setDayValue(field, field.defaultHours);
      }
    }
  }

  protected dayValue(field: DayField): number {
    return this[field.key];
  }

  protected setDayValue(field: DayField, value: number): void {
    this[field.key] = Math.min(MAX_HOURS_PER_DAY, Math.max(0, value || 0));
  }

  protected formTotal(): number {
    return this.dayFields.reduce((sum, field) => sum + (this.dayValue(field) || 0), 0);
  }

  protected async submit(): Promise<void> {
    if (!this.effectiveFrom) {
      this.error.set('Bitte ein Startdatum angeben.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const body = {
      userId: this.data.userId,
      mondayHours: this.mondayHours,
      tuesdayHours: this.tuesdayHours,
      wednesdayHours: this.wednesdayHours,
      thursdayHours: this.thursdayHours,
      fridayHours: this.fridayHours,
      saturdayHours: this.saturdayHours,
      sundayHours: this.sundayHours,
      effectiveFrom: this.effectiveFrom.toISOString(),
      effectiveTo: this.effectiveTo ? this.effectiveTo.toISOString() : null,
    };

    try {
      const existing = this.data.existing;
      if (existing) {
        await this.workScheduleService.apiWorkScheduleIdPut$Json({ id: Number(existing.id), body });
      } else {
        await this.workScheduleService.apiWorkSchedulePost$Json({ body });
      }
      this.data.onSuccess();
      this.dialogRef.close();
    } catch {
      this.error.set('Der Zeitraum konnte nicht gespeichert werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
