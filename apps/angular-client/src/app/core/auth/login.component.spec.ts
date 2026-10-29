import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../services/auth.service';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let router: Router;
  let auth: {
    login: ReturnType<typeof vi.fn>;
    register: ReturnType<typeof vi.fn>;
  };
  let queryParams: Record<string, string>;

  beforeEach(async () => {
    queryParams = {};
    auth = {
      login: vi.fn().mockReturnValue(of({})),
      register: vi.fn().mockReturnValue(of({})),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: auth },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              // Read on demand, so a test can set the query params afterwards.
              get queryParamMap() {
                return convertToParamMap(queryParams);
              },
            },
          },
        },
        { provide: Router, useValue: { navigateByUrl: vi.fn().mockResolvedValue(true) } },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(LoginComponent);
  });

  function fill(email: string, password: string): void {
    fixture.componentInstance.form.patchValue({ email, password });
  }

  it('should sign in and go to the movies', () => {
    fixture.detectChanges();

    fill('ada@example.com', 'Passw0rd!');
    fixture.componentInstance.onSubmit();

    expect(auth.login).toHaveBeenCalledWith({
      email: 'ada@example.com',
      password: 'Passw0rd!',
    });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/movies');
  });

  it('should go back to where the user came from', () => {
    queryParams['returnUrl'] = '/movies/42/edit';
    fixture.detectChanges();

    fill('ada@example.com', 'Passw0rd!');
    fixture.componentInstance.onSubmit();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/movies/42/edit');
  });

  it('should create an account instead of signing in', () => {
    fixture.detectChanges();

    fixture.componentInstance.toggleMode();
    fixture.componentInstance.form.patchValue({
      email: 'ada@example.com',
      password: 'Passw0rd!',
      confirmPassword: 'Passw0rd!',
      firstName: 'Ada',
      lastName: 'Lovelace',
    });
    fixture.componentInstance.onSubmit();

    expect(auth.register).toHaveBeenCalledWith({
      email: 'ada@example.com',
      password: 'Passw0rd!',
      confirmPassword: 'Passw0rd!',
      firstName: 'Ada',
      lastName: 'Lovelace',
    });
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('should ask for the password twice when creating an account', () => {
    fixture.detectChanges();

    fixture.componentInstance.toggleMode();
    fixture.componentInstance.form.patchValue({
      email: 'ada@example.com',
      password: 'Passw0rd!',
      confirmPassword: '',
    });
    fixture.componentInstance.onSubmit();

    expect(auth.register).not.toHaveBeenCalled();
  });

  it('should refuse to sign in with an invalid form', () => {
    fixture.detectChanges();

    fill('not-an-email', 'short');
    fixture.componentInstance.onSubmit();

    expect(auth.login).not.toHaveBeenCalled();
  });

  it('should show the reason the sign-in failed', () => {
    auth.login.mockReturnValue(throwError(() => new Error('Invalid credentials.')));
    fixture.detectChanges();

    fill('ada@example.com', 'Passw0rd!');
    fixture.componentInstance.onSubmit();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.error').textContent).toContain(
      'Invalid credentials.',
    );
  });

  it('should switch between signing in and creating an account', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Sign in');

    fixture.componentInstance.toggleMode();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Create your account');
  });
});
