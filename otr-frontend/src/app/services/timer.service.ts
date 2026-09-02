import { Injectable, afterNextRender, computed, inject, signal } from '@angular/core';

import { TimeEntry, TimeEntryBreak, TimeEntryService } from '@/api-client';

const RUNNING = 0;

function formatHhMmSs(totalSeconds: number): string {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  return [hours, minutes, seconds].map(n => n.toString().padStart(2, '0')).join(':');
}

@Injectable({ providedIn: 'root' })
export class TimerService {
  private readonly timeEntryService = inject(TimeEntryService);

  private readonly runningEntrySignal = signal<TimeEntry | null>(null);
  private readonly loadedSignal = signal(false);
  private readonly nowTick = signal(Date.now());
  private tickInterval?: ReturnType<typeof setInterval>;

  readonly runningEntry = this.runningEntrySignal.asReadonly();
  readonly isRunning = computed(() => this.runningEntrySignal() !== null);

  readonly runningBreak = computed<TimeEntryBreak | null>(
    () => this.runningEntrySignal()?.breaks?.find(b => !b.endTime) ?? null,
  );

  /** How long the entry has actually been worked, i.e. gross time minus every completed break. */
  readonly elapsed = computed(() => {
    const running = this.runningEntrySignal();
    if (!running?.startTime) {
      return '00:00:00';
    }

    const startMs = new Date(running.startTime).getTime();
    const openBreak = this.runningBreak();
    // Freeze the clock at the moment a break started instead of letting it keep ticking through it.
    const endpointMs = openBreak?.startTime ? new Date(openBreak.startTime).getTime() : this.nowTick();
    const grossMs = Math.max(0, endpointMs - startMs);
    const totalSeconds = Math.max(0, Math.floor((grossMs - this.completedBreakMs()) / 1000));

    return formatHhMmSs(totalSeconds);
  });

  private readonly completedBreakMs = computed(() =>
    (this.runningEntrySignal()?.breaks ?? []).reduce((sum, b) => {
      if (!b.endTime || !b.startTime) {
        return sum;
      }
      return sum + (new Date(b.endTime).getTime() - new Date(b.startTime).getTime());
    }, 0),
  );

  constructor() {
    afterNextRender(() => {
      this.tickInterval = setInterval(() => this.nowTick.set(Date.now()), 1000);
    });
  }

  async ensureLoaded(): Promise<void> {
    if (!this.loadedSignal()) {
      await this.refresh();
    }
  }

  async refresh(): Promise<void> {
    const entries = await this.timeEntryService.apiTimeEntryMineGet$Json();
    const running = entries.find(entry => entry.status === RUNNING) ?? null;
    this.runningEntrySignal.set(running);
    this.loadedSignal.set(true);
  }

  async start(): Promise<void> {
    const entry = await this.timeEntryService.apiTimeEntryStartPost$Json();
    this.runningEntrySignal.set(entry);
  }

  async stop(): Promise<void> {
    const running = this.runningEntrySignal();
    if (!running?.id) {
      return;
    }

    await this.timeEntryService.apiTimeEntryIdStopPost$Json({ id: running.id });
    this.runningEntrySignal.set(null);
  }

  async startBreak(reason?: string): Promise<void> {
    const running = this.runningEntrySignal();
    if (!running?.id) {
      return;
    }

    const newBreak = await this.timeEntryService.apiTimeEntryIdBreaksPost$Json({
      id: running.id,
      body: { reason: reason || null },
    });
    this.runningEntrySignal.update(entry => (entry ? { ...entry, breaks: [...(entry.breaks ?? []), newBreak] } : entry));
  }

  async stopBreak(): Promise<void> {
    const running = this.runningEntrySignal();
    const openBreak = this.runningBreak();
    if (!running?.id || !openBreak?.id) {
      return;
    }

    const stopped = await this.timeEntryService.apiTimeEntryIdBreaksBreakIdStopPut$Json({
      id: running.id,
      breakId: openBreak.id,
    });
    this.runningEntrySignal.update(entry =>
      entry ? { ...entry, breaks: (entry.breaks ?? []).map(b => (b.id === stopped.id ? stopped : b)) } : entry,
    );
  }
}
