import { Component, PLATFORM_ID, afterNextRender, effect, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { Header } from './components/header/header';
import { AuthService } from '@/services/auth.service';
import { ThemeService } from '@/services/theme.service';
import { TimerService } from '@/services/timer.service';

const APP_NAME = 'Open Time Record';

@Component({
  selector: 'app-root',
  imports: [Header, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);
  private readonly timerService = inject(TimerService);
  private readonly router = inject(Router);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  constructor() {
    // The server can't read localStorage, so route guards always see "logged
    // out" during SSR and may have redirected to /login. Once the client
    // hydrates with the real session, bounce back if we actually are logged in.
    afterNextRender(() => {
      const onAuthPage = window.location.pathname === '/login' || window.location.pathname === '/register';
      if (onAuthPage && this.authService.isLoggedIn()) {
        this.router.navigateByUrl('/');
      }
    });

    effect(() => {
      if (!this.isBrowser) {
        return;
      }

      document.title = this.timerService.isRunning()
        ? `${this.timerService.elapsed().slice(0, 5)} · ${APP_NAME}`
        : APP_NAME;
    });
  }
}
