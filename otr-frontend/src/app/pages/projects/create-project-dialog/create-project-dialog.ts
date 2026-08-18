import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ProjectService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';

export interface CreateProjectDialogData {
  onSuccess: () => void;
}

@Component({
  selector: 'app-create-project-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './create-project-dialog.html',
})
export class CreateProjectDialog {
  private readonly projectService = inject(ProjectService);
  private readonly dialogRef = inject(ZardDialogRef<CreateProjectDialog>);
  private readonly data = injectDialogData<CreateProjectDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected projectName = '';
  protected projectDisplayName = '';

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.projectService.apiProjectPost$Json({
        body: {
          projectName: this.projectName,
          projectDisplayName: this.projectDisplayName,
          isActive: true,
          createdAt: new Date().toISOString(),
        },
      });
      this.data.onSuccess();
      this.dialogRef.close();
    } catch {
      this.error.set('Projekt konnte nicht angelegt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
