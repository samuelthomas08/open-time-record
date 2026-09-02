import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideMessageSquareText } from '@ng-icons/lucide';

import { ApiConfiguration, AppSettingsService, CorrectionRequestDto, TimeEntryCorrectionRequestService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { RejectCorrectionDialog } from '@/pages/correction-requests/reject-correction-dialog/reject-correction-dialog';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardDialogService } from '@/shared/components/dialog';
import { ZardPopoverImports } from '@/shared/components/popover';
import { ZardTableImports } from '@/shared/components/table';

const PENDING = 0;
const APPROVED = 1;
const REJECTED = 2;

@Component({
  selector: 'app-correction-requests',
  imports: [DatePipe, NgIcon, ZardBadgeComponent, ZardButtonComponent, ZardPopoverImports, ZardTableImports],
  templateUrl: './correction-requests.html',
  viewProviders: [provideIcons({ lucideMessageSquareText })],
})
export class CorrectionRequests implements OnInit {
  private readonly correctionRequestService = inject(TimeEntryCorrectionRequestService);
  private readonly dialogService = inject(ZardDialogService);
  private readonly appSettingsService = inject(AppSettingsService);
  private readonly apiConfiguration = inject(ApiConfiguration);

  protected readonly authService = inject(AuthService);
  protected readonly PENDING = PENDING;
  protected readonly APPROVED = APPROVED;
  protected readonly REJECTED = REJECTED;

  protected readonly mineRequests = signal<CorrectionRequestDto[]>([]);
  protected readonly pendingRequests = signal<CorrectionRequestDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly actionPending = signal(false);
  protected readonly profilePicturesEnabled = signal(false);

  ngOnInit(): void {
    this.loadMine();
    this.loadSettings();

    if (this.authService.isSuperadmin()) {
      this.loadPending();
    }
  }

  protected reviewerPictureSrc(request: CorrectionRequestDto): string | null {
    const url = request.reviewedByProfilePictureUrl;
    return url ? `${this.apiConfiguration.rootUrl}${url}` : null;
  }

  protected approve(request: CorrectionRequestDto): Promise<void> {
    return this.review(() =>
      this.correctionRequestService.apiTimeEntryCorrectionRequestIdApprovePut$Json({
        id: request.id!,
      }),
    );
  }

  protected reject(request: CorrectionRequestDto): void {
    this.dialogService.create({
      zTitle: 'Antrag ablehnen',
      zContent: RejectCorrectionDialog,
      zHideFooter: true,
      zWidth: '28rem',
      zData: {
        request,
        onSuccess: () => {
          this.loadPending();
          this.loadMine();
        },
      },
    });
  }

  private async review(action: () => Promise<unknown>): Promise<void> {
    this.actionPending.set(true);
    this.error.set(null);

    try {
      await action();
      await Promise.all([this.loadPending(), this.loadMine()]);
    } catch {
      this.error.set('Aktion konnte nicht ausgeführt werden.');
    } finally {
      this.actionPending.set(false);
    }
  }

  private async loadMine(): Promise<void> {
    this.loading.set(true);

    try {
      this.mineRequests.set(
        await this.correctionRequestService.apiTimeEntryCorrectionRequestMineGet$Json(),
      );
    } catch {
      this.error.set('Anträge konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }

  private async loadPending(): Promise<void> {
    try {
      this.pendingRequests.set(
        await this.correctionRequestService.apiTimeEntryCorrectionRequestPendingGet$Json(),
      );
    } catch {
      this.error.set('Offene Anträge konnten nicht geladen werden.');
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
}
