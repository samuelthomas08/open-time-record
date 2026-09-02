import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardFieldImports } from '@/shared/components/field';
import { ZardInputComponent } from '@/shared/components/input';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink, ZardButtonComponent, ZardFieldImports, ZardInputComponent],
  templateUrl: './login.html',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  async onSubmit(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      await this.authService.login(this.email, this.password);
      await this.router.navigateByUrl('/');
    } catch {
      this.error.set('E-Mail oder Passwort ist falsch.');
    } finally {
      this.loading.set(false);
    }
  }
}
