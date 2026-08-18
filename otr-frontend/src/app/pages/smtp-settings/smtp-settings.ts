import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SmtpSettingsService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCheckboxComponent } from '@/shared/components/checkbox';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

@Component({
  selector: 'app-smtp-settings',
  imports: [FormsModule, ZardButtonComponent, ZardCheckboxComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './smtp-settings.html',
})
export class SmtpSettingsPage implements OnInit {
  private readonly smtpSettingsService = inject(SmtpSettingsService);

  protected readonly authService = inject(AuthService);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly isConfigured = signal(false);

  protected host = '';
  protected port = 587;
  protected username = '';
  protected password = '';
  protected fromAddress = '';
  protected fromName = '';
  protected useSsl = true;

  ngOnInit(): void {
    if (!this.authService.isSuperadmin()) {
      this.loading.set(false);
      return;
    }

    this.load();
  }

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
