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
  Id: '00000000-0000-0000-0000-000000000001',
  Title: 'The Matrix',
  ReleaseYear: 1999,
  Description: '',
  Favourite: false,
  Criterion: false,
  CriterionNumber: null,
  OwnerId: 'user-1',
  CreatedAt: '2026-01-01T00:00:00Z',
  UpdatedAt: null,
  Genre: 'Sci-Fi',
  MediaType: 'Blu-Ray',
  Format: 'Movie',
  FilmGenres: [],
  MediaTypes: [],
  Formats: [],
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
      expect(action).toEqual(MovieActions.loadMoviesSuccess({ movies: [movie1] }));
    });

    it('should dispatch loadMoviesFailure on service error', async () => {
      movieService.search.mockReturnValue(throwError(() => new Error('Network error')));
      actions$ = of(MovieActions.loadMovies({}));

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(
        MovieActions.loadMoviesFailure({ error: 'Error: Network error' }),
      );
    });
  });

  describe('addMovie$', () => {
    it('should dispatch addMovieSuccess with saved movie', async () => {
      movieService.create.mockReturnValue(of(movie1));
      actions$ = of(MovieActions.addMovie({
        movie: {
          Title: 'The Matrix',
          ReleaseYear: 1999,
          Favourite: false,
          Criterion: false,
        },
      }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(MovieActions.addMovieSuccess({ movie: movie1 }));
    });

    it('should dispatch addMovieFailure on service error', async () => {
      movieService.create.mockReturnValue(throwError(() => new Error('Save failed')));
      actions$ = of(MovieActions.addMovie({
        movie: {
          Title: 'The Matrix',
          ReleaseYear: 1999,
          Favourite: false,
          Criterion: false,
        },
      }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(
        MovieActions.addMovieFailure({ error: 'Error: Save failed' }),
      );
    });
  });

  describe('updateMovie$', () => {
    it('should dispatch updateMovieSuccess with updated movie', async () => {
      const updated = { ...movie1, Title: 'The Matrix Reloaded' };
      movieService.update.mockReturnValue(of(void 0));
      actions$ = of(MovieActions.updateMovie({ movie: updated }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(MovieActions.updateMovieSuccess({ movie: updated }));
    });

    it('should dispatch updateMovieFailure on service error', async () => {
      movieService.update.mockReturnValue(throwError(() => new Error('Update failed')));
      actions$ = of(MovieActions.updateMovie({ movie: movie1 }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(
        MovieActions.updateMovieFailure({ error: 'Error: Update failed' }),
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
        MovieActions.removeMovieFailure({ error: 'Error: Delete failed' }),
      );
    });
  });
});
