import { moviesReducer, initialState, adapter, initialPage } from './movies.reducer';
import { MovieActions } from './movies.actions';
import { Movie } from '../../models/movie';

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

const movie2: Movie = {
  ...movie1,
  id: '00000000-0000-0000-0000-000000000002',
  title: 'Dark',
  releaseYear: 2017,
  genre: 'Thriller',
  format: 'TVSeries',
  criterion: true,
  criterionNumber: 1,
  favourite: true,
};

const page = {
  pageNumber: 2,
  pageSize: 6,
  totalCount: 13,
  totalPages: 3,
  hasPrevious: true,
  hasNext: true,
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
        MovieActions.loadMovies({}),
      );
      expect(state.loading).toBe(true);
      expect(state.error).toBeNull();
    });
  });

  describe('loadMoviesSuccess', () => {
    it('should populate entities and clear loading', () => {
      const loaded = moviesReducer(
        { ...initialState, loading: true },
        MovieActions.loadMoviesSuccess({ movies: [movie1, movie2], page: initialPage }),
      );
      expect(loaded.loading).toBe(false);
      expect(loaded.ids).toEqual(['00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002']);
      expect(loaded.entities['00000000-0000-0000-0000-000000000001']).toEqual(movie1);
    });

    it('should store the paging metadata', () => {
      const loaded = moviesReducer(
        initialState,
        MovieActions.loadMoviesSuccess({ movies: [movie1], page }),
      );
      expect(loaded.page).toEqual(page);
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
      const state = moviesReducer(initialState, MovieActions.selectMovie({ id: '00000000-0000-0000-0000-000000000002' }));
      expect(state.selectedId).toBe('00000000-0000-0000-0000-000000000002');
    });

    it('should clear selectedId', () => {
      const withSelection = moviesReducer(initialState, MovieActions.selectMovie({ id: '00000000-0000-0000-0000-000000000002' }));
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
      expect(state.ids).toContain('00000000-0000-0000-0000-000000000001');
      expect(state.entities['00000000-0000-0000-0000-000000000001']).toEqual(movie1);
    });
  });

  describe('updateMovieSuccess', () => {
    it('should replace the existing movie', () => {
      const withMovie = adapter.addOne(movie1, initialState);
      const updated = { ...movie1, title: 'The Matrix Reloaded' };
      const state = moviesReducer(
        withMovie,
        MovieActions.updateMovieSuccess({ movie: updated }),
      );
      expect(state.entities['00000000-0000-0000-0000-000000000001']!.title).toBe('The Matrix Reloaded');
    });
  });

  describe('removeMovieSuccess', () => {
    it('should remove the movie from the collection', () => {
      const withMovies = adapter.setAll([movie1, movie2], initialState);
      const state = moviesReducer(
        withMovies,
        MovieActions.removeMovieSuccess({ id: '00000000-0000-0000-0000-000000000001' }),
      );
      expect(state.ids).not.toContain('00000000-0000-0000-0000-000000000001');
      expect(state.ids).toContain('00000000-0000-0000-0000-000000000002');
    });
  });
});
