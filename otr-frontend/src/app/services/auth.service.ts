import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

import { AuthService as GeneratedAuthService } from '@/api-client';
import type { AuthResponse } from '@/api-client';

const TOKEN_KEY = 'otr_auth_token';
const USER_KEY = 'otr_auth_user';

export interface CurrentUser {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
}

export type RegisterOutcome = { needsActivation: true } | { needsActivation: false };

/** Holds the login session (token and basic user data), persists it in `localStorage` and wraps the generated auth API client. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly generatedAuthService = inject(GeneratedAuthService);
  private readonly router = inject(Router);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  private readonly currentUserSignal = signal<CurrentUser | null>(this.readStoredUser());
  private readonly profilePictureUrlSignal = signal<string | null>(null);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUserSignal() !== null);
  readonly isSuperadmin = computed(() => this.currentUserSignal()?.roles.includes('Superadmin') ?? false);

  /** Shared across the header avatar and the Profile settings page, so editing it in one place updates the other live. */
  readonly profilePictureUrl = this.profilePictureUrlSignal.asReadonly();

  /** Updates the shared profile picture URL. */
  setProfilePictureUrl(url: string | null): void {
    this.profilePictureUrlSignal.set(url);
  }

  get token(): string | null {
    return this.isBrowser ? localStorage.getItem(TOKEN_KEY) : null;
  }

  /** Logs in and stores the session. */
  async login(email: string, password: string): Promise<void> {
    const response = await this.generatedAuthService.apiAuthLoginPost$Json({ body: { email, password } });
    this.persistSession(response);
  }

  /** Registers an account; stores the session right away if no e-mail verification is needed. */
  async register(firstName: string, lastName: string, email: string, password: string, invitationCode?: string): Promise<RegisterOutcome> {
    const response = await this.generatedAuthService.apiAuthRegisterPost$Json({
      body: { firstName, lastName, email, password, invitationCode: invitationCode || null },
    });

    if (response.auth) {
      this.persistSession(response.auth);
      return { needsActivation: false };
    }

    return { needsActivation: true };
  }

  /** Confirms the e-mail address with the emailed code and stores the session. */
  async activate(email: string, code: string): Promise<void> {
    const response = await this.generatedAuthService.apiAuthActivatePost$Json({ body: { email, code } });
    this.persistSession(response);
  }

  /** Asks the backend to send a new verification code. */
  async resendActivation(email: string): Promise<void> {
    await this.generatedAuthService.apiAuthResendActivationPost$Json({ body: { email } });
  }

  /** Ends the session and returns to the login page. */
  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    }
    this.currentUserSignal.set(null);
    this.profilePictureUrlSignal.set(null);
    this.router.navigateByUrl('/login');
  }

  /** Stores token and user data in memory and `localStorage`. */
  private persistSession(response: AuthResponse): void {
    const user: CurrentUser = {
      userId: Number(response.userId),
      firstName: response.firstName!,
      lastName: response.lastName!,
      email: response.email!,
      roles: response.roles ?? [],
    };

    if (this.isBrowser) {
      localStorage.setItem(TOKEN_KEY, response.token!);
      localStorage.setItem(USER_KEY, JSON.stringify(user));
    }
    this.currentUserSignal.set(user);
  }

  /** Restores the user from `localStorage`, or `null` if unavailable. */
  private readStoredUser(): CurrentUser | null {
    if (!this.isBrowser) {
      return null;
    }

    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}
