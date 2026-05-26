import { moviesReducer, initialState, adapter } from './movies.reducer';
import { MovieActions } from './movies.actions';
import { Movie } from '../../../models/movie';
import { MediaType } from '../../../models/media-type';
import { MovieFormat } from '../../../models/movie-format';

const movie1: Movie = {
  Id: 1, Title: 'The Matrix', Description: '', Genre: 'Sci-Fi',
  Criterion: false, CriterionNumber: undefined,
  MovieMediaType: MediaType.BluRay, MovieFormat: MovieFormat.Movie,
  Year: 1999, Favourite: false,
};

const movie2: Movie = {
  Id: 2, Title: 'Dark', Description: '', Genre: 'Thriller',
  Criterion: true, CriterionNumber: 1,
  MovieMediaType: MediaType.UltraHd, MovieFormat: MovieFormat.TVSeries,
  Year: 2017, Favourite: true,
};

describe('moviesReducer', () => {
  it('should return initial state for unknown action', () => {
    const state = moviesReducer(undefined, { type: '@@INIT' } as any);
    expect(state).toEqual(initialState);
  });

  describe('loadMovies', () => {
    it('should set loading true and clear error', () => {
      const state = moviesReducer(
        { ...initialState, error: 'previous error' },
        MovieActions.loadMovies(),
      );
      expect(state.loading).toBe(true);
      expect(state.error).toBeNull();
    });
  });

  describe('loadMoviesSuccess', () => {
    it('should populate entities and clear loading', () => {
      const loaded = moviesReducer(
        { ...initialState, loading: true },
        MovieActions.loadMoviesSuccess({ movies: [movie1, movie2] }),
      );
      expect(loaded.loading).toBe(false);
      expect(loaded.ids).toEqual([1, 2]);
      expect(loaded.entities[1]).toEqual(movie1);
    });
  });

  describe('loadMoviesFailure', () => {
    it('should set error and clear loading', () => {
      const state = moviesReducer(
        { ...initialState, loading: true },
        MovieActions.loadMoviesFailure({ error: 'Network error' }),
      );
      expect(state.loading).toBe(false);
      expect(state.error).toBe('Network error');
    });
  });

  describe('selectMovie / clearSelectedMovie', () => {
    it('should set selectedId', () => {
      const state = moviesReducer(initialState, MovieActions.selectMovie({ id: 2 }));
      expect(state.selectedId).toBe(2);
    });

    it('should clear selectedId', () => {
      const withSelection = moviesReducer(initialState, MovieActions.selectMovie({ id: 2 }));
      const cleared = moviesReducer(withSelection, MovieActions.clearSelectedMovie());
      expect(cleared.selectedId).toBeNull();
    });
  });

  describe('addMovieSuccess', () => {
    it('should add the movie to the collection', () => {
      const state = moviesReducer(
        initialState,
        MovieActions.addMovieSuccess({ movie: movie1 }),
      );
      expect(state.ids).toContain(1);
      expect(state.entities[1]).toEqual(movie1);
    });
  });

  describe('updateMovieSuccess', () => {
    it('should replace the existing movie', () => {
      const withMovie = adapter.addOne(movie1, initialState);
      const updated = { ...movie1, Title: 'The Matrix Reloaded' };
      const state = moviesReducer(
        withMovie,
        MovieActions.updateMovieSuccess({ movie: updated }),
      );
      expect(state.entities[1]!.Title).toBe('The Matrix Reloaded');
    });
  });

  describe('removeMovieSuccess', () => {
    it('should remove the movie from the collection', () => {
      const withMovies = adapter.setAll([movie1, movie2], initialState);
      const state = moviesReducer(
        withMovies,
        MovieActions.removeMovieSuccess({ id: 1 }),
      );
      expect(state.ids).not.toContain(1);
      expect(state.ids).toContain(2);
    });
  });
});
