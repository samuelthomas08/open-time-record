import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';

import { ApiConfiguration, AppSettingsService, UserService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardComponent, ZardCardContentComponent } from '@/shared/components/card';

/** Mirrors the backend PermissionResource/PermissionLevel enums — see roles.ts for the full list. */
const PROFILE_PICTURE_RESOURCE = 20;
const WRITE_LEVEL = 2;

@Component({
  selector: 'app-settings-profile',
  imports: [ZardButtonComponent, ZardCardComponent, ZardCardContentComponent],
  templateUrl: './profile.html',
})
export class SettingsProfile implements OnInit {
  private readonly userService = inject(UserService);
  private readonly appSettingsService = inject(AppSettingsService);
  private readonly apiConfiguration = inject(ApiConfiguration);

  protected readonly authService = inject(AuthService);
  protected readonly user = this.authService.currentUser;

  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('fileInput');

  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly profilePicturesEnabled = signal(false);
  protected readonly canEditPicture = signal(false);
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
    const url = this.authService.profilePictureUrl();
    return url ? `${this.apiConfiguration.rootUrl}${url}` : null;
  });

  ngOnInit(): void {
    this.load();
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
      this.authService.setProfilePictureUrl(updated.profilePictureUrl ?? null);
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
      this.authService.setProfilePictureUrl(updated.profilePictureUrl ?? null);
    } catch {
      this.error.set('Bild konnte nicht entfernt werden.');
    } finally {
      this.uploading.set(false);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    this.loadError.set(null);
    const userId = this.user()?.userId;

    try {
      const [settings, permissions, fullUser] = await Promise.all([
        this.appSettingsService.apiAppSettingsGet$Json(),
        this.userService.apiUserMePermissionsGet$Json(),
        userId ? this.userService.apiUserIdGet$Json({ id: userId }) : Promise.resolve(null),
      ]);

      this.profilePicturesEnabled.set(settings.profilePicturesEnabled ?? false);
      this.authService.setProfilePictureUrl(fullUser?.profilePictureUrl ?? null);

      const own = permissions.find(p => Number(p.resource) === PROFILE_PICTURE_RESOURCE);
      this.canEditPicture.set(Number(own?.level ?? 0) >= WRITE_LEVEL);
    } catch {
      this.loadError.set('Einstellungen konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
