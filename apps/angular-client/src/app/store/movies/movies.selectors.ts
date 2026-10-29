import { createFeatureSelector, createSelector } from '@ngrx/store';
import { MoviesState, adapter } from './movies.reducer';

const { selectAll, selectEntities, selectTotal } = adapter.getSelectors();

export const selectMoviesState = createFeatureSelector<MoviesState>('movies');

export const selectAllMovies = createSelector(selectMoviesState, selectAll);
export const selectMovieEntities = createSelector(selectMoviesState, selectEntities);
export const selectMoviesTotal = createSelector(selectMoviesState, selectTotal);
export const selectMoviesLoading = createSelector(selectMoviesState, (s) => s.loading);
export const selectMoviesError = createSelector(selectMoviesState, (s) => s.error);

export const selectMoviesPage = createSelector(selectMoviesState, (s) => s.page);
export const selectMoviesHasNextPage = createSelector(selectMoviesPage, (p) => p.hasNext);
export const selectMoviesHasPreviousPage = createSelector(selectMoviesPage, (p) => p.hasPrevious);
export const selectMoviesTotalPages = createSelector(selectMoviesPage, (p) => p.totalPages);
export const selectMoviesPageNumber = createSelector(selectMoviesPage, (p) => p.pageNumber);
export const selectMoviesTotalCount = createSelector(selectMoviesPage, (p) => p.totalCount);

export const selectSelectedMovieId = createSelector(selectMoviesState, (s) => s.selectedId);
export const selectSelectedMovie = createSelector(
  selectMovieEntities,
  selectSelectedMovieId,
  (entities, id) => (id != null ? entities[id] : undefined),
);

export const selectFavouriteMovies = createSelector(selectAllMovies, (movies) =>
  movies.filter((m) => !!m.favourite),
);
