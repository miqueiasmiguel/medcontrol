import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  NgZone,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

const DEEP_LINK_TIMEOUT_MS = 2500;

@Component({
  selector: 'app-magic-link-callback',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="callback-page">
      @if (tryingDeepLink()) {
        <p>Abrindo o aplicativo...</p>
        <p class="subtitle">Se não abrir automaticamente, aguarde...</p>
      }
      @if (!tryingDeepLink() && loading()) {
        <mat-spinner diameter="48" />
        <p>Autenticando...</p>
      }
    </div>
  `,
  styles: [
    `
      .callback-page {
        min-height: 100vh;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 1rem;
      }
      .subtitle {
        font-size: 0.875rem;
        opacity: 0.6;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MagicLinkCallbackComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly win = inject(WINDOW);
  private readonly doc = inject(DOCUMENT);
  private readonly ngZone = inject(NgZone);

  readonly loading = signal(true);
  readonly tryingDeepLink = signal(false);

  private trampolineTimer: ReturnType<typeof setTimeout> | null = null;
  private appOpenDetected = false;

  ngOnInit() {
    const token = this.route.snapshot.queryParams['token'] as string | undefined;

    if (!token) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.tryingDeepLink.set(true);
    this.win.location.href = `medcontrol://verify?token=${token}`;

    const onVisibility = () => {
      if (this.doc.visibilityState === 'hidden') {
        this.appOpenDetected = true;
        if (this.trampolineTimer !== null) {
          clearTimeout(this.trampolineTimer);
          this.trampolineTimer = null;
        }
        this.doc.removeEventListener('visibilitychange', onVisibility);
      }
    };
    this.doc.addEventListener('visibilitychange', onVisibility);

    this.trampolineTimer = setTimeout(() => {
      this.ngZone.run(() => {
        this.trampolineTimer = null;
        this.doc.removeEventListener('visibilitychange', onVisibility);
        this.tryingDeepLink.set(false);

        if (!this.appOpenDetected) {
          this.auth.verifyMagicLink(token).subscribe({
            next: () => {
              this.loading.set(false);
              this.router.navigate(['/']);
            },
            error: () => {
              this.loading.set(false);
              this.router.navigate(['/auth/login']);
            },
          });
        }
      });
    }, DEEP_LINK_TIMEOUT_MS);
  }

  ngOnDestroy() {
    if (this.trampolineTimer !== null) {
      clearTimeout(this.trampolineTimer);
      this.trampolineTimer = null;
    }
  }
}
