import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Standalone login form. Lives outside the chrome layout. On success,
 * navigates to the {@code returnUrl} query param or /movies.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <main class="login-shell">
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-card">
        <h1>Sign in to MoviesApp</h1>
        <p class="hint">Use the email and password you registered with the Web API.</p>

        <label>
          <span>Email</span>
          <input type="email" formControlName="email" autocomplete="email" />
        </label>

        <label>
          <span>Password</span>
          <input type="password" formControlName="password" autocomplete="current-password" />
        </label>

        @if (error()) {
          <p class="error">{{ error() }}</p>
        }

        <button type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>
    </main>
  `,
  styles: [`
    .login-shell {
      min-height: 100vh;
      display: grid;
      place-items: center;
      background: var(--bg, #fafafa);
      padding: 2rem;
    }
    .login-card {
      width: 100%;
      max-width: 360px;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      padding: 2rem;
      background: var(--surface, #fff);
      border: 1px solid var(--surface-border, #e6e6e6);
      border-radius: 16px;
    }
    .login-card h1 { margin: 0 0 0.25rem; font-size: 20px; }
    .hint { margin: 0 0 1rem; color: #666; font-size: 13px; }
    label { display: flex; flex-direction: column; gap: 4px; font-size: 13px; }
    input {
      padding: 10px 12px;
      border-radius: 10px;
      border: 1px solid #ccc;
      font-size: 14px;
    }
    button {
      margin-top: 0.5rem;
      padding: 10px 16px;
      border-radius: 999px;
      border: none;
      background: var(--primary, #7a1256);
      color: white;
      font: 500 14px Roboto, sans-serif;
      cursor: pointer;
    }
    button[disabled] { opacity: 0.6; cursor: not-allowed; }
    .error { color: #b00020; font-size: 13px; margin: 0; }
  `],
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  onSubmit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/movies';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: { error?: { message?: string }; status?: number }) => {
        this.loading.set(false);
        this.error.set(
          err.error?.message ??
            (err.status === 401 ? 'Email or password is incorrect.' : 'Sign-in failed. Please try again.'),
        );
      },
    });
  }
}
