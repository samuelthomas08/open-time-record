import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Team, TeamService, User, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardTableImports } from '@/shared/components/table';
import { CreateTeamDialog } from './create-team-dialog/create-team-dialog';

/** Lists teams and lets authorized users create and (de)activate them. */
@Component({
  selector: 'app-teams',
  imports: [FormsModule, ZardBadgeComponent, ZardButtonComponent, ZardInputComponent, ZardTableImports],
  templateUrl: './teams.html',
})
export class Teams implements OnInit {
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly authService = inject(AuthService);
  protected readonly teams = signal<Team[]>([]);
  protected readonly users = signal<User[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal<string>('');

  protected readonly filteredTeams = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.teams();
    }
    return this.teams().filter(
      team => (team.teamDisplayName ?? '').toLowerCase().includes(term) || (team.teamName ?? '').toLowerCase().includes(term),
    );
  });

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    this.load();
    this.userService.apiUserGet$Json().then(users => this.users.set(users));
  }

  /** Opens the dialog for creating a new entry and reloads the list on success. */
  protected openCreateDialog(): void {
    this.dialogService.create({
      zTitle: 'Team anlegen',
      zContent: CreateTeamDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { onSuccess: () => this.load() },
    });
  }

  /** Flips the active flag and reloads the list. */
  protected async toggleActive(team: Team): Promise<void> {
    await this.teamService.apiTeamIdPut$Json({ id: team.id!, body: { ...team, isActive: !team.isActive } });
    await this.load();
  }

  /** Resolves a manager id to a display name, or an em dash if unknown. */
  protected managerName(managerId: Team['managerId']): string {
    const manager = this.users().find(u => Number(u.id) === Number(managerId));
    return manager ? `${manager.firstName} ${manager.lastName}` : '—';
  }

  /** Fetches the data shown on this page and updates loading/error state. */
  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      this.teams.set(await this.teamService.apiTeamGet$Json());
    } catch {
      this.error.set('Teams konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
