import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { UserDto } from '../../settings/data-access/settings.service';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private readonly http = inject(HttpClient);

  readonly #user = signal<UserDto | null>(null);
  readonly currentUser = this.#user.asReadonly();
  readonly isDoctor = computed(() => this.#user()?.tenantRole === 'doctor');
  readonly isGlobalAdmin = computed(() => this.#user()?.globalRole === 'Admin');
  readonly tenantName = computed(() => this.#user()?.tenantName ?? null);

  getMe(): Observable<UserDto> {
    const cached = this.#user();
    if (cached) {
      return of(cached);
    }
    return this.http
      .get<UserDto>('/api/users/me', { withCredentials: true })
      .pipe(tap((user) => this.#user.set(user)));
  }

  invalidate(): void {
    this.#user.set(null);
  }
}
