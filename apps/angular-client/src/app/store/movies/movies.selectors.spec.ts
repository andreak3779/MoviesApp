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
      const result = selectSelectedMovie(buildState({ selectedId: 2 }));
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
      expect(result[0].Id).toBe(2);
    });
  });
});
