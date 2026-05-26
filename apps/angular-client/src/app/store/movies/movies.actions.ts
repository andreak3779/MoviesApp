import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Movie } from '../../../models/movie';

export const MovieActions = createActionGroup({
  source: 'Movies',
  events: {
    'Load Movies': emptyProps(),
    'Load Movies Success': props<{ movies: Movie[] }>(),
    'Load Movies Failure': props<{ error: string }>(),

    'Select Movie': props<{ id: number }>(),
    'Clear Selected Movie': emptyProps(),

    'Add Movie': props<{ movie: Omit<Movie, 'Id'> }>(),
    'Add Movie Success': props<{ movie: Movie }>(),
    'Add Movie Failure': props<{ error: string }>(),

    'Update Movie': props<{ movie: Movie }>(),
    'Update Movie Success': props<{ movie: Movie }>(),
    'Update Movie Failure': props<{ error: string }>(),

    'Remove Movie': props<{ id: number }>(),
    'Remove Movie Success': props<{ id: number }>(),
    'Remove Movie Failure': props<{ error: string }>(),
  },
});
