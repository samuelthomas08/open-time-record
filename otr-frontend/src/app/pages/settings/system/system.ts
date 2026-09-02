import { Component, OnInit, inject, signal } from '@angular/core';

import { AppSettingsService } from '@/api-client';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';
import { ZardSwitchComponent } from '@/shared/components/switch';

@Component({
  selector: 'app-settings-system',
  imports: [ZardCardComponent, ZardCardContentComponent, ZardSwitchComponent],
  templateUrl: './system.html',
})
export class SettingsSystem implements OnInit {
  private readonly appSettingsService = inject(AppSettingsService);

  protected readonly profilePicturesEnabled = signal(false);
  protected readonly breakReasonRequired = signal(false);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);

  ngOnInit(): void {
    this.load();
  }

  protected async toggleProfilePictures(enabled: boolean): Promise<void> {
    await this.save({ profilePicturesEnabled: enabled, breakReasonRequired: this.breakReasonRequired() });
  }

  protected async toggleBreakReasonRequired(required: boolean): Promise<void> {
    await this.save({ profilePicturesEnabled: this.profilePicturesEnabled(), breakReasonRequired: required });
  }

  private async save(body: { profilePicturesEnabled: boolean; breakReasonRequired: boolean }): Promise<void> {
    this.saving.set(true);

    try {
      const response = await this.appSettingsService.apiAppSettingsPut$Json({ body });
      this.profilePicturesEnabled.set(response.profilePicturesEnabled ?? false);
      this.breakReasonRequired.set(response.breakReasonRequired ?? false);
    } finally {
      this.saving.set(false);
    }
  }

  private async load(): Promise<void> {
    try {
      const response = await this.appSettingsService.apiAppSettingsGet$Json();
      this.profilePicturesEnabled.set(response.profilePicturesEnabled ?? false);
      this.breakReasonRequired.set(response.breakReasonRequired ?? false);
    } finally {
      this.loading.set(false);
    }
  }
}
