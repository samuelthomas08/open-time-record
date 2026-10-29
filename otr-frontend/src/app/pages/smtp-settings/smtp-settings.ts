import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SmtpSettingsService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCheckboxComponent } from '@/shared/components/checkbox';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

/** Superadmin form for configuring the outgoing mail server. */
@Component({
  selector: 'app-smtp-settings',
  imports: [FormsModule, ZardButtonComponent, ZardCheckboxComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './smtp-settings.html',
})
export class SmtpSettingsPage implements OnInit {
  private readonly smtpSettingsService = inject(SmtpSettingsService);

  protected readonly authService = inject(AuthService);
  protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly isConfigured = signal<boolean>(false);

  protected host: string = '';
  protected port: number = 587;
  protected username: string = '';
  protected password: string = '';
  protected fromAddress: string = '';
  protected fromName: string = '';
  protected useSsl: boolean = true;

  /** Loads the data the page needs when it is created. */
  ngOnInit(): void {
    if (!this.authService.isSuperadmin()) {
      this.loading.set(false);
      return;
    }

    this.load();
  }

  /** Saves the form. */
  protected async save(): Promise<void> {
    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    try {
      await this.smtpSettingsService.apiSmtpSettingsPut$Json({
        body: {
          host: this.host,
          port: this.port,
          username: this.username,
          password: this.password || null,
          fromAddress: this.fromAddress,
          fromName: this.fromName,
          useSsl: this.useSsl,
        },
      });
      this.password = '';
      this.isConfigured.set(true);
      this.success.set('SMTP-Einstellungen wurden gespeichert.');
    } catch {
      this.error.set('Einstellungen konnten nicht gespeichert werden.');
    } finally {
      this.saving.set(false);
    }
  }

  /** Fetches the data shown on this page and updates loading/error state. */
  private async load(): Promise<void> {
    this.loading.set(true);

    try {
      const settings = await this.smtpSettingsService.apiSmtpSettingsGet$Json();
      this.isConfigured.set(settings.isConfigured ?? false);
      this.host = settings.host ?? '';
      this.port = settings.port != null ? Number(settings.port) : 587;
      this.username = settings.username ?? '';
      this.fromAddress = settings.fromAddress ?? '';
      this.fromName = settings.fromName ?? '';
      this.useSsl = settings.useSsl ?? true;
    } catch {
      this.error.set('Einstellungen konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
