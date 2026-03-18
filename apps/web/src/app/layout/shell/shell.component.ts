import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent],
  template: `
    <div class="shell" [class.shell--collapsed]="collapsed()">
      <app-sidebar [collapsed]="collapsed()" (toggleCollapse)="toggle()" />
      <main class="shell__content">
        <router-outlet />
      </main>
    </div>
  `,
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  readonly collapsed = signal(false);

  toggle() {
    this.collapsed.update((v) => !v);
  }
}
