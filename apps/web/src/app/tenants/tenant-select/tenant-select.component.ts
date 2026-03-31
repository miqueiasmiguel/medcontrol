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
import { TenantService, TenantDto } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

@Component({
  selector: 'app-tenant-select',
  standalone: true,
  imports: [],
  template: `
    <div class="tenant-select-page">
      <h1>Selecionar organização</h1>
      @for (tenant of tenants(); track tenant.id) {
        <button (click)="selectTenant(tenant.id)">{{ tenant.name }}</button>
      }
      @if (errorMessage()) {
        <p class="error">{{ errorMessage() }}</p>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantSelectComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly currentUserService = inject(CurrentUserService);

  readonly tenants = signal<TenantDto[]>([]);
  readonly errorMessage = signal('');

  ngOnInit() {
    this.tenantService
      .getMyTenants()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tenants) => this.tenants.set(tenants),
        error: () => this.errorMessage.set('Erro ao carregar organizações.'),
      });
  }

  selectTenant(tenantId: string) {
    this.tenantService
      .switchTenant(tenantId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.currentUserService.invalidate();
          this.router.navigate(['/']);
        },
        error: () => this.errorMessage.set('Erro ao selecionar organização.'),
      });
  }
}
