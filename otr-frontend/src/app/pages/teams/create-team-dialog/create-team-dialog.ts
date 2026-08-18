import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TeamService, User, UserService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardSelectImports } from '@/shared/components/select';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';

export interface CreateTeamDialogData {
  onSuccess: () => void;
}

@Component({
  selector: 'app-create-team-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent, ZardSelectImports],
  templateUrl: './create-team-dialog.html',
})
export class CreateTeamDialog implements OnInit {
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly dialogRef = inject(ZardDialogRef<CreateTeamDialog>);
  private readonly data = injectDialogData<CreateTeamDialogData>();

  protected readonly users = signal<User[]>([]);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected teamName = '';
  protected teamDisplayName = '';
  protected managerId = '';

  ngOnInit(): void {
    this.userService.apiUserGet$Json().then(users => this.users.set(users));
  }

  protected async submit(): Promise<void> {
    if (!this.managerId) {
      this.error.set('Bitte einen Vorgesetzten auswählen.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.teamService.apiTeamPost$Json({
        body: {
          teamName: this.teamName,
          teamDisplayName: this.teamDisplayName,
          managerId: Number(this.managerId),
          isActive: true,
          createdAt: new Date().toISOString(),
        },
      });
      this.data.onSuccess();
      this.dialogRef.close();
    } catch {
      this.error.set('Team konnte nicht angelegt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
