import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Project, ProjectService } from '@/api-client';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardTableImports } from '@/shared/components/table';
import { CreateProjectDialog } from './create-project-dialog/create-project-dialog';

@Component({
  selector: 'app-projects',
  imports: [FormsModule, ZardBadgeComponent, ZardButtonComponent, ZardInputComponent, ZardTableImports],
  templateUrl: './projects.html',
})
export class Projects implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly projects = signal<Project[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal('');

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

  ngOnInit(): void {
    this.load();
  }

  protected openCreateDialog(): void {
    this.dialogService.create({
      zTitle: 'Projekt anlegen',
      zContent: CreateProjectDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { onSuccess: () => this.load() },
    });
  }

  protected async toggleActive(project: Project): Promise<void> {
    await this.projectService.apiProjectIdPut$Json({ id: project.id!, body: { ...project, isActive: !project.isActive } });
    await this.load();
  }

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
