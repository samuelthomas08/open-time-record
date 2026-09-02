import { Component, effect, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucidePlay, lucideSettings, lucideSquare } from '@ng-icons/lucide';

import { ZardButtonComponent } from '@/shared/components/button';
import { ZardNavigationMenuImports } from '@/shared/components/navigation-menu';
import { AuthService } from '@/services/auth.service';
import { TimerService } from '@/services/timer.service';
import { Logo } from '@/components/logo/logo';
import { UserMenu } from '@/components/user-menu/user-menu';

@Component({
  selector: 'app-header',
  imports: [NgIcon, ZardButtonComponent, ZardNavigationMenuImports, RouterLink, RouterLinkActive, Logo, UserMenu],
  templateUrl: './header.html',
  styleUrl: './header.css',
  viewProviders: [provideIcons({ lucidePlay, lucideSettings, lucideSquare })],
})
export class Header {
  protected readonly authService = inject(AuthService);
  protected readonly timerService = inject(TimerService);

  constructor() {
    effect(() => {
      if (this.authService.isLoggedIn()) {
        this.timerService.ensureLoaded();
      }
    });
  }

  protected async toggleTimer(): Promise<void> {
    if (this.timerService.isRunning()) {
      await this.timerService.stop();
    } else {
      await this.timerService.start();
    }
  }
}
