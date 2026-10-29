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
import { TIME_ENTRY_STATUS_RUNNING as RUNNING, netWorkedMs } from '@/utils/time-entry.utils';

/** Page for starting/stopping the timer, managing breaks and reviewing or editing the own time entries. */
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
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);
  protected readonly actionPending = signal<boolean>(false);
  protected readonly editingId = signal<number | null>(null);
  protected readonly breakReasonRequired = signal<boolean>(false);

  protected runningProjectId: string = '';
  protected runningDescription: string = '';
  protected editProjectId: string = '';
  protected editDescription: string = '';

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    this.timerService.ensureLoaded().then(() => this.syncRunningForm());
    this.load();
    this.loadProjects();
    this.loadBreakSettings();
  }

  /** Starts the timer. */
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

  /** Stops the timer. */
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

  /** Saves project and description of the running entry. */
  protected async saveRunning(): Promise<void> {
    const running = this.timerService.runningEntry();
    if (!running?.id) {
      return;
    }

    await this.updateEntry(Number(running.id), this.runningProjectId, this.runningDescription);
    await this.timerService.refresh();
  }

  /** Switches a row into edit mode. */
  protected startEdit(entry: TimeEntry): void {
    this.editingId.set(Number(entry.id));
    this.editProjectId = entry.projectId != null ? String(entry.projectId) : '';
    this.editDescription = entry.description ?? '';
  }

  /** Leaves edit mode without saving. */
  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  /** Saves project and description of the edited entry. */
  protected async saveEdit(entry: TimeEntry): Promise<void> {
    await this.updateEntry(Number(entry.id), this.editProjectId, this.editDescription);
    this.editingId.set(null);

    if (entry.status === RUNNING) {
      await this.timerService.refresh();
    }
  }

  /** Deletes an entry. */
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

  /** Opens the dialog for starting a break. */
  protected openStartBreakDialog(): void {
    this.dialogService.create({
      zTitle: 'Pause starten',
      zContent: StartBreakDialog,
      zHideFooter: true,
      zWidth: '24rem',
      zData: { reasonRequired: this.breakReasonRequired() },
    });
  }

  /** Ends the running break. */
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

    return this.formatHhMm(netWorkedMs(entry));
  }

  /** Opens the dialog for requesting a correction of an entry. */
  protected requestCorrection(entry: TimeEntry): void {
    this.dialogService.create({
      zTitle: 'Korrektur beantragen',
      zContent: RequestCorrectionDialog,
      zHideFooter: true,
      zWidth: '26rem',
      zData: { timeEntry: entry },
    });
  }

  /** Resolves a project id to its display name, or an em dash if unknown. */
  protected projectName(projectId: TimeEntry['projectId']): string {
    if (projectId == null) {
      return '—';
    }

    const project = this.projects().find(p => Number(p.id) === Number(projectId));
    return project?.projectDisplayName ?? '—';
  }

  /** Persists project and description of an entry and reloads the list. */
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

  /** Copies the running entry's values into the inline edit form. */
  private syncRunningForm(): void {
    const running = this.timerService.runningEntry();
    this.runningProjectId = running?.projectId != null ? String(running.projectId) : '';
    this.runningDescription = running?.description ?? '';
  }

  /** Fetches the data shown on this page and updates loading/error state. */
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

  /** Fetches projects so their names can be shown; failures are not critical. */
  private async loadProjects(): Promise<void> {
    try {
      this.projects.set(await this.projectService.apiProjectGet$Json());
    } catch {
      // Not critical for this page — project names just won't resolve.
    }
  }

  /** Fetches whether a break reason is mandatory. */
  private async loadBreakSettings(): Promise<void> {
    try {
      const settings = await this.appSettingsService.apiAppSettingsGet$Json();
      this.breakReasonRequired.set(settings.breakReasonRequired ?? false);
    } catch {
      this.breakReasonRequired.set(false);
    }
  }

  /** Formats milliseconds as `h:mm`. */
  private formatHhMm(ms: number): string {
    const totalMinutes = Math.floor(ms / 60000);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}:${minutes.toString().padStart(2, '0')}`;
  }
}
