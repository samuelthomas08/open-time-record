import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Project, ProjectService } from '@/api-client';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardTableImports } from '@/shared/components/table';
import { CreateProjectDialog } from './create-project-dialog/create-project-dialog';

/** Lists projects and lets authorized users create and (de)activate them. */
@Component({
  selector: 'app-projects',
  imports: [FormsModule, ZardBadgeComponent, ZardButtonComponent, ZardInputComponent, ZardTableImports],
  templateUrl: './projects.html',
})
export class Projects implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly projects = signal<Project[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal<string>('');

  protected readonly filteredProjects = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.projects();
    }
    return this.projects().filter(
      project =>
        (project.projectDisplayName ?? '').toLowerCase().includes(term) || (project.projectName ?? '').toLowerCase().includes(term),
    );
  });

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    this.load();
  }

  /** Opens the dialog for creating a new entry and reloads the list on success. */
  protected openCreateDialog(): void {
    this.dialogService.create({
      zTitle: 'Projekt anlegen',
      zContent: CreateProjectDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { onSuccess: () => this.load() },
    });
  }

  /** Flips the active flag and reloads the list. */
  protected async toggleActive(project: Project): Promise<void> {
    await this.projectService.apiProjectIdPut$Json({ id: project.id!, body: { ...project, isActive: !project.isActive } });
    await this.load();
  }

  /** Fetches the data shown on this page and updates loading/error state. */
  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const projects = await this.projectService.apiProjectGet$Json();
      this.projects.set(projects);
    } catch {
      this.error.set('Projekte konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
