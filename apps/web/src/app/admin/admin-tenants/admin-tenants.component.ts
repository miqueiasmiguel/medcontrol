import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AdminTenantsService, AdminTenantDto } from '../data-access/admin-tenants.service';

@Component({
  selector: 'app-admin-tenants',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  template: `
    <div class="admin-tenants">
      <h1>Tenants</h1>

      @if (loading()) {
        <p>Carregando...</p>
      } @else {
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Status</th>
              <th>Membros</th>
              <th>Criado em</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (tenant of tenants(); track tenant.id) {
              <tr>
                <td>{{ tenant.name }}</td>
                <td>{{ tenant.isActive ? 'Ativo' : 'Inativo' }}</td>
                <td>{{ tenant.memberCount }}</td>
                <td>{{ tenant.createdAt | date: 'dd/MM/yyyy' }}</td>
                <td>
                  <button
                    data-testid="toggle-status"
                    (click)="toggleStatus(tenant)"
                  >
                    {{ tenant.isActive ? 'Desativar' : 'Ativar' }}
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
})
export class AdminTenantsComponent implements OnInit {
  private readonly adminService = inject(AdminTenantsService);

  readonly tenants = signal<AdminTenantDto[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.adminService.listTenants().subscribe({
      next: (tenants) => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleStatus(tenant: AdminTenantDto): void {
    const newStatus = !tenant.isActive;
    this.adminService.setTenantStatus(tenant.id, newStatus).subscribe(() => {
      this.tenants.update((list) =>
        list.map((t) => (t.id === tenant.id ? { ...t, isActive: newStatus } : t)),
      );
    });
  }
}
