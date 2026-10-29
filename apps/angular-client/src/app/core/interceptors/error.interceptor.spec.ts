import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';
import { errorInterceptor } from './error.interceptor';

/** The interceptor redirects to the login page, so that route has to exist. */
@Component({ standalone: true, template: '' })
class LoginStubComponent {}

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let router: Router;
  let token: string | null;
  let auth: {
    token: ReturnType<typeof vi.fn>;
    clear: ReturnType<typeof vi.fn>;
    refreshSession: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    token = 'access-1';
    auth = {
      token: vi.fn(() => token),
      clear: vi.fn(),
      refreshSession: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        // Both interceptors, in the order the application registers them,
        // so a request carries a bearer token the way it does in the app.
        provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([{ path: 'login', component: LoginStubComponent }]),
        { provide: AuthService, useValue: auth },
      ],
    });

    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => controller.verify());

  it('should refresh the session and retry the request', async () => {
    // An expired access token is the common cause of a 401. The refresh
    // token is still good for one exchange, so the request is worth
    // repeating with the new token.
    auth.refreshSession.mockReturnValue(of({ token: 'access-2' } as never));

    let completed = false;
    http.get('/api/v1/Movie').subscribe(() => (completed = true));

    controller
      .expectOne('/api/v1/Movie')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    const retried = controller.expectOne('/api/v1/Movie');
    expect(retried.request.headers.get('Authorization')).toBe('Bearer access-2');
    retried.flush([]);

    await Promise.resolve();
    expect(completed).toBe(true);
    expect(auth.clear).not.toHaveBeenCalled();
  });

  it('should clear the session when the refresh fails', async () => {
    auth.refreshSession.mockReturnValue(throwError(() => new Error('Refresh failed')));

    let failed = false;
    http.get('/api/v1/Movie').subscribe({ error: () => (failed = true) });

    controller
      .expectOne('/api/v1/Movie')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await Promise.resolve();
    expect(failed).toBe(true);
    expect(auth.clear).toHaveBeenCalled();
  });

  it('should leave a 401 from the login request alone', async () => {
    let failed = false;
    http.post('/api/v1/Auth/login', {}).subscribe({ error: () => (failed = true) });

    controller
      .expectOne('/api/v1/Auth/login')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await Promise.resolve();
    expect(failed).toBe(true);
    expect(auth.refreshSession).not.toHaveBeenCalled();
    expect(auth.clear).not.toHaveBeenCalled();
  });

  it('should leave other failures alone', async () => {
    let failed = false;
    http.get('/api/v1/Movie').subscribe({ error: () => (failed = true) });

    controller.expectOne('/api/v1/Movie').flush(null, { status: 500, statusText: 'Error' });

    await Promise.resolve();
    expect(failed).toBe(true);
    expect(auth.refreshSession).not.toHaveBeenCalled();
    expect(auth.clear).not.toHaveBeenCalled();
  });

  it('should clear the session when a request without a token is rejected', async () => {
    // Nothing to refresh: the session is already gone.
    token = null;

    let failed = false;
    http.get('/api/v1/Movie').subscribe({ error: () => (failed = true) });

    controller
      .expectOne('/api/v1/Movie')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await Promise.resolve();
    expect(failed).toBe(true);
    expect(auth.clear).toHaveBeenCalled();
    expect(auth.refreshSession).not.toHaveBeenCalled();
  });

  it('should send the user to the login page', async () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    http.get('/api/v1/Movie').subscribe({ error: () => undefined });

    controller
      .expectOne('/api/v1/Movie')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    await Promise.resolve();
    expect(navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/api/v1/Movie' },
    });
  });
});
