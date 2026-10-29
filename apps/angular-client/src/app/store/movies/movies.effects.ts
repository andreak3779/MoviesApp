import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, mergeMap } from 'rxjs';
import { MovieService } from '../../services/movie-service';
import { describeHttpError } from '../../core/http-error';
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
          map((response) =>
            MovieActions.loadMoviesSuccess({
              movies: response.items,
              page: {
                pageNumber: response.pageNumber,
                pageSize: response.pageSize,
                totalCount: response.totalCount,
                totalPages: response.totalPages,
                hasPrevious: response.hasPrevious,
                hasNext: response.hasNext,
              },
            }),
          ),
          catchError((error: unknown) =>
            of(MovieActions.loadMoviesFailure({ error: describeHttpError(error) })),
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
            of(MovieActions.addMovieFailure({ error: describeHttpError(error) })),
          ),
        ),
      ),
    ),
  );

  updateMovie$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MovieActions.updateMovie),
      mergeMap(({ id, changes }) =>
        this.movieService.update(id, changes).pipe(
          // The API answers an update with 204 No Content, so the updated
          // entity has to be read back to keep the store in step with the
          // server rather than with what the form believed it sent.
          switchMap(() => this.movieService.getById(id)),
          map((movie) => MovieActions.updateMovieSuccess({ movie })),
          catchError((error: unknown) =>
            of(MovieActions.updateMovieFailure({ error: describeHttpError(error) })),
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
            of(MovieActions.removeMovieFailure({ error: describeHttpError(error) })),
          ),
        ),
      ),
    ),
  );
}
