import { ActionReducerMap } from '@ngrx/store';
import { RouterReducerState, routerReducer } from '@ngrx/router-store';
import { MoviesState, moviesReducer } from './movies/movies.reducer';

export interface AppState {
  movies: MoviesState;
  router: RouterReducerState;
}

export const reducers: ActionReducerMap<AppState> = {
  movies: moviesReducer,
  router: routerReducer,
};

