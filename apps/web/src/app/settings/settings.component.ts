import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap, of } from 'rxjs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SettingsService } from './data-access/settings.service';
import { ThemeService, Theme } from './data-access/theme.service';
import { CurrentUserService } from '../core/data-access/current-user.service';
import { DoctorService } from '../doctors/data-access/doctor.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [ReactiveFormsModule, MatProgressSpinnerModule, RouterLink],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly doctorService = inject(DoctorService);
  readonly themeService = inject(ThemeService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly isDoctor = this.currentUserService.isDoctor;

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly savingDoctorProfile = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly profileForm = this.fb.group({
    displayName: ['', [Validators.maxLength(100)]],
  });

  readonly doctorProfileForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    crm: ['', [Validators.required, Validators.maxLength(20)]],
    councilState: ['', [Validators.required, Validators.maxLength(2)]],
    specialty: ['', [Validators.required, Validators.maxLength(256)]],
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

  saveDoctorProfile() {
    if (this.doctorProfileForm.invalid || this.savingDoctorProfile()) {
      this.doctorProfileForm.markAllAsTouched();
      return;
    }

    this.savingDoctorProfile.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const raw = this.doctorProfileForm.getRawValue();

    this.settingsService
      .updateMyDoctorProfile({
        name: raw.name ?? '',
        crm: raw.crm ?? '',
        councilState: raw.councilState ?? '',
        specialty: raw.specialty ?? '',
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.savingDoctorProfile.set(false);
          this.successMessage.set('Perfil médico atualizado com sucesso.');
        },
        error: () => {
          this.savingDoctorProfile.set(false);
          this.errorMessage.set('Erro ao atualizar perfil médico.');
        },
      });
  }

  private loadProfile() {
    this.loading.set(true);
    this.currentUserService
      .getMe()
      .pipe(
        switchMap((user) => {
          this.profileForm.patchValue({ displayName: user.displayName ?? '' });
          if (user.tenantRole === 'doctor') {
            return this.doctorService.getMyDoctorProfile();
          }
          return of(null);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (doctorProfile) => {
          this.loading.set(false);
          if (doctorProfile) {
            this.doctorProfileForm.patchValue({
              name: doctorProfile.name,
              crm: doctorProfile.crm,
              councilState: doctorProfile.councilState,
              specialty: doctorProfile.specialty,
            });
          }
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao carregar perfil.');
        },
      });
  }
}
