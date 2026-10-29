import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../api-config';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let controller: HttpTestingController;
  const baseUrl = 'http://api.test/api/v1';

  beforeEach(() => {
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });

    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    controller.verify();
    sessionStorage.clear();
  });

  it('should sign in and keep the tokens', () => {
    const service = TestBed.inject(AuthService);
    service.login({ email: 'ada@example.com', password: 'Passw0rd!' }).subscribe();

    const request = controller.expectOne(`${baseUrl}/Auth/login`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ email: 'ada@example.com', password: 'Passw0rd!' });

    request.flush({
      token: 'access-1',
      refreshToken: 'refresh-1',
      expiresAt: '2026-10-29T18:00:00Z',
      email: 'ada@example.com',
    });

    expect(service.token()).toBe('access-1');
    expect(service.isAuthenticated()).toBe(true);
    expect(sessionStorage.getItem('moviesapp.accessToken')).toBe('access-1');
    expect(sessionStorage.getItem('moviesapp.refreshToken')).toBe('refresh-1');
  });

  it('should register and keep the tokens', () => {
    const service = TestBed.inject(AuthService);
    service
      .register({
        email: 'ada@example.com',
        password: 'Passw0rd!',
        confirmPassword: 'Passw0rd!',
      })
      .subscribe();

    const request = controller.expectOne(`${baseUrl}/Auth/register`);
    expect(request.request.method).toBe('POST');

    request.flush({
      token: 'access-1',
      refreshToken: 'refresh-1',
      expiresAt: '2026-10-29T18:00:00Z',
      email: 'ada@example.com',
    });

    expect(service.token()).toBe('access-1');
  });

  it('should exchange the stored refresh token for a new session', () => {
    sessionStorage.setItem('moviesapp.accessToken', 'access-1');
    sessionStorage.setItem('moviesapp.refreshToken', 'refresh-1');

    const service = TestBed.inject(AuthService);
    expect(service.getRefreshToken()).toBe('refresh-1');

    service.refreshSession().subscribe();

    const request = controller.expectOne(`${baseUrl}/Auth/refresh`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ token: 'access-1', refreshToken: 'refresh-1' });

    request.flush({
      token: 'access-2',
      refreshToken: 'refresh-2',
      expiresAt: '2026-10-29T18:15:00Z',
      email: 'ada@example.com',
    });

    expect(service.token()).toBe('access-2');
    expect(sessionStorage.getItem('moviesapp.refreshToken')).toBe('refresh-2');
  });

  it('should refuse to refresh when there is no session', () => {
    const service = TestBed.inject(AuthService);
    let failed = false;
    service.refreshSession().subscribe({
      error: () => (failed = true),
    });

    expect(failed).toBe(true);
  });

  it('should send the access token and the refresh token when signing out', () => {
    sessionStorage.setItem('moviesapp.accessToken', 'access-1');
    sessionStorage.setItem('moviesapp.refreshToken', 'refresh-1');

    const service = TestBed.inject(AuthService);
    service.logout().subscribe();

    const request = controller.expectOne(`${baseUrl}/Auth/logout`);
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-1');
    expect(request.request.body).toEqual({ token: 'access-1', refreshToken: 'refresh-1' });

    request.flush(null);

    expect(service.token()).toBeNull();
    expect(sessionStorage.getItem('moviesapp.accessToken')).toBeNull();
  });

  it('should clear the session without calling the API', () => {
    sessionStorage.setItem('moviesapp.accessToken', 'access-1');
    sessionStorage.setItem('moviesapp.refreshToken', 'refresh-1');

    const service = TestBed.inject(AuthService);
    service.clear();

    expect(service.token()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(sessionStorage.getItem('moviesapp.accessToken')).toBeNull();
    expect(sessionStorage.getItem('moviesapp.refreshToken')).toBeNull();
  });
});
