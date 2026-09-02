import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Role, RoleService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardSelectImports } from '@/shared/components/select';
import { ZardTableImports } from '@/shared/components/table';
import { CreateRoleDialog } from './create-role-dialog/create-role-dialog';

/** Mirrors the backend PermissionResource enum — values are its declaration order, serialized as plain numbers. */
const PERMISSION_RESOURCES: ReadonlyArray<{ value: number; label: string }> = [
  { value: 0, label: 'Zeiteinträge' },
  { value: 1, label: 'Projekte' },
  { value: 2, label: 'Nutzer' },
  { value: 3, label: 'Regeln' },
  { value: 4, label: 'Rollen' },
  { value: 5, label: 'Teams' },
  { value: 6, label: 'Korrekturanträge' },
  { value: 7, label: 'Einladungen' },
  { value: 8, label: 'SMTP-Einstellungen' },
  { value: 9, label: 'Systemeinstellungen' },
  { value: 10, label: 'Kunden' },
  { value: 11, label: 'Projektaufgaben' },
  { value: 12, label: 'Tags' },
  { value: 13, label: 'Arbeitszeitpläne' },
  { value: 14, label: 'Urlaubsarten' },
  { value: 15, label: 'Urlaubsanträge' },
  { value: 16, label: 'Urlaubskonten' },
  { value: 17, label: 'Feiertage' },
  { value: 18, label: 'Benachrichtigungen' },
  { value: 19, label: 'Änderungsprotokoll' },
  { value: 20, label: 'Profilbild' },
];

/** Mirrors the backend PermissionLevel enum, same reasoning. */
const PERMISSION_LEVELS: ReadonlyArray<{ value: number; label: string }> = [
  { value: 0, label: 'Keine' },
  { value: 1, label: 'Lesen' },
  { value: 2, label: 'Bearbeiten' },
  { value: 3, label: 'Admin' },
];

@Component({
  selector: 'app-roles',
  imports: [FormsModule, ZardBadgeComponent, ZardButtonComponent, ZardInputComponent, ZardSelectImports, ZardTableImports],
  templateUrl: './roles.html',
})
export class Roles implements OnInit {
  private readonly roleService = inject(RoleService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly authService = inject(AuthService);
  protected readonly roles = signal<Role[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal('');

  protected readonly filteredRoles = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.roles();
    }
    return this.roles().filter(
      role => (role.roleDisplayName ?? '').toLowerCase().includes(term) || (role.roleName ?? '').toLowerCase().includes(term),
    );
  });

  protected readonly PERMISSION_RESOURCES = PERMISSION_RESOURCES;
  protected readonly PERMISSION_LEVELS = PERMISSION_LEVELS;

  protected readonly expandedRoleId = signal<number | null>(null);
  protected readonly permissionsLoading = signal(false);
  protected readonly savingPermissions = signal(false);
  protected permissionLevels: Record<number, string> = {};

  ngOnInit(): void {
    this.load();
  }

  protected openCreateDialog(): void {
    this.dialogService.create({
      zTitle: 'Rolle anlegen',
      zContent: CreateRoleDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { onSuccess: () => this.load() },
    });
  }

  protected async toggleActive(role: Role): Promise<void> {
    await this.roleService.apiRoleIdPut$Json({ id: role.id!, body: { ...role, isActive: !role.isActive } });
    await this.load();
  }

  protected async toggleExpand(role: Role): Promise<void> {
    const id = Number(role.id);

    if (this.expandedRoleId() === id) {
      this.expandedRoleId.set(null);
      return;
    }

    this.expandedRoleId.set(id);
    await this.loadPermissions(id);
  }

  protected async savePermissions(role: Role): Promise<void> {
    this.savingPermissions.set(true);
    this.error.set(null);

    try {
      const body = PERMISSION_RESOURCES.map(resource => ({
        resource: resource.value,
        level: Number(this.permissionLevels[resource.value] ?? 0),
      }));
      await this.roleService.apiRoleIdPermissionsPut$Json({ id: role.id!, body });
    } catch {
      this.error.set('Berechtigungen konnten nicht gespeichert werden.');
    } finally {
      this.savingPermissions.set(false);
    }
  }

  private async loadPermissions(roleId: number): Promise<void> {
    this.permissionsLoading.set(true);
    this.permissionLevels = Object.fromEntries(PERMISSION_RESOURCES.map(r => [r.value, '0']));

    try {
      const permissions = await this.roleService.apiRoleIdPermissionsGet$Json({ id: roleId });
      for (const permission of permissions) {
        if (permission.resource != null && permission.level != null) {
          this.permissionLevels[Number(permission.resource)] = String(permission.level);
        }
      }
    } catch {
      this.error.set('Berechtigungen konnten nicht geladen werden.');
    } finally {
      this.permissionsLoading.set(false);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      this.roles.set(await this.roleService.apiRoleGet$Json());
    } catch {
      this.error.set('Rollen konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
