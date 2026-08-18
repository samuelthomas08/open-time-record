import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './register.html',
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected firstName = '';
  protected lastName = '';
  protected email = this.route.snapshot.queryParamMap.get('email') ?? '';
  protected password = '';
  protected invitationCode = this.route.snapshot.queryParamMap.get('code') ?? '';
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  async onSubmit(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const outcome = await this.authService.register(
        this.firstName,
        this.lastName,
        this.email,
        this.password,
        this.invitationCode,
      );

      if (outcome.needsActivation) {
        await this.router.navigate(['/activate'], { queryParams: { email: this.email } });
      } else {
        await this.router.navigateByUrl('/');
      }
    } catch {
      this.error.set('Registrierung fehlgeschlagen. Ist die E-Mail-Adresse schon vergeben, oder ist der Einladungscode ungültig?');
    } finally {
      this.loading.set(false);
    }
  }
}
