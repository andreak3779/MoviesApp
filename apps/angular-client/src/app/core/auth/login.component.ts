import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { describeHttpError } from '../http-error';

/**
 * Standalone sign-in form. Lives outside the chrome layout. On success,
 * navigates to the {@code returnUrl} query param or /movies.
 *
 * The same screen creates accounts, because the movie collection is scoped
 * to a user and a fresh install has none.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <main class="login-shell">
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-card">
        <h1>{{ isRegister() ? 'Create your account' : 'Sign in to MoviesApp' }}</h1>
        <p class="hint">
          {{ isRegister()
            ? 'Pick an email and a password, and your collection is ready.'
            : 'Use the email and password you registered with the Web API.' }}
        </p>

        @if (isRegister()) {
          <div class="names">
            <label>
              <span>First name</span>
              <input type="text" formControlName="firstName" autocomplete="given-name" />
            </label>

            <label>
              <span>Last name</span>
              <input type="text" formControlName="lastName" autocomplete="family-name" />
            </label>
          </div>
        }

        <label>
          <span>Email</span>
          <input type="email" formControlName="email" autocomplete="email" />
        </label>

        <label>
          <span>Password</span>
          <input type="password" formControlName="password" autocomplete="current-password" />
        </label>

        @if (isRegister()) {
          <label>
            <span>Confirm password</span>
            <input type="password" formControlName="confirmPassword" autocomplete="new-password" />
          </label>
          <p class="hint rules">
            At least 8 characters, with an uppercase letter, a lowercase letter and a number.
          </p>
        }

        @if (error()) {
          <p class="error">{{ error() }}</p>
        }

        <button type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Please wait…' : isRegister() ? 'Create account' : 'Sign in' }}
        </button>

        <button type="button" class="link" (click)="toggleMode()">
          {{ isRegister() ? 'Already have an account? Sign in' : 'Need an account? Create one' }}
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
    .hint { margin: 0 0 1rem; color: var(--ink-secondary, #555); font-size: 13px; }
    .hint.rules { margin: -0.25rem 0 0; font-size: 12px; }
    .names { display: flex; gap: 12px; }
    .names label { flex: 1; }
    label { display: flex; flex-direction: column; gap: 4px; font-size: 13px; }
    input {
      padding: 10px 12px;
      border-radius: 10px;
      border: 1px solid var(--surface-border, #e6e6e6);
      font-size: 14px;
      font-family: inherit;
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
    button.link {
      margin-top: 0;
      background: none;
      color: var(--primary, #7a1256);
      padding: 0;
      font-size: 13px;
    }
    .error { color: var(--danger, #b03b3b); font-size: 13px; margin: 0; }
  `],
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly isRegister = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: [''],
    firstName: [''],
    lastName: [''],
  });

  toggleMode(): void {
    this.isRegister.set(!this.isRegister());
    this.error.set(null);
    this.form.controls.confirmPassword.setValidators(
      this.isRegister() ? [Validators.required] : [],
    );
    this.form.controls.confirmPassword.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set(null);

    const v = this.form.getRawValue();
    const request = this.isRegister()
      ? this.auth.register({
          email: v.email,
          password: v.password,
          confirmPassword: v.confirmPassword,
          firstName: v.firstName || undefined,
          lastName: v.lastName || undefined,
        })
      : this.auth.login({ email: v.email, password: v.password });

    request.subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/movies';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(err));
      },
    });
  }
}
