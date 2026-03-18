import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-magic-link-sent',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './magic-link-sent.component.html',
  styleUrl: './magic-link-sent.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MagicLinkSentComponent {
  private readonly router = inject(Router);

  readonly email: string =
    (this.router.getCurrentNavigation()?.extras?.state?.['email'] as string) ?? '';
}
