import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="callback-page">
      @if (loading()) {
        <mat-spinner diameter="48" />
        <p>Autenticando com Google...</p>
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
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoogleCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly win = inject(WINDOW);

  readonly loading = signal(true);

  ngOnInit() {
    const code = this.route.snapshot.queryParams['code'] as string | undefined;

    if (!code) {
      this.router.navigate(['/auth/login']);
      return;
    }

    const redirectUri = `${this.win.location.origin}/auth/callback`;

    this.auth.loginWithGoogle(code, redirectUri).subscribe({
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
