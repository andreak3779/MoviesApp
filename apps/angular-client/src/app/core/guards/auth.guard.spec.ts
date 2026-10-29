import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authenticated: boolean;

  beforeEach(() => {
    authenticated = false;

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: { isAuthenticated: () => authenticated },
        },
      ],
    });
  });

  /** Runs the guard the way the router would: inside an injection context. */
  function run(): boolean | UrlTree {
    return TestBed.runInInjectionContext(() =>
      authGuard(
        {} as never,
        [{ path: 'movies' }, { path: '42' }] as never,
        {} as never,
      ),
    ) as boolean | UrlTree;
  }

  it('should let a signed-in user through', () => {
    authenticated = true;

    expect(run()).toBe(true);
  });

  it('should send an anonymous user to the login page', () => {
    const result = run();

    expect(result).toBeInstanceOf(UrlTree);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/login?returnUrl=%2Fmovies%2F42',
    );
  });
});
