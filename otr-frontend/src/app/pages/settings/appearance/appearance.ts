import { Component, inject } from '@angular/core';

import { ThemeService } from '@/services/theme.service';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';
import { ZardSwitchComponent } from '@/shared/components/switch';

@Component({
  selector: 'app-settings-appearance',
  imports: [ZardCardComponent, ZardCardContentComponent, ZardSwitchComponent],
  templateUrl: './appearance.html',
})
export class SettingsAppearance {
  protected readonly themeService = inject(ThemeService);
}
