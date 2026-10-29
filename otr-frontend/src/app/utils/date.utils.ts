/** Date helpers shared by the dashboard and the reports page. All calculations use the local time zone. */

/** Formats a date as a sortable local-calendar key, e.g. `2026-03-07`. */
export function toDateKey(date: Date): string {
  const pad = (value: number): string => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

/**
 * Truncates a date to its local calendar day and returns it as a timestamp, so that
 * a value created later today (e.g. 14:32) still compares equal to "today".
 */
export function toDateOnly(date: Date): number {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
}

/** Returns midnight of the Monday of the week the date falls into (weeks start on Monday). */
export function startOfWeek(date: Date): Date {
  const result: Date = new Date(date);
  const mondayIndex: number = (result.getDay() + 6) % 7;
  result.setDate(result.getDate() - mondayIndex);
  result.setHours(0, 0, 0, 0);
  return result;
}

/** Returns midnight of the first day of the month the date falls into. */
export function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

/** Returns the ISO 8601 week number (1-53) of a date. */
export function isoWeekNumber(date: Date): number {
  const target: Date = new Date(date);
  target.setHours(0, 0, 0, 0);
  // ISO weeks belong to the year that contains their Thursday.
  target.setDate(target.getDate() + 3 - ((target.getDay() + 6) % 7));
  const firstThursday: Date = new Date(target.getFullYear(), 0, 4);
  const diffMs: number = target.getTime() - firstThursday.getTime();
  return 1 + Math.round(diffMs / (7 * 24 * 3600 * 1000));
}

/** Formats a number of hours as `h:mm Std.`, e.g. `7.5` becomes `7:30 Std.`. */
export function formatHours(hours: number): string {
  const totalMinutes: number = Math.round(hours * 60);
  const wholeHours: number = Math.floor(totalMinutes / 60);
  const minutes: number = totalMinutes % 60;
  return `${wholeHours}:${minutes.toString().padStart(2, '0')} Std.`;
}
