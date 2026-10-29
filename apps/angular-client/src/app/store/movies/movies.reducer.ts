import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { Movie, MoviePage } from '../../models/movie';
import { MovieActions } from './movies.actions';

export interface MoviesState extends EntityState<Movie> {
  selectedId: string | null;
  loading: boolean;
  error: string | null;

  /**
   * The paging metadata of the last loaded page. The API returns it with
   * every response; without it the list can only guess whether another page
   * exists by counting the rows it happens to have.
   */
  page: MoviePage;
}

export const adapter: EntityAdapter<Movie> = createEntityAdapter<Movie>({
  selectId: (movie) => movie.id,
});

export const initialPage: MoviePage = {
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
  hasPrevious: false,
  hasNext: false,
};

export const initialState: MoviesState = adapter.getInitialState({
  selectedId: null,
  loading: false,
  error: null,
  page: initialPage,
});

export const moviesReducer = createReducer(
  initialState,

  on(MovieActions.loadMovies, (state) => ({ ...state, loading: true, error: null })),
  on(MovieActions.loadMoviesSuccess, (state, { movies, page }) =>
    adapter.setAll(movies, { ...state, loading: false, page }),
  ),
  on(MovieActions.loadMoviesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(MovieActions.selectMovie, (state, { id }) => ({ ...state, selectedId: id })),
  on(MovieActions.clearSelectedMovie, (state) => ({ ...state, selectedId: null })),

  on(MovieActions.addMovieSuccess, (state, { movie }) => adapter.addOne(movie, state)),
  on(MovieActions.addMovieFailure, (state, { error }) => ({ ...state, error })),

  on(MovieActions.updateMovieSuccess, (state, { movie }) =>
    adapter.upsertOne(movie, state),
  ),
  on(MovieActions.updateMovieFailure, (state, { error }) => ({ ...state, error })),

  on(MovieActions.removeMovieSuccess, (state, { id }) => adapter.removeOne(id, state)),
  on(MovieActions.removeMovieFailure, (state, { error }) => ({ ...state, error })),
);
