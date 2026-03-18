import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HealthPlanService, HealthPlanDto } from '../data-access/health-plan.service';
import { HealthPlanFormComponent } from '../health-plan-form/health-plan-form.component';

@Component({
  selector: 'app-health-plans-list',
  standalone: true,
  imports: [HealthPlanFormComponent],
  templateUrl: './health-plans-list.component.html',
  styleUrl: './health-plans-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HealthPlansListComponent implements OnInit {
  private readonly healthPlanService = inject(HealthPlanService);
  private readonly destroyRef = inject(DestroyRef);

  readonly healthPlans = signal<HealthPlanDto[]>([]);
  readonly formOpen = signal(false);
  readonly selectedHealthPlan = signal<HealthPlanDto | null>(null);
  readonly errorMessage = signal('');

  ngOnInit() {
    this.loadHealthPlans();
  }

  openCreateForm() {
    this.selectedHealthPlan.set(null);
    this.formOpen.set(true);
  }

  openEditForm(healthPlan: HealthPlanDto) {
    this.selectedHealthPlan.set(healthPlan);
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.selectedHealthPlan.set(null);
  }

  onSaved(healthPlan: HealthPlanDto) {
    this.healthPlans.update((list) => {
      const index = list.findIndex((h) => h.id === healthPlan.id);
      if (index >= 0) {
        return list.map((h) => (h.id === healthPlan.id ? healthPlan : h));
      }
      return [...list, healthPlan];
    });
    this.closeForm();
  }

  private loadHealthPlans() {
    this.healthPlanService
      .getHealthPlans()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (healthPlans) => this.healthPlans.set(healthPlans),
        error: () => this.errorMessage.set('Erro ao carregar convênios.'),
      });
  }
}
