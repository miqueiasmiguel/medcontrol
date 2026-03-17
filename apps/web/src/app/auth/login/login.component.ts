import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

const GOOGLE_AUTH_URL =
  'https://accounts.google.com/o/oauth2/v2/auth?response_type=code&scope=openid%20email%20profile';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatProgressSpinnerModule, MatSnackBarModule],
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
    const redirectUri = `${this.win.location.origin}/auth/callback`;
    this.win.location.href = `${GOOGLE_AUTH_URL}&redirect_uri=${encodeURIComponent(redirectUri)}`;
  }
}
