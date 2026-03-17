import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Router } from '@angular/router';
import { MagicLinkSentComponent } from './magic-link-sent.component';

describe('MagicLinkSentComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MagicLinkSentComponent],
      providers: [provideRouter([])],
    });
  });

  function createComponent(state: Record<string, unknown> = {}) {
    jest.spyOn(TestBed.inject(Router), 'getCurrentNavigation').mockReturnValue({
      extras: { state },
    } as never);
    const fixture = TestBed.createComponent(MagicLinkSentComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement };
  }

  it('should display confirmation message', () => {
    const { el } = createComponent();
    expect(el.textContent).toContain('Verifique seu e-mail');
  });

  it('should display the email from navigation state', () => {
    const { el } = createComponent({ email: 'user@test.com' });
    expect(el.textContent).toContain('user@test.com');
  });

  it('should have a link back to /auth/login', () => {
    const { el } = createComponent();
    const link = el.querySelector('[routerLink="/auth/login"]') ?? el.querySelector('a[href="/auth/login"]');
    expect(link).toBeTruthy();
  });
});
