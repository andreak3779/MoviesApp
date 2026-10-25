import { InjectionToken } from '@angular/core';

/**
 * Base URL the Angular client uses to reach the Web API.
 *
 * Override at bootstrap via the {@link API_BASE_URL} provider. Defaults to
 * the dev port used by docker-compose.yml (8081). For production set this
 * via a build-time substitute or a runtime configuration endpoint.
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => 'http://localhost:8081/api/v1',
});
