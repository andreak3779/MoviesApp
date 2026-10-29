import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { provideStore, Store } from '@ngrx/store';
import { signal } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import type { MockInstance } from 'vitest';
import { MovieFormComponent } from './movie-form.component';
import { moviesReducer } from '../../store/movies/movies.reducer';
import { MovieActions } from '../../store/movies/movies.actions';
import { LookupService } from '../../core/services/lookup.service';
import { MovieService } from '../../services/movie-service';
import { Movie } from '../../models/movie';

const existing: Movie = {
  id: 'movie-1',
  title: 'The Matrix',
  releaseYear: 1999,
  description: 'A hacker learns the truth.',
  favourite: true,
  criterion: true,
  criterionNumber: 3,
  ownerId: 'user-1',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
  genre: 'Science Fiction',
  mediaType: 'Blu-Ray',
  format: 'Movie',
  filmGenres: [{ id: 7, name: 'Science Fiction' }],
  mediaTypes: [{ id: 4, name: 'Blu-Ray' }],
  formats: [{ id: 2, name: 'Movie' }],
};

describe('MovieFormComponent', () => {
  let fixture: ComponentFixture<MovieFormComponent>;
  let store: Store;
  let router: Router;
  let params: BehaviorSubject<ReturnType<typeof convertToParamMap>>;
  let movieService: { getById: ReturnType<typeof vi.fn> };
  let dispatch: MockInstance<Store['dispatch']>;

  const lookup = {
    filmGenres: signal([]),
    mediaTypes: signal([]),
    formats: signal([]),
    refreshAll: vi.fn().mockResolvedValue(undefined),
  };

  beforeEach(async () => {
    params = new BehaviorSubject(convertToParamMap({}));
    movieService = { getById: vi.fn().mockReturnValue(of(existing)) };

    await TestBed.configureTestingModule({
      imports: [MovieFormComponent],
      providers: [
        provideStore({ movies: moviesReducer }),
        provideRouter([]),
        { provide: LookupService, useValue: lookup },
        { provide: MovieService, useValue: movieService },
        { provide: ActivatedRoute, useValue: { paramMap: params } },
      ],
    }).compileComponents();

    store = TestBed.inject(Store);
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(MovieFormComponent);
    dispatch = vi.spyOn(store, 'dispatch');
  });

  it('should ask the store to add the movie', () => {
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({
      title: 'Arrival',
      year: 2016,
      description: 'A linguist meets aliens.',
    });
    fixture.componentInstance.onSubmit();

    expect(dispatch).toHaveBeenCalledWith(
      MovieActions.addMovie({
        movie: {
          title: 'Arrival',
          releaseYear: 2016,
          description: 'A linguist meets aliens.',
          favourite: false,
          criterion: false,
          criterionNumber: null,
          filmGenreId: null,
          mediaTypeId: null,
          formatId: null,
        },
      }),
    );
  });

  it('should refuse to save an empty title', () => {
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({ title: '' });
    fixture.componentInstance.onSubmit();
    fixture.detectChanges();

    expect(dispatch).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Title is required.');
  });

  it('should require a criterion number once the criterion box is ticked', () => {
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({
      title: 'The Matrix',
      criterion: true,
      criterionNumber: null,
    });
    fixture.componentInstance.onSubmit();

    expect(dispatch).not.toHaveBeenCalled();
  });

  it('should send the criterion number with the movie', () => {
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({
      title: 'The Matrix',
      criterion: true,
      criterionNumber: 3,
    });
    fixture.componentInstance.onSubmit();

    expect(dispatch).toHaveBeenCalledWith(
      MovieActions.addMovie({
        movie: expect.objectContaining({ criterion: true, criterionNumber: 3 }),
      }),
    );
  });

  it('should load the movie and ask the store to update it', () => {
    params.next(convertToParamMap({ id: 'movie-1' }));
    fixture.detectChanges();

    expect(movieService.getById).toHaveBeenCalledWith('movie-1');
    expect(fixture.componentInstance.form.getRawValue().title).toBe('The Matrix');
    expect(fixture.componentInstance.form.getRawValue().year).toBe(1999);
    expect(fixture.componentInstance.form.getRawValue().genre).toBe(7);

    fixture.componentInstance.onSubmit();

    expect(dispatch).toHaveBeenCalledWith(
      MovieActions.updateMovie({
        id: 'movie-1',
        changes: expect.objectContaining({ title: 'The Matrix', releaseYear: 1999 }),
      }),
    );
  });

  it('should use the movie already in the store instead of asking the API', () => {
    store.dispatch(MovieActions.loadMoviesSuccess({ movies: [existing], page: {
      pageNumber: 1, pageSize: 6, totalCount: 1, totalPages: 1, hasPrevious: false, hasNext: false,
    } }));

    params.next(convertToParamMap({ id: 'movie-1' }));
    fixture.detectChanges();

    expect(movieService.getById).not.toHaveBeenCalled();
    expect(fixture.componentInstance.form.getRawValue().title).toBe('The Matrix');
  });

  it('should show the error when the save fails', () => {
    fixture.detectChanges();

    store.dispatch(MovieActions.addMovieFailure({ error: 'Could not save the movie.' }));
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('[data-testid="submit-error"]');
    expect(error.textContent).toContain('Could not save the movie.');
  });

  it('should go back to the list once the save succeeds', () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();

    store.dispatch(MovieActions.addMovieSuccess({ movie: existing }));
    fixture.detectChanges();

    expect(navigate).toHaveBeenCalledWith(['/movies']);
  });

  it('should go back to the list when the form is cancelled', () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();

    fixture.componentInstance.cancel();

    expect(navigate).toHaveBeenCalledWith(['/movies']);
  });
});
