import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CorrectionRequestDto, TimeEntryCorrectionRequestService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

export interface RejectCorrectionDialogData {
  request: CorrectionRequestDto;
  onSuccess: () => void;
}

@Component({
  selector: 'app-reject-correction-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './reject-correction-dialog.html',
})
export class RejectCorrectionDialog {
  private readonly correctionRequestService = inject(TimeEntryCorrectionRequestService);
  private readonly dialogRef = inject(ZardDialogRef<RejectCorrectionDialog>);
  protected readonly data = injectDialogData<RejectCorrectionDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected note = '';

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.correctionRequestService.apiTimeEntryCorrectionRequestIdRejectPut$Json({
        id: this.data.request.id!,
        body: { note: this.note.trim() || null },
      });
      this.data.onSuccess();
      this.dialogRef.close();
    } catch {
      this.error.set('Antrag konnte nicht abgelehnt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
