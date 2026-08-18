import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const THEME_KEY = 'otr_theme';

type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  private readonly darkSignal = signal(this.readInitial());
  readonly isDark = this.darkSignal.asReadonly();

  constructor() {
    this.apply(this.darkSignal());
  }

  toggle(): void {
    this.setDark(!this.darkSignal());
  }

  setDark(value: boolean): void {
    this.darkSignal.set(value);
    this.apply(value);

    if (this.isBrowser) {
      localStorage.setItem(THEME_KEY, value ? 'dark' : 'light');
    }
  }

  private apply(value: boolean): void {
    if (!this.isBrowser) {
      return;
    }

    document.documentElement.classList.toggle('dark', value);
  }

  private readInitial(): boolean {
    if (!this.isBrowser) {
      return false;
    }

    const stored = localStorage.getItem(THEME_KEY) as Theme | null;
    if (stored) {
      return stored === 'dark';
    }

    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
  }
}
