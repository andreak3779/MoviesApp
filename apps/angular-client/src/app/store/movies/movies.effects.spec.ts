import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';

import { MovieEffects } from './movies.effects';
import { MovieActions } from './movies.actions';
import { MovieService } from '../../services/movie-service';
import { Movie, PagedResponse } from '../../models/movie';

const movie1: Movie = {
  id: '00000000-0000-0000-0000-000000000001',
  title: 'The Matrix',
  releaseYear: 1999,
  description: '',
  favourite: false,
  criterion: false,
  criterionNumber: null,
  ownerId: 'user-1',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
  genre: 'Sci-Fi',
  mediaType: 'Blu-Ray',
  format: 'Movie',
  filmGenres: [],
  mediaTypes: [],
  formats: [],
};

const pagedResponse = (items: Movie[]): PagedResponse<Movie> => ({
  items,
  pageNumber: 1,
  pageSize: 20,
  totalCount: items.length,
  totalPages: 1,
  hasPrevious: false,
  hasNext: false,
});

describe('MovieEffects', () => {
  let actions$: Observable<Action>;
  let effects: MovieEffects;
  let movieService: {
    search: ReturnType<typeof vi.fn>;
    getById: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    remove: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    movieService = {
      search: vi.fn(),
      getById: vi.fn(),
      create: vi.fn(),
      update: vi.fn(),
      remove: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        MovieEffects,
        provideMockActions(() => actions$),
        { provide: MovieService, useValue: movieService },
      ],
    });

    effects = TestBed.inject(MovieEffects);
  });

  describe('loadMovies$', () => {
    it('should dispatch loadMoviesSuccess with returned movies', async () => {
      movieService.search.mockReturnValue(of(pagedResponse([movie1])));
      actions$ = of(MovieActions.loadMovies({}));

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(
        MovieActions.loadMoviesSuccess({
          movies: [movie1],
          page: {
            pageNumber: 1,
            pageSize: 20,
            totalCount: 1,
            totalPages: 1,
            hasPrevious: false,
            hasNext: false,
          },
        }),
      );
    });

    it('should keep the paging metadata the API returns', async () => {
      // The list needs to know whether another page exists; counting the
      // rows on screen cannot answer that when the last page is short.
      movieService.search.mockReturnValue(
        of({
          ...pagedResponse([movie1]),
          pageNumber: 2,
          totalCount: 27,
          totalPages: 3,
          hasPrevious: true,
          hasNext: true,
        }),
      );
      actions$ = of(MovieActions.loadMovies({}));

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(
        MovieActions.loadMoviesSuccess({
          movies: [movie1],
          page: {
            pageNumber: 2,
            pageSize: 20,
            totalCount: 27,
            totalPages: 3,
            hasPrevious: true,
            hasNext: true,
          },
        }),
      );
    });

    it('should dispatch loadMoviesFailure on service error', async () => {
      movieService.search.mockReturnValue(throwError(() => new Error('Network error')));
      actions$ = of(MovieActions.loadMovies({}));

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(
        MovieActions.loadMoviesFailure({ error: 'Network error' }),
      );
    });
  });

  describe('addMovie$', () => {
    it('should dispatch addMovieSuccess with saved movie', async () => {
      movieService.create.mockReturnValue(of(movie1));
      actions$ = of(MovieActions.addMovie({
        movie: {
          title: 'The Matrix',
          releaseYear: 1999,
          favourite: false,
          criterion: false,
        },
      }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(MovieActions.addMovieSuccess({ movie: movie1 }));
    });

    it('should dispatch addMovieFailure on service error', async () => {
      movieService.create.mockReturnValue(throwError(() => new Error('Save failed')));
      actions$ = of(MovieActions.addMovie({
        movie: {
          title: 'The Matrix',
          releaseYear: 1999,
          favourite: false,
          criterion: false,
        },
      }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(
        MovieActions.addMovieFailure({ error: 'Save failed' }),
      );
    });
  });

  describe('updateMovie$', () => {
    it('should send the changes and read the movie back', async () => {
      // The API answers an update with 204 No Content, so the store is
      // updated from the entity the server returns, not from the form.
      const updated = { ...movie1, title: 'The Matrix Reloaded' };
      movieService.update.mockReturnValue(of(void 0));
      movieService.getById.mockReturnValue(of(updated));
      actions$ = of(
        MovieActions.updateMovie({
          id: movie1.id,
          changes: { title: 'The Matrix Reloaded' },
        }),
      );

      const action = await firstValueFrom(effects.updateMovie$);
      expect(movieService.update).toHaveBeenCalledWith(movie1.id, {
        title: 'The Matrix Reloaded',
      });
      expect(action).toEqual(MovieActions.updateMovieSuccess({ movie: updated }));
    });

    it('should dispatch updateMovieFailure on service error', async () => {
      movieService.update.mockReturnValue(throwError(() => new Error('Update failed')));
      actions$ = of(MovieActions.updateMovie({ id: movie1.id, changes: {} }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(
        MovieActions.updateMovieFailure({ error: 'Update failed' }),
      );
    });

    it('should dispatch updateMovieFailure when the read-back fails', async () => {
      movieService.update.mockReturnValue(of(void 0));
      movieService.getById.mockReturnValue(throwError(() => new Error('Read failed')));
      actions$ = of(MovieActions.updateMovie({ id: movie1.id, changes: {} }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(
        MovieActions.updateMovieFailure({ error: 'Read failed' }),
      );
    });
  });

  describe('removeMovie$', () => {
    it('should dispatch removeMovieSuccess with the removed id', async () => {
      movieService.remove.mockReturnValue(of(void 0));
      actions$ = of(MovieActions.removeMovie({ id: '00000000-0000-0000-0000-000000000001' }));

      const action = await firstValueFrom(effects.removeMovie$);
      expect(action).toEqual(MovieActions.removeMovieSuccess({ id: '00000000-0000-0000-0000-000000000001' }));
    });

    it('should dispatch removeMovieFailure on service error', async () => {
      movieService.remove.mockReturnValue(throwError(() => new Error('Delete failed')));
      actions$ = of(MovieActions.removeMovie({ id: '00000000-0000-0000-0000-000000000001' }));

      const action = await firstValueFrom(effects.removeMovie$);
      expect(action).toEqual(
        MovieActions.removeMovieFailure({ error: 'Delete failed' }),
      );
    });
  });
});
