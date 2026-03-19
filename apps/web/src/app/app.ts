import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './settings/data-access/theme.service';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  template: '<router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  // Injetado aqui para garantir instanciação eager na inicialização do app,
  // independente de qual rota está ativa. Sem isto, ThemeService só seria
  // criado ao visitar /settings, e o tema salvo não seria aplicado no reload.
  protected readonly theme = inject(ThemeService);
}
