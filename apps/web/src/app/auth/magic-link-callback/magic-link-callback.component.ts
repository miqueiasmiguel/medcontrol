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

// TODO: atualizar após publicação nas lojas
const APP_STORE_URL = '#'; // https://apps.apple.com/app/medcontrol/id...
const PLAY_STORE_URL = '#'; // https://play.google.com/store/apps/details?id=com.medcontrol.app

type Platform = 'desktop' | 'ios' | 'android';

@Component({
  selector: 'app-magic-link-callback',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="callback-page">
      @if (appNotFound()) {
        <div class="app-not-found">
          <h2>Baixe o app MedControl</h2>
          <p>Use o aplicativo para acessar sua conta com segurança.</p>
          @if (platform() === 'ios') {
            <a [href]="appStoreUrl" class="store-btn store-btn--ios">
              Baixar na App Store
            </a>
          }
          @if (platform() === 'android') {
            <a [href]="playStoreUrl" class="store-btn store-btn--android">
              Baixar no Google Play
            </a>
          }
        </div>
      } @else if (tryingDeepLink()) {
        <mat-spinner diameter="48" />
        <p>Abrindo o aplicativo...</p>
        <p class="subtitle">Aguarde um momento...</p>
      } @else if (loading()) {
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
        padding: 2rem;
        text-align: center;
      }
      .subtitle {
        font-size: 0.875rem;
        opacity: 0.6;
      }
      .app-not-found {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 1rem;
        max-width: 320px;
      }
      .app-not-found h2 {
        margin: 0;
        font-size: 1.5rem;
      }
      .app-not-found p {
        margin: 0;
        opacity: 0.7;
      }
      .store-btn {
        display: inline-block;
        padding: 0.875rem 2rem;
        border-radius: 0.5rem;
        font-weight: 600;
        text-decoration: none;
        color: white;
      }
      .store-btn--ios {
        background: #000;
      }
      .store-btn--android {
        background: #01875f;
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
  readonly appNotFound = signal(false);
  readonly platform = signal<Platform>('desktop');

  readonly appStoreUrl = APP_STORE_URL;
  readonly playStoreUrl = PLAY_STORE_URL;

  private trampolineTimer: ReturnType<typeof setTimeout> | null = null;
  private appOpenDetected = false;

  ngOnInit() {
    const token = this.route.snapshot.queryParams['token'] as string | undefined;

    if (!token) {
      this.router.navigate(['/auth/login']);
      return;
    }

    const detectedPlatform = this.detectPlatform();
    this.platform.set(detectedPlatform);

    if (detectedPlatform === 'desktop') {
      this.verifyOnWeb(token);
      return;
    }

    // Mobile: attempt to open the native app first
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
          this.appNotFound.set(true);
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

  private detectPlatform(): Platform {
    const ua = this.win.navigator?.userAgent ?? '';
    if (/iphone|ipad|ipod/i.test(ua)) return 'ios';
    if (/android/i.test(ua)) return 'android';
    return 'desktop';
  }

  private verifyOnWeb(token: string): void {
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
}
