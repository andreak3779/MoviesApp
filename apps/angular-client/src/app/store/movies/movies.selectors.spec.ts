import { adapter, initialState, MoviesState } from './movies.reducer';
import {
  selectAllMovies,
  selectMoviesLoading,
  selectMoviesError,
  selectMoviesTotal,
  selectSelectedMovie,
  selectFavouriteMovies,
} from './movies.selectors';
import { AppState } from '../../store';
import { Movie } from '../../models/movie';

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

const movie2: Movie = {
  ...movie1,
  Id: '00000000-0000-0000-0000-000000000002',
  Title: 'Dark',
  ReleaseYear: 2017,
  Genre: 'Thriller',
  Format: 'TVSeries',
  Criterion: true,
  CriterionNumber: 1,
  Favourite: true,
};

function buildState(overrides: Partial<MoviesState> = {}): AppState {
  return {
    movies: { ...adapter.setAll([movie1, movie2], initialState), ...overrides },
  } as AppState;
}

describe('Movies Selectors', () => {
  describe('selectAllMovies', () => {
    it('should return all movies as an array', () => {
      const result = selectAllMovies(buildState());
      expect(result).toHaveLength(2);
      expect(result[0]).toEqual(movie1);
    });
  });

  describe('selectMoviesTotal', () => {
    it('should return the count of movies', () => {
      expect(selectMoviesTotal(buildState())).toBe(2);
    });
  });

  describe('selectMoviesLoading', () => {
    it('should return the loading flag', () => {
      expect(selectMoviesLoading(buildState({ loading: true }))).toBe(true);
      expect(selectMoviesLoading(buildState({ loading: false }))).toBe(false);
    });
  });

  describe('selectMoviesError', () => {
    it('should return the error message', () => {
      expect(selectMoviesError(buildState({ error: 'oops' }))).toBe('oops');
      expect(selectMoviesError(buildState())).toBeNull();
    });
  });

  describe('selectSelectedMovie', () => {
    it('should return the movie matching selectedId', () => {
      const result = selectSelectedMovie(buildState({ selectedId: '00000000-0000-0000-0000-000000000002' }));
      expect(result).toEqual(movie2);
    });

    it('should return undefined when nothing is selected', () => {
      const result = selectSelectedMovie(buildState({ selectedId: null }));
      expect(result).toBeUndefined();
    });
  });

  describe('selectFavouriteMovies', () => {
    it('should return only movies where Favourite is true', () => {
      const result = selectFavouriteMovies(buildState());
      expect(result).toHaveLength(1);
      expect(result[0].Id).toBe('00000000-0000-0000-0000-000000000002');
    });
  });
});
