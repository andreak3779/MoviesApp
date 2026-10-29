import { adapter, initialState, MoviesState } from './movies.reducer';
import {
  selectAllMovies,
  selectMoviesLoading,
  selectMoviesError,
  selectMoviesTotal,
  selectSelectedMovie,
  selectFavouriteMovies,
  selectMoviesHasNextPage,
  selectMoviesTotalPages,
  selectMoviesPageNumber,
  selectMoviesTotalCount,
} from './movies.selectors';
import { AppState } from '../../store';
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

  describe('paging selectors', () => {
    const paged = {
      pageNumber: 2,
      pageSize: 6,
      totalCount: 13,
      totalPages: 3,
      hasPrevious: true,
      hasNext: true,
    };

    it('should report that another page exists', () => {
      expect(selectMoviesHasNextPage(buildState({ page: paged }))).toBe(true);
      expect(
        selectMoviesHasNextPage(buildState({ page: { ...paged, hasNext: false } })),
      ).toBe(false);
    });

    it('should report the total number of pages', () => {
      expect(selectMoviesTotalPages(buildState({ page: paged }))).toBe(3);
    });

    it('should report the current page number', () => {
      expect(selectMoviesPageNumber(buildState({ page: paged }))).toBe(2);
    });

    it('should report the total number of movies', () => {
      expect(selectMoviesTotalCount(buildState({ page: paged }))).toBe(13);
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
    it('should return only movies where favourite is true', () => {
      const result = selectFavouriteMovies(buildState());
      expect(result).toHaveLength(1);
      expect(result[0].id).toBe('00000000-0000-0000-0000-000000000002');
    });
  });
});
