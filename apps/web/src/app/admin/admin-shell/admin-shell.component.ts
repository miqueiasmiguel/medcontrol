import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet],
  template: `
    <div class="admin-layout">
      <header class="admin-header">
        <h2>MedControl Admin</h2>
      </header>
      <main class="admin-content">
        <router-outlet />
      </main>
    </div>
  `,
})
export class AdminShellComponent {}
