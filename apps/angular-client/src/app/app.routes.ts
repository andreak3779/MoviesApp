import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'movies', pathMatch: 'full' },
  {
    path: 'movies',
    loadComponent: () =>
      import('./features/movies/movies-list.component').then((m) => m.MoviesListComponent),
  },
];
