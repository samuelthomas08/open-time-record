import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { Role, RoleService, Team, TeamService, User, UserService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardSelectImports } from '@/shared/components/select';

export interface CreateUserDialogData {
  onSuccess: () => void;
}

@Component({
  selector: 'app-create-user-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent, ZardSelectImports],
  templateUrl: './create-user-dialog.html',
})
export class CreateUserDialog implements OnInit {
  private readonly userService = inject(UserService);
  private readonly roleService = inject(RoleService);
  private readonly teamService = inject(TeamService);
  private readonly dialogRef = inject(ZardDialogRef<CreateUserDialog>);
  private readonly data = injectDialogData<CreateUserDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly roles = signal<Role[]>([]);
  protected readonly teams = signal<Team[]>([]);
  protected readonly users = signal<User[]>([]);

  protected firstName = '';
  protected lastName = '';
  protected email = '';
  protected password = '';
  protected roleId = '';
  protected teamId = '';
  protected managerId = '';

  ngOnInit(): void {
    this.roleService.apiRoleGet$Json().then(roles => this.roles.set(roles.filter(r => r.isActive)));
    this.teamService.apiTeamGet$Json().then(teams => this.teams.set(teams.filter(t => t.isActive)));
    this.userService.apiUserGet$Json().then(users => this.users.set(users));
  }

  /** Pre-fills the manager with the chosen team's own manager — just a suggestion, still editable. */
  protected onTeamChange(teamId: string): void {
    if (!teamId) {
      return;
    }

    const team = this.teams().find(t => String(t.id) === teamId);
    if (team?.managerId != null) {
      this.managerId = String(team.managerId);
    }
  }

  protected async submit(): Promise<void> {
    if (!this.roleId) {
      this.error.set('Bitte eine Rolle auswählen.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.userService.apiUserPost$Json({
        body: {
          firstName: this.firstName,
          lastName: this.lastName,
          email: this.email,
          password: this.password,
          roleId: Number(this.roleId),
          teamId: this.teamId ? Number(this.teamId) : null,
          managerId: this.managerId ? Number(this.managerId) : null,
        },
      });
      this.data.onSuccess();
      this.dialogRef.close();
    } catch (err) {
      const backendMessage = err instanceof HttpErrorResponse && typeof err.error === 'string' ? err.error : null;
      this.error.set(backendMessage ?? 'Nutzer konnte nicht angelegt werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
