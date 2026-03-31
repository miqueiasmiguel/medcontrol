import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TenantService, TenantDto } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

@Component({
  selector: 'app-tenant-select',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  templateUrl: './tenant-select.component.html',
  styleUrl: './tenant-select.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantSelectComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly currentUserService = inject(CurrentUserService);

  readonly tenants = signal<TenantDto[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly selectingId = signal<string | null>(null);

  ngOnInit() {
    this.tenantService
      .getMyTenants()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tenants) => {
          this.tenants.set(tenants);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Erro ao carregar organizações.');
          this.loading.set(false);
        },
      });
  }

  selectTenant(tenantId: string) {
    this.selectingId.set(tenantId);
    this.errorMessage.set('');

    this.tenantService
      .switchTenant(tenantId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.currentUserService.invalidate();
          this.router.navigate(['/']);
        },
        error: () => {
          this.selectingId.set(null);
          this.errorMessage.set('Erro ao selecionar organização.');
        },
      });
  }

  getRoleLabel(role: string): string {
    const labels: Record<string, string> = {
      owner: 'Proprietário',
      admin: 'Administrador',
      operator: 'Operador',
      doctor: 'Médico',
    };
    return labels[role] ?? role;
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .slice(0, 2)
      .map((w) => w[0])
      .join('')
      .toUpperCase();
  }
}
