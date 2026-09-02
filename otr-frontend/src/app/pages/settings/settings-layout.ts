import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideBuilding2,
  lucideCircleUserRound,
  lucideFolderKanban,
  lucideInfo,
  lucideMail,
  lucideServer,
  lucideShieldCheck,
  lucideSunMoon,
  lucideUsers,
} from '@ng-icons/lucide';

import { AuthService } from '@/services/auth.service';
import { LayoutImports } from '@/shared/components/layout';

@Component({
  selector: 'app-settings-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, LayoutImports, NgIcon],
  templateUrl: './settings-layout.html',
  viewProviders: [
    provideIcons({
      lucideSunMoon,
      lucideCircleUserRound,
      lucideUsers,
      lucideShieldCheck,
      lucideBuilding2,
      lucideFolderKanban,
      lucideMail,
      lucideServer,
      lucideInfo,
    }),
  ],
})
export class SettingsLayout {
  protected readonly authService = inject(AuthService);
}
