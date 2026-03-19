import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SettingsService } from './data-access/settings.service';
import { ThemeService, Theme } from './data-access/theme.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  readonly themeService = inject(ThemeService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly profileForm = this.fb.group({
    displayName: ['', [Validators.maxLength(100)]],
  });

  ngOnInit() {
    this.loadProfile();
  }

  applyTheme(theme: Theme) {
    this.themeService.apply(theme);
  }

  saveProfile() {
    if (this.profileForm.invalid || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const displayName = this.profileForm.value.displayName || null;

    this.settingsService
      .updateProfile({ displayName })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.successMessage.set('Perfil atualizado com sucesso.');
        },
        error: () => {
          this.saving.set(false);
          this.errorMessage.set('Erro ao atualizar perfil.');
        },
      });
  }

  private loadProfile() {
    this.loading.set(true);
    this.settingsService
      .getMe()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.loading.set(false);
          this.profileForm.patchValue({ displayName: user.displayName ?? '' });
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao carregar perfil.');
        },
      });
  }
}
