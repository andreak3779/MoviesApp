import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Movie, MovieCreatePayload, MovieQuery } from '../../models/movie';

export const MovieActions = createActionGroup({
  source: 'Movies',
  events: {
    'Load Movies': props<{ query?: MovieQuery }>(),
    'Load Movies Success': props<{ movies: Movie[] }>(),
    'Load Movies Failure': props<{ error: string }>(),

    'Select Movie': props<{ id: string }>(),
    'Clear Selected Movie': emptyProps(),

    'Add Movie': props<{ movie: MovieCreatePayload }>(),
    'Add Movie Success': props<{ movie: Movie }>(),
    'Add Movie Failure': props<{ error: string }>(),

    'Update Movie': props<{ movie: Movie }>(),
    'Update Movie Success': props<{ movie: Movie }>(),
    'Update Movie Failure': props<{ error: string }>(),

    'Remove Movie': props<{ id: string }>(),
    'Remove Movie Success': props<{ id: string }>(),
    'Remove Movie Failure': props<{ error: string }>(),
  },
});
