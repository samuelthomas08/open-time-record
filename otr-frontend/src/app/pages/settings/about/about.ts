import { Component } from '@angular/core';

import { Logo } from '@/components/logo/logo';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';

const APP_VERSION = '0.1.0';

@Component({
  selector: 'app-settings-about',
  imports: [Logo, ZardCardComponent, ZardCardContentComponent],
  templateUrl: './about.html',
})
export class SettingsAbout {
  protected readonly version = APP_VERSION;
}
