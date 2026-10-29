import type { TimeEntry, TimeEntryBreak } from '@/api-client';

/** Numeric value of the backend's `TimeEntryStatus.Running`; enums are serialized as their declaration order. */
export const TIME_ENTRY_STATUS_RUNNING = 0;

/** Milliseconds in one hour. */
export const MS_PER_HOUR = 3_600_000;

/** Total duration in milliseconds of all breaks that have already ended. A running break is ignored. */
export function sumCompletedBreakMs(breaks: TimeEntryBreak[] | undefined | null): number {
  return (breaks ?? []).reduce((sum: number, entryBreak: TimeEntryBreak): number => {
    if (!entryBreak.startTime || !entryBreak.endTime) {
      return sum;
    }
    return sum + (new Date(entryBreak.endTime).getTime() - new Date(entryBreak.startTime).getTime());
  }, 0);
}

/** Net worked time in milliseconds of a finished entry (gross time minus breaks); `0` while it is still running. */
export function netWorkedMs(entry: TimeEntry): number {
  if (!entry.startTime || !entry.endTime) {
    return 0;
  }

  const grossMs: number = new Date(entry.endTime).getTime() - new Date(entry.startTime).getTime();
  return Math.max(0, grossMs - sumCompletedBreakMs(entry.breaks));
}
