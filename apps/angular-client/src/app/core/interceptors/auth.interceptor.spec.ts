import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let token: string | null;

  beforeEach(() => {
    token = 'access-1';

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AuthService,
          useValue: {
            token: () => token,
            getRefreshToken: () => null,
          },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should attach the bearer token', () => {
    http.get('/api/v1/Movie').subscribe();

    const request = controller.expectOne('/api/v1/Movie');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-1');
    request.flush([]);
  });

  it('should not attach a token when there is none', () => {
    token = null;

    http.get('/api/v1/Movie').subscribe();

    const request = controller.expectOne('/api/v1/Movie');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush([]);
  });

  it('should not attach a token to the login request', () => {
    http.post('/api/v1/Auth/login', {}).subscribe();

    const request = controller.expectOne('/api/v1/Auth/login');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('should not attach a token to the refresh request', () => {
    http.post('/api/v1/Auth/refresh', {}).subscribe();

    const request = controller.expectOne('/api/v1/Auth/refresh');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });
});
