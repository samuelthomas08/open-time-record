import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { RoleService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';

export interface CreateRoleDialogData {
  onSuccess: () => void;
}

@Component({
  selector: 'app-create-role-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './create-role-dialog.html',
})
export class CreateRoleDialog {
  private readonly roleService = inject(RoleService);
  private readonly dialogRef = inject(ZardDialogRef<CreateRoleDialog>);
  private readonly data = injectDialogData<CreateRoleDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected roleName = '';
  protected roleDisplayName = '';

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.roleService.apiRolePost$Json({
        body: { roleName: this.roleName, roleDisplayName: this.roleDisplayName, isActive: true, createdAt: new Date().toISOString() },
      });
      this.data.onSuccess();
      this.dialogRef.close();
    } catch {
      this.error.set('Rolle konnte nicht angelegt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
