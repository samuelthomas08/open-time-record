import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { CorrectionRequestDto, TimeEntryCorrectionRequestService } from '@/api-client';
import { AuthService } from '@/services/auth.service';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardTableImports } from '@/shared/components/table';

const PENDING = 0;
const APPROVED = 1;
const REJECTED = 2;

@Component({
  selector: 'app-correction-requests',
  imports: [DatePipe, ZardBadgeComponent, ZardButtonComponent, ZardTableImports],
  templateUrl: './correction-requests.html',
})
export class CorrectionRequests implements OnInit {
  private readonly correctionRequestService = inject(TimeEntryCorrectionRequestService);

  protected readonly authService = inject(AuthService);
  protected readonly PENDING = PENDING;
  protected readonly APPROVED = APPROVED;
  protected readonly REJECTED = REJECTED;

  protected readonly mineRequests = signal<CorrectionRequestDto[]>([]);
  protected readonly pendingRequests = signal<CorrectionRequestDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly actionPending = signal(false);

  ngOnInit(): void {
    this.loadMine();

    if (this.authService.isSuperadmin()) {
      this.loadPending();
    }
  }

  protected approve(request: CorrectionRequestDto): Promise<void> {
    return this.review(() =>
      this.correctionRequestService.apiTimeEntryCorrectionRequestIdApprovePut$Json({
        id: request.id!,
      }),
    );
  }

  protected reject(request: CorrectionRequestDto): Promise<void> {
    return this.review(() =>
      this.correctionRequestService.apiTimeEntryCorrectionRequestIdRejectPut$Json({
        id: request.id!,
      }),
    );
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
}
