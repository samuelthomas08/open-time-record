import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { Role, RoleService, Team, TeamService, User, UserInvitationService, UserService } from '@/api-client';
import { ZardButtonComponent } from '@/shared/components/button';
import { injectDialogData, ZardDialogRef } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardSelectImports } from '@/shared/components/select';
import { ZardSwitchComponent } from '@/shared/components/switch';

export interface InviteUserDialogData {
  onSuccess: () => void;
}

@Component({
  selector: 'app-invite-user-dialog',
  imports: [FormsModule, ZardButtonComponent, ZardFieldImports, ZardInputComponent, ZardSelectImports, ZardSwitchComponent],
  templateUrl: './invite-user-dialog.html',
})
export class InviteUserDialog implements OnInit {
  private readonly userInvitationService = inject(UserInvitationService);
  private readonly roleService = inject(RoleService);
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly dialogRef = inject(ZardDialogRef<InviteUserDialog>);
  private readonly data = injectDialogData<InviteUserDialogData>();

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly roles = signal<Role[]>([]);
  protected readonly teams = signal<Team[]>([]);
  protected readonly users = signal<User[]>([]);
  protected readonly linkResult = signal<string | null>(null);
  protected readonly copied = signal(false);

  protected email = '';
  protected roleId = '';
  protected teamId = '';
  protected managerId = '';
  protected sendEmail = true;

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

  protected setSendEmail(value: boolean): void {
    this.sendEmail = value;
  }

  protected async submit(): Promise<void> {
    if (!this.roleId) {
      this.error.set('Bitte eine Rolle auswählen.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    try {
      const response = await this.userInvitationService.apiUserInvitationPost$Json({
        body: {
          email: this.email,
          roleId: Number(this.roleId),
          teamId: this.teamId ? Number(this.teamId) : null,
          managerId: this.managerId ? Number(this.managerId) : null,
          sendEmail: this.sendEmail,
        },
      });

      this.data.onSuccess();

      if (!this.sendEmail && response.invitationCode) {
        const link = new URL('/register', window.location.origin);
        link.searchParams.set('email', this.email);
        link.searchParams.set('code', response.invitationCode);
        this.linkResult.set(link.toString());
      } else {
        this.dialogRef.close();
      }
    } catch (err) {
      const backendMessage = err instanceof HttpErrorResponse && typeof err.error === 'string' ? err.error : null;
      this.error.set(backendMessage ?? 'Einladung konnte nicht versendet werden.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected async copyLink(link: string): Promise<void> {
    await navigator.clipboard.writeText(link);
    this.copied.set(true);
    setTimeout(() => this.copied.set(false), 2000);
  }

  protected close(): void {
    this.dialogRef.close();
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
