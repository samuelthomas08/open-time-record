import { Component, OnInit, computed, inject, signal } from '@angular/core';
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

  protected readonly user = this.authService.currentUser;
  protected readonly profilePicturesEnabled = signal(false);

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
    this.loadSettings();
    this.loadOwnPicture();
  }

  protected logout(): void {
    this.authService.logout();
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
      this.authService.setProfilePictureUrl(fullUser.profilePictureUrl ?? null);
    } catch {
      // Non-critical — the avatar just falls back to initials.
    }
  }
}
