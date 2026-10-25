import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./core/auth/login.component').then((m) => m.LoginComponent),
  },
  { path: '', redirectTo: 'movies', pathMatch: 'full' },
  {
    path: 'movies',
    canMatch: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/movies/movies-list.component').then((m) => m.MoviesListComponent),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/movies/movie-form.component').then((m) => m.MovieFormComponent),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./features/movies/movie-form.component').then((m) => m.MovieFormComponent),
      },
    ],
  },
  {
    path: 'genres',
    canMatch: [authGuard],
    data: { lookupKind: 'filmGenres' },
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/lookups/lookup-list.component').then((m) => m.LookupListComponent),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent),
      },
    ],
  },
  {
    path: 'media-types',
    canMatch: [authGuard],
    data: { lookupKind: 'mediaTypes' },
    children: [
      { path: '', loadComponent: () => import('./features/lookups/lookup-list.component').then((m) => m.LookupListComponent) },
      { path: 'new', loadComponent: () => import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent) },
      { path: ':id/edit', loadComponent: () => import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent) },
    ],
  },
  {
    path: 'formats',
    canMatch: [authGuard],
    data: { lookupKind: 'formats' },
    children: [
      { path: '', loadComponent: () => import('./features/lookups/lookup-list.component').then((m) => m.LookupListComponent) },
      { path: 'new', loadComponent: () => import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent) },
      { path: ':id/edit', loadComponent: () => import('./features/lookups/lookup-form.component').then((m) => m.LookupFormComponent) },
    ],
  },
  { path: '**', redirectTo: 'movies' },
];
