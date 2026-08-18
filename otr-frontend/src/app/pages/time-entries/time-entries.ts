import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideCoffee } from '@ng-icons/lucide';

import { AppSettingsService, Project, ProjectService, TimeEntry, TimeEntryService } from '@/api-client';
import { TimerService } from '@/services/timer.service';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';
import { ZardSelectImports } from '@/shared/components/select';
import { ZardTableImports } from '@/shared/components/table';
import { RequestCorrectionDialog } from './request-correction-dialog/request-correction-dialog';
import { StartBreakDialog } from './start-break-dialog/start-break-dialog';

const RUNNING = 0;

@Component({
  selector: 'app-time-entries',
  imports: [
    DatePipe,
    FormsModule,
    NgIcon,
    ZardBadgeComponent,
    ZardButtonComponent,
    ZardFieldImports,
    ZardInputComponent,
    ZardSelectImports,
    ZardTableImports,
  ],
  templateUrl: './time-entries.html',
  viewProviders: [provideIcons({ lucideCoffee })],
})
export class TimeEntries implements OnInit {
  private readonly timeEntryService = inject(TimeEntryService);
  private readonly projectService = inject(ProjectService);
  private readonly appSettingsService = inject(AppSettingsService);
  private readonly dialogService = inject(ZardDialogService);

  protected readonly timerService = inject(TimerService);
  protected readonly RUNNING = RUNNING;

  protected readonly timeEntries = signal<TimeEntry[]>([]);
  protected readonly projects = signal<Project[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly actionPending = signal(false);
  protected readonly editingId = signal<number | null>(null);
  protected readonly breakReasonRequired = signal(false);

  protected runningProjectId = '';
  protected runningDescription = '';
  protected editProjectId = '';
  protected editDescription = '';

  ngOnInit(): void {
    this.timerService.ensureLoaded().then(() => this.syncRunningForm());
    this.load();
    this.loadProjects();
    this.loadBreakSettings();
  }

  protected async start(): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await this.timerService.start();
      this.syncRunningForm();
      await this.load();
    } catch {
      this.error.set('Zeiterfassung konnte nicht gestartet werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  protected async stop(): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await this.timerService.stop();
      await this.load();
    } catch {
      this.error.set('Zeiterfassung konnte nicht gestoppt werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  protected async saveRunning(): Promise<void> {
    const running = this.timerService.runningEntry();
    if (!running?.id) {
      return;
    }

    await this.updateEntry(Number(running.id), this.runningProjectId, this.runningDescription);
    await this.timerService.refresh();
  }

  protected startEdit(entry: TimeEntry): void {
    this.editingId.set(Number(entry.id));
    this.editProjectId = entry.projectId != null ? String(entry.projectId) : '';
    this.editDescription = entry.description ?? '';
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected async saveEdit(entry: TimeEntry): Promise<void> {
    await this.updateEntry(Number(entry.id), this.editProjectId, this.editDescription);
    this.editingId.set(null);

    if (entry.status === RUNNING) {
      await this.timerService.refresh();
    }
  }

  protected async deleteEntry(entry: TimeEntry): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await this.timeEntryService.apiTimeEntryIdDelete({ id: entry.id! });
      await this.load();

      if (entry.status === RUNNING) {
        await this.timerService.refresh();
      }
    } catch {
      this.error.set('Eintrag konnte nicht gelöscht werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  protected openStartBreakDialog(): void {
    this.dialogService.create({
      zTitle: 'Pause starten',
      zContent: StartBreakDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { reasonRequired: this.breakReasonRequired() },
    });
  }

  protected async stopBreak(): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await this.timerService.stopBreak();
    } catch {
      this.error.set('Pause konnte nicht beendet werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  /** Net worked duration for a completed entry — gross time minus every completed break. */
  protected duration(entry: TimeEntry): string {
    if (!entry.startTime || !entry.endTime) {
      return '—';
    }

    const grossMs = new Date(entry.endTime).getTime() - new Date(entry.startTime).getTime();
    const breakMs = (entry.breaks ?? []).reduce((sum, b) => {
      if (!b.endTime || !b.startTime) {
        return sum;
      }
      return sum + (new Date(b.endTime).getTime() - new Date(b.startTime).getTime());
    }, 0);

    return this.formatHhMm(Math.max(0, grossMs - breakMs));
  }

  protected requestCorrection(entry: TimeEntry): void {
    this.dialogService.create({
      zTitle: 'Korrektur beantragen',
      zContent: RequestCorrectionDialog,
      zHideFooter: true,
      zWidth: '26rem',
      zData: { timeEntry: entry },
    });
  }

  protected projectName(projectId: TimeEntry['projectId']): string {
    if (projectId == null) {
      return '—';
    }

    const project = this.projects().find(p => Number(p.id) === Number(projectId));
    return project?.projectDisplayName ?? '—';
  }

  private async updateEntry(id: number, projectId: string, description: string): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await this.timeEntryService.apiTimeEntryIdPut$Json({
        id,
        body: { projectId: projectId === '' ? null : Number(projectId), description: description || null },
      });
      await this.load();
    } catch {
      this.error.set('Änderungen konnten nicht gespeichert werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  private syncRunningForm(): void {
    const running = this.timerService.runningEntry();
    this.runningProjectId = running?.projectId != null ? String(running.projectId) : '';
    this.runningDescription = running?.description ?? '';
  }

  private async load(): Promise<void> {
    this.loading.set(true);

    try {
      const entries = await this.timeEntryService.apiTimeEntryMineGet$Json();
      this.timeEntries.set(entries);
    } catch {
      this.error.set('Zeiteinträge konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
      this.syncRunningForm();
    }
  }

  private async loadProjects(): Promise<void> {
    try {
      this.projects.set(await this.projectService.apiProjectGet$Json());
    } catch {
      // Not critical for this page — project names just won't resolve.
    }
  }

  private async loadBreakSettings(): Promise<void> {
    try {
      const settings = await this.appSettingsService.apiAppSettingsGet$Json();
      this.breakReasonRequired.set(settings.breakReasonRequired ?? false);
    } catch {
      this.breakReasonRequired.set(false);
    }
  }

  private formatHhMm(ms: number): string {
    const totalMinutes = Math.floor(ms / 60000);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}:${minutes.toString().padStart(2, '0')}`;
  }
}
