import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TimeEntry, TimeEntryCorrectionRequestService, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDatePickerComponent } from '@/shared/components/date-picker';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

export interface RequestCorrectionDialogData {
  timeEntry: TimeEntry;
}

@Component({
  selector: 'app-request-correction-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardDatePickerComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './request-correction-dialog.html',
})
export class RequestCorrectionDialog implements OnInit {
  private readonly correctionRequestService = inject(TimeEntryCorrectionRequestService);
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly dialogRef = inject(ZardDialogRef<RequestCorrectionDialog>);
  private readonly data = injectDialogData<RequestCorrectionDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly loadingManagers = signal(true);
  protected readonly managerNames = signal<string[]>([]);

  protected reason = '';
  protected proposedStartDate: Date | null = null;
  protected proposedStartTimeOfDay = '';
  protected proposedEndDate: Date | null = null;
  protected proposedEndTimeOfDay = '';

  constructor() {
    const entry = this.data.timeEntry;

    if (entry.startTime) {
      [this.proposedStartDate, this.proposedStartTimeOfDay] = this.splitIsoIntoDateAndTime(entry.startTime);
    }
    if (entry.endTime) {
      [this.proposedEndDate, this.proposedEndTimeOfDay] = this.splitIsoIntoDateAndTime(entry.endTime);
    }
  }

  ngOnInit(): void {
    this.loadManagers();
  }

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.correctionRequestService.apiTimeEntryCorrectionRequestPost$Json({
        body: {
          timeEntryId: this.data.timeEntry.id,
          reason: this.reason,
          proposedStartTime: this.combineDateAndTime(this.proposedStartDate, this.proposedStartTimeOfDay),
          proposedEndTime: this.combineDateAndTime(this.proposedEndDate, this.proposedEndTimeOfDay),
        },
      });
      this.dialogRef.close();
    } catch {
      this.error.set('Korrekturantrag konnte nicht erstellt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  /** Managers of the current user's team(s) — informational only; approval itself is Superadmin-gated. */
  private async loadManagers(): Promise<void> {
    const userId = this.authService.currentUser()?.userId;
    if (!userId) {
      this.loadingManagers.set(false);
      return;
    }

    try {
      const [teams, users] = await Promise.all([
        this.userService.apiUserIdTeamsGet$Json({ id: userId }),
        this.userService.apiUserGet$Json(),
      ]);

      const namesByManagerId = new Map<number, string>();
      for (const team of teams) {
        const manager = users.find(u => Number(u.id) === Number(team.managerId));
        if (manager) {
          namesByManagerId.set(Number(manager.id), `${manager.firstName} ${manager.lastName}`);
        }
      }
      this.managerNames.set([...namesByManagerId.values()]);
    } catch {
      this.managerNames.set([]);
    } finally {
      this.loadingManagers.set(false);
    }
  }

  /** Combines a calendar date with an `hh:mm` time-of-day into an ISO string, in local time. */
  private combineDateAndTime(date: Date | null, timeOfDay: string): string | null {
    if (!date) {
      return null;
    }

    const [hours, minutes] = timeOfDay ? timeOfDay.split(':').map(Number) : [0, 0];
    const combined = new Date(date);
    combined.setHours(hours || 0, minutes || 0, 0, 0);
    return combined.toISOString();
  }

  private splitIsoIntoDateAndTime(iso: string): [Date, string] {
    const date = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return [date, `${pad(date.getHours())}:${pad(date.getMinutes())}`];
  }
}
