import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';
import { DoctorFormComponent } from '../doctor-form/doctor-form.component';
import { DoctorLinkFormComponent } from '../doctor-link-form/doctor-link-form.component';

@Component({
  selector: 'app-doctors-list',
  standalone: true,
  imports: [DoctorFormComponent, DoctorLinkFormComponent],
  templateUrl: './doctors-list.component.html',
  styleUrl: './doctors-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorsListComponent implements OnInit {
  private readonly doctorService = inject(DoctorService);
  private readonly destroyRef = inject(DestroyRef);

  readonly doctors = signal<DoctorDto[]>([]);
  readonly formOpen = signal(false);
  readonly selectedDoctor = signal<DoctorDto | null>(null);
  readonly linkFormOpen = signal(false);
  readonly doctorToLink = signal<DoctorDto | null>(null);
  readonly errorMessage = signal('');
  readonly showLinkHint = signal(false);

  ngOnInit() {
    this.loadDoctors();
  }

  openCreateForm() {
    this.selectedDoctor.set(null);
    this.formOpen.set(true);
  }

  openEditForm(doctor: DoctorDto) {
    this.selectedDoctor.set(doctor);
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.selectedDoctor.set(null);
  }

  onSaved(doctor: DoctorDto) {
    this.doctors.update((list) => {
      const index = list.findIndex((d) => d.id === doctor.id);
      if (index >= 0) {
        return list.map((d) => (d.id === doctor.id ? doctor : d));
      }
      return [...list, doctor];
    });
    this.closeForm();
  }

  onCreatedWithoutInvite(doctor: DoctorDto) {
    this.doctors.update((list) => {
      if (!list.find((d) => d.id === doctor.id)) {
        return [...list, doctor];
      }
      return list;
    });
    this.showLinkHint.set(true);
    setTimeout(() => this.showLinkHint.set(false), 8000);
  }

  openLinkForm(doctor: DoctorDto) {
    this.doctorToLink.set(doctor);
    this.linkFormOpen.set(true);
  }

  closeLinkForm() {
    this.linkFormOpen.set(false);
    this.doctorToLink.set(null);
  }

  onLinked(updated: DoctorDto) {
    this.doctors.update((list) => list.map((d) => (d.id === updated.id ? updated : d)));
    this.closeLinkForm();
  }

  private loadDoctors() {
    this.doctorService
      .getDoctors()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (doctors) => this.doctors.set(doctors),
        error: () => this.errorMessage.set('Erro ao carregar médicos.'),
      });
  }
}
