import { ApplicationConfig, provideBrowserGlobalErrorListeners, isDevMode } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideRouterStore } from '@ngrx/router-store';

import { routes } from './app.routes';
import { reducers } from './store';
import { MovieEffects } from './store/movies/movies.effects';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

// provideStoreDevtools is gated on isDevMode() so the Redux DevTools hook
// is not registered in production at all. Combined with the build-time
// "production" optimisation in @ngrx/store-devtools this ensures no
// production console output, no time-travel buffers, and no observable
// leakage. See docs/SECURITY-OVERVIEW.md.
const devtoolsProviders = isDevMode()
  ? [provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() })]
  : [];

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
    provideStore(reducers),
    provideEffects([MovieEffects]),
    ...devtoolsProviders,
    provideRouterStore(),
  ],
};
