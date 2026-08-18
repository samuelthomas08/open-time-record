import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ApiConfiguration, AppSettingsService, User, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardDropdownImports } from '@/shared/components/dropdown';

@Component({
  selector: 'app-user-menu',
  imports: [RouterLink, ZardDropdownImports],
  templateUrl: './user-menu.html',
})
export class UserMenu implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly appSettingsService = inject(AppSettingsService);
  private readonly apiConfiguration = inject(ApiConfiguration);

  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('fileInput');

  protected readonly user = this.authService.currentUser;
  protected readonly profilePicturesEnabled = signal(false);
  protected readonly profilePictureUrl = signal<string | null>(null);
  protected readonly uploading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : '';
  });

  protected readonly initials = computed(() => {
    const user = this.user();
    return user ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase() : '';
  });

  protected readonly pictureSrc = computed(() => {
    const url = this.profilePictureUrl();
    return url ? `${this.apiConfiguration.rootUrl}${url}` : null;
  });

  ngOnInit(): void {
    this.loadSettings();
    this.loadOwnPicture();
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected triggerFileInput(): void {
    this.error.set(null);
    this.fileInput().nativeElement.click();
  }

  protected async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';

    const userId = this.user()?.userId;
    if (!file || !userId) {
      return;
    }

    this.uploading.set(true);
    this.error.set(null);

    try {
      const updated = await this.userService.apiUserIdProfilePicturePost$Json({ id: userId, body: { file } });
      this.profilePictureUrl.set(updated.profilePictureUrl ?? null);
    } catch {
      this.error.set('Bild konnte nicht hochgeladen werden.');
    } finally {
      this.uploading.set(false);
    }
  }

  protected async removePicture(): Promise<void> {
    const userId = this.user()?.userId;
    if (!userId) {
      return;
    }

    this.uploading.set(true);
    this.error.set(null);

    try {
      const updated = await this.userService.apiUserIdProfilePictureDelete$Json({ id: userId });
      this.profilePictureUrl.set(updated.profilePictureUrl ?? null);
    } catch {
      this.error.set('Bild konnte nicht entfernt werden.');
    } finally {
      this.uploading.set(false);
    }
  }

  private async loadSettings(): Promise<void> {
    try {
      const settings = await this.appSettingsService.apiAppSettingsGet$Json();
      this.profilePicturesEnabled.set(settings.profilePicturesEnabled ?? false);
    } catch {
      this.profilePicturesEnabled.set(false);
    }
  }

  private async loadOwnPicture(): Promise<void> {
    const userId = this.user()?.userId;
    if (!userId) {
      return;
    }

    try {
      const fullUser: User = await this.userService.apiUserIdGet$Json({ id: userId });
      this.profilePictureUrl.set(fullUser.profilePictureUrl ?? null);
    } catch {
      // Non-critical — the avatar just falls back to initials.
    }
  }
}
