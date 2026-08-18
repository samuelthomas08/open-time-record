import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Role, RoleService, Team, TeamService, User, UserInvitation, UserInvitationService, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardSelectImports } from '@/shared/components/select';
import { ZardTableImports } from '@/shared/components/table';
import { CreateUserDialog } from './create-user-dialog/create-user-dialog';
import { InviteUserDialog } from './invite-user-dialog/invite-user-dialog';

@Component({
  selector: 'app-users',
  imports: [DatePipe, FormsModule, ZardBadgeComponent, ZardButtonComponent, ZardSelectImports, ZardTableImports],
  templateUrl: './users.html',
})
export class Users implements OnInit {
  private readonly userService = inject(UserService);
  private readonly roleService = inject(RoleService);
  private readonly teamService = inject(TeamService);
  private readonly userInvitationService = inject(UserInvitationService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly authService = inject(AuthService);

  protected readonly users = signal<User[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly invitations = signal<UserInvitation[]>([]);

  protected readonly allRoles = signal<Role[]>([]);
  protected readonly allTeams = signal<Team[]>([]);

  protected readonly expandedUserId = signal<number | null>(null);
  protected readonly panelLoading = signal(false);
  protected readonly userRoles = signal<Role[]>([]);
  protected readonly userTeams = signal<Team[]>([]);

  protected selectedRoleId = '';
  protected selectedTeamId = '';

  protected readonly availableRoles = computed(() =>
    this.allRoles().filter(role => !this.userRoles().some(assigned => assigned.id === role.id)),
  );
  protected readonly availableTeams = computed(() =>
    this.allTeams().filter(team => !this.userTeams().some(assigned => assigned.id === team.id)),
  );

  ngOnInit(): void {
    this.load();
    this.roleService.apiRoleGet$Json().then(roles => this.allRoles.set(roles));
    this.teamService.apiTeamGet$Json().then(teams => this.allTeams.set(teams));

    if (this.authService.isSuperadmin()) {
      this.loadInvitations();
    }
  }

  protected openInviteDialog(): void {
    this.dialogService.create({
      zTitle: 'Nutzer einladen',
      zContent: InviteUserDialog,
      zHideFooter: true,
      zWidth: '26rem',
      zData: { onSuccess: () => this.loadInvitations() },
    });
  }

  protected openCreateUserDialog(): void {
    this.dialogService.create({
      zTitle: 'Nutzer anlegen',
      zContent: CreateUserDialog,
      zHideFooter: true,
      zWidth: '26rem',
      zData: { onSuccess: () => this.load() },
    });
  }

  protected async toggleActive(user: User): Promise<void> {
    const updated: User = { ...user, isActive: !user.isActive };
    await this.userService.apiUserIdPut$Json({ id: user.id!, body: updated });
    this.load();
  }

  protected async toggleExpand(user: User): Promise<void> {
    const id = Number(user.id);

    if (this.expandedUserId() === id) {
      this.expandedUserId.set(null);
      return;
    }

    this.expandedUserId.set(id);
    await this.loadPanel(id);
  }

  protected async addRole(userId: number): Promise<void> {
    if (!this.selectedRoleId) {
      return;
    }

    await this.userService.apiUserIdRolesRoleIdPost({ id: userId, roleId: Number(this.selectedRoleId) });
    this.selectedRoleId = '';
    await this.loadPanel(userId);
  }

  protected async removeRole(userId: number, role: Role): Promise<void> {
    await this.userService.apiUserIdRolesRoleIdDelete({ id: userId, roleId: role.id! });
    await this.loadPanel(userId);
  }

  protected async addTeam(userId: number): Promise<void> {
    if (!this.selectedTeamId) {
      return;
    }

    await this.userService.apiUserIdTeamsTeamIdPost({ id: userId, teamId: Number(this.selectedTeamId) });
    this.selectedTeamId = '';
    await this.loadPanel(userId);
  }

  protected async removeTeam(userId: number, team: Team): Promise<void> {
    await this.userService.apiUserIdTeamsTeamIdDelete({ id: userId, teamId: team.id! });
    await this.loadPanel(userId);
  }

  private async loadPanel(userId: number): Promise<void> {
    this.panelLoading.set(true);

    try {
      const enriched = await this.userService.apiUserIdEnrichedGet$Json({ id: userId });
      this.userRoles.set(enriched.roles ?? []);
      this.userTeams.set(enriched.teams ?? []);
    } catch {
      this.error.set('Rollen/Teams konnten nicht geladen werden.');
    } finally {
      this.panelLoading.set(false);
    }
  }

  private async loadInvitations(): Promise<void> {
    try {
      this.invitations.set(await this.userInvitationService.apiUserInvitationGet$Json());
    } catch {
      this.error.set('Einladungen konnten nicht geladen werden.');
    }
  }

  private load(): void {
    this.loading.set(true);
    this.userService.apiUserGet$Json().then(
      users => {
        this.users.set(users);
        this.loading.set(false);
      },
      () => {
        this.error.set('Nutzer konnten nicht geladen werden.');
        this.loading.set(false);
      },
    );
  }
}
