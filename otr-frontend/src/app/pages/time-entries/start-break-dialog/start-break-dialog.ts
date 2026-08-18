import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TimerService } from '@/services/timer.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

export interface StartBreakDialogData {
  reasonRequired: boolean;
}

@Component({
  selector: 'app-start-break-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './start-break-dialog.html',
})
export class StartBreakDialog {
  private readonly timerService = inject(TimerService);
  private readonly dialogRef = inject(ZardDialogRef<StartBreakDialog>);

  protected readonly data = injectDialogData<StartBreakDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected reason = '';

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.timerService.startBreak(this.reason);
      this.dialogRef.close();
    } catch {
      this.error.set('Pause konnte nicht gestartet werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
