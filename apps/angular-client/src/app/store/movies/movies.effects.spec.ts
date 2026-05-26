import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';

import { MovieEffects } from './movies.effects';
import { MovieActions } from './movies.actions';
import { MovieService } from '../../../services/movie-service';
import { Movie } from '../../../models/movie';
import { MediaType } from '../../../models/media-type';
import { MovieFormat } from '../../../models/movie-format';

const movie1: Movie = {
  Id: 1, Title: 'The Matrix', Description: '', Genre: 'Sci-Fi',
  Criterion: false, CriterionNumber: undefined,
  MovieMediaType: MediaType.BluRay, MovieFormat: MovieFormat.Movie,
  Year: 1999, Favourite: false,
};

describe('MovieEffects', () => {
  let actions$: Observable<Action>;
  let effects: MovieEffects;
  let movieService: { [K in keyof MovieService]: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    movieService = {
      getAllMovies: vi.fn(),
      getMovie: vi.fn(),
      addMovie: vi.fn(),
      updateMovie: vi.fn(),
      removeMovie: vi.fn(),
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
      movieService.getAllMovies.mockReturnValue(of({ items: [movie1], total: 1 }));
      actions$ = of(MovieActions.loadMovies());

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(MovieActions.loadMoviesSuccess({ movies: [movie1] }));
    });

    it('should dispatch loadMoviesFailure on service error', async () => {
      movieService.getAllMovies.mockReturnValue(throwError(() => new Error('Network error')));
      actions$ = of(MovieActions.loadMovies());

      const action = await firstValueFrom(effects.loadMovies$);
      expect(action).toEqual(
        MovieActions.loadMoviesFailure({ error: 'Error: Network error' }),
      );
    });
  });

  describe('addMovie$', () => {
    it('should dispatch addMovieSuccess with saved movie', async () => {
      const newMovie = { ...movie1, Id: undefined as any };
      movieService.addMovie.mockReturnValue(of(movie1));
      actions$ = of(MovieActions.addMovie({ movie: newMovie }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(MovieActions.addMovieSuccess({ movie: movie1 }));
    });

    it('should dispatch addMovieFailure on service error', async () => {
      movieService.addMovie.mockReturnValue(throwError(() => new Error('Save failed')));
      actions$ = of(MovieActions.addMovie({ movie: movie1 }));

      const action = await firstValueFrom(effects.addMovie$);
      expect(action).toEqual(
        MovieActions.addMovieFailure({ error: 'Error: Save failed' }),
      );
    });
  });

  describe('updateMovie$', () => {
    it('should dispatch updateMovieSuccess with updated movie', async () => {
      const updated = { ...movie1, Title: 'The Matrix Reloaded' };
      movieService.updateMovie.mockReturnValue(of(updated));
      actions$ = of(MovieActions.updateMovie({ movie: updated }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(MovieActions.updateMovieSuccess({ movie: updated }));
    });

    it('should dispatch updateMovieFailure on service error', async () => {
      movieService.updateMovie.mockReturnValue(throwError(() => new Error('Update failed')));
      actions$ = of(MovieActions.updateMovie({ movie: movie1 }));

      const action = await firstValueFrom(effects.updateMovie$);
      expect(action).toEqual(
        MovieActions.updateMovieFailure({ error: 'Error: Update failed' }),
      );
    });
  });

  describe('removeMovie$', () => {
    it('should dispatch removeMovieSuccess with the removed id', async () => {
      movieService.removeMovie.mockReturnValue(of(void 0));
      actions$ = of(MovieActions.removeMovie({ id: 1 }));

      const action = await firstValueFrom(effects.removeMovie$);
      expect(action).toEqual(MovieActions.removeMovieSuccess({ id: 1 }));
    });

    it('should dispatch removeMovieFailure on service error', async () => {
      movieService.removeMovie.mockReturnValue(throwError(() => new Error('Delete failed')));
      actions$ = of(MovieActions.removeMovie({ id: 1 }));

      const action = await firstValueFrom(effects.removeMovie$);
      expect(action).toEqual(
        MovieActions.removeMovieFailure({ error: 'Error: Delete failed' }),
      );
    });
  });
});
