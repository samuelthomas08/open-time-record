import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

@Component({
  selector: 'app-activate',
  imports: [FormsModule, RouterLink, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './activate.html',
})
export class Activate {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected email = this.route.snapshot.queryParamMap.get('email') ?? '';
  protected code = '';
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly info = signal<string | null>(null);

  protected async onSubmit(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    this.info.set(null);

    try {
      await this.authService.activate(this.email, this.code);
      await this.router.navigateByUrl('/');
    } catch {
      this.error.set('Der Code ist ungültig oder abgelaufen.');
    } finally {
      this.loading.set(false);
    }
  }

  protected async resend(): Promise<void> {
    this.error.set(null);
    this.info.set(null);

    try {
      await this.authService.resendActivation(this.email);
      this.info.set('Falls das Konto existiert und noch nicht bestätigt ist, wurde ein neuer Code versendet.');
    } catch {
      this.error.set('Der Code konnte nicht erneut versendet werden.');
    }
  }
}
