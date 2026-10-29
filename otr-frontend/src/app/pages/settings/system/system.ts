import { Component, OnInit, inject, signal } from '@angular/core';

import { AppSettingsService } from '@/api-client';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';
import { ZardSwitchComponent } from '@/shared/components/switch';

/** Superadmin settings page for instance-wide feature toggles. */
@Component({
  selector: 'app-settings-system',
  imports: [ZardCardComponent, ZardCardContentComponent, ZardSwitchComponent],
  templateUrl: './system.html',
})
export class SettingsSystem implements OnInit {
  private readonly appSettingsService = inject(AppSettingsService);

  protected readonly profilePicturesEnabled = signal<boolean>(false);
  protected readonly breakReasonRequired = signal<boolean>(false);
  protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    this.load();
  }

  /** Enables or disables profile pictures instance-wide. */
  protected async toggleProfilePictures(enabled: boolean): Promise<void> {
    await this.save({ profilePicturesEnabled: enabled, breakReasonRequired: this.breakReasonRequired() });
  }

  /** Sets whether a break reason is mandatory. */
  protected async toggleBreakReasonRequired(required: boolean): Promise<void> {
    await this.save({ profilePicturesEnabled: this.profilePicturesEnabled(), breakReasonRequired: required });
  }

  /** Saves the form. */
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

  /** Fetches the data shown on this page and updates loading/error state. */
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
