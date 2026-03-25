import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';
import { GOOGLE_CLIENT_ID } from '../../core/tokens/google-client-id.token';
import { GOOGLE_REDIRECT_URI } from '../../core/tokens/google-redirect-uri.token';

const GOOGLE_AUTH_BASE_URL = 'https://accounts.google.com/o/oauth2/v2/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatProgressSpinnerModule, MatSnackBarModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly win = inject(WINDOW);
  private readonly destroyRef = inject(DestroyRef);
  private readonly googleClientId = inject(GOOGLE_CLIENT_ID);
  private readonly googleRedirectUri = inject(GOOGLE_REDIRECT_URI, { optional: true });

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  get emailControl() {
    return this.form.controls['email'];
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.auth
      .sendMagicLink(this.form.value.email!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/auth/magic-link-sent'], {
            state: { email: this.form.value.email },
          });
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao enviar o link. Tente novamente.');
        },
      });
  }

  loginWithGoogle() {
    const origin = this.googleRedirectUri ?? this.win.location.origin;
    const redirectUri = `${origin}/auth/callback`;
    const params = new URLSearchParams({
      response_type: 'code',
      scope: 'openid email profile',
      client_id: this.googleClientId,
      redirect_uri: redirectUri,
    });
    this.win.location.href = `${GOOGLE_AUTH_BASE_URL}?${params.toString()}`;
  }
}
