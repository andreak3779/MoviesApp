import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { Movie } from '../../../models/movie';
import { MovieActions } from './movies.actions';

export interface MoviesState extends EntityState<Movie> {
  selectedId: number | null;
  loading: boolean;
  error: string | null;
}

export const adapter: EntityAdapter<Movie> = createEntityAdapter<Movie>({
  selectId: (movie) => movie.Id,
});

export const initialState: MoviesState = adapter.getInitialState({
  selectedId: null,
  loading: false,
  error: null,
});

export const moviesReducer = createReducer(
  initialState,

  on(MovieActions.loadMovies, (state) => ({ ...state, loading: true, error: null })),
  on(MovieActions.loadMoviesSuccess, (state, { movies }) =>
    adapter.setAll(movies, { ...state, loading: false }),
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
