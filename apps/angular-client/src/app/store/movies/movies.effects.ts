import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, mergeMap } from 'rxjs';
import { MovieService } from '../../services/movie-service';
import { MovieActions } from './movies.actions';

@Injectable()
export class MovieEffects {
  private readonly actions$ = inject(Actions);
  private readonly movieService = inject(MovieService);

  loadMovies$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MovieActions.loadMovies),
      switchMap(({ query }) =>
        this.movieService.search({ pageNumber: 1, pageSize: 50, ...query }).pipe(
          map(({ items }) => MovieActions.loadMoviesSuccess({ movies: items })),
          catchError((error: unknown) =>
            of(MovieActions.loadMoviesFailure({ error: String(error) })),
          ),
        ),
      ),
    ),
  );

  addMovie$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MovieActions.addMovie),
      mergeMap(({ movie }) =>
        this.movieService.create(movie).pipe(
          map((saved) => MovieActions.addMovieSuccess({ movie: saved })),
          catchError((error: unknown) =>
            of(MovieActions.addMovieFailure({ error: String(error) })),
          ),
        ),
      ),
    ),
  );

  updateMovie$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MovieActions.updateMovie),
      mergeMap(({ movie }) =>
        this.movieService.update(movie.Id, movie).pipe(
          map(() => MovieActions.updateMovieSuccess({ movie })),
          catchError((error: unknown) =>
            of(MovieActions.updateMovieFailure({ error: String(error) })),
          ),
        ),
      ),
    ),
  );

  removeMovie$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MovieActions.removeMovie),
      mergeMap(({ id }) =>
        this.movieService.remove(id).pipe(
          map(() => MovieActions.removeMovieSuccess({ id })),
          catchError((error: unknown) =>
            of(MovieActions.removeMovieFailure({ error: String(error) })),
          ),
        ),
      ),
    ),
  );
}
