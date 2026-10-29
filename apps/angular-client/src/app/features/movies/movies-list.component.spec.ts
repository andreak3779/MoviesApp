import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideStore, Store } from '@ngrx/store';
import { signal } from '@angular/core';
import { MoviesListComponent } from './movies-list.component';
import { moviesReducer } from '../../store/movies/movies.reducer';
import { MovieActions } from '../../store/movies/movies.actions';
import { LookupService } from '../../core/services/lookup.service';
import { Movie, MoviePage } from '../../models/movie';

const movie1: Movie = {
  id: '00000000-0000-0000-0000-000000000001',
  title: 'The Matrix',
  releaseYear: 1999,
  description: 'A hacker learns the truth.',
  favourite: true,
  criterion: false,
  criterionNumber: null,
  ownerId: 'user-1',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
  genre: 'Science Fiction',
  mediaType: 'Blu-Ray',
  format: 'Movie',
  filmGenres: [],
  mediaTypes: [],
  formats: [],
};

const page: MoviePage = {
  pageNumber: 1,
  pageSize: 6,
  totalCount: 13,
  totalPages: 3,
  hasPrevious: false,
  hasNext: true,
};

describe('MoviesListComponent', () => {
  let fixture: ComponentFixture<MoviesListComponent>;
  let store: Store;

  const lookup = {
    filmGenres: signal([]),
    mediaTypes: signal([]),
    formats: signal([]),
    refreshAll: vi.fn().mockResolvedValue(undefined),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoviesListComponent],
      providers: [
        provideStore({ movies: moviesReducer }),
        provideRouter([]),
        { provide: LookupService, useValue: lookup },
      ],
    }).compileComponents();

    store = TestBed.inject(Store);
    fixture = TestBed.createComponent(MoviesListComponent);
  });

  /** Loads the page the component will render. */
  function load(movies: Movie[], paging: MoviePage = page): void {
    store.dispatch(MovieActions.loadMoviesSuccess({ movies, page: paging }));
    fixture.detectChanges();
  }

  it('should render a card for every movie', () => {
    fixture.detectChanges();
    load([movie1]);

    const cards = fixture.nativeElement.querySelectorAll('[data-testid="movies-grid"] .card');
    expect(cards.length).toBe(1);
    expect(cards[0].querySelector('h3').textContent).toContain('The Matrix');
  });

  it('should show the page and the number of pages', () => {
    fixture.detectChanges();
    load([movie1]);

    const paginator = fixture.nativeElement.querySelector('.paginator');
    expect(paginator.textContent).toContain('Page 1 of 3');
  });

  it('should enable Next when the API says there is another page', () => {
    fixture.detectChanges();
    load([movie1], { ...page, hasNext: true });

    const buttons = fixture.nativeElement.querySelectorAll('.paginator button');
    expect(buttons[1].disabled).toBe(false);
  });

  it('should disable Next when the API says there is no other page', () => {
    // Counting the rows on screen would get this wrong whenever the last
    // page is short, so the answer has to come from the API.
    fixture.detectChanges();
    load([movie1], { ...page, hasNext: false, totalPages: 1 });

    const buttons = fixture.nativeElement.querySelectorAll('.paginator button');
    expect(buttons[1].disabled).toBe(true);
  });

  it('should show an empty state when nothing matches', () => {
    fixture.detectChanges();
    load([]);

    expect(fixture.nativeElement.querySelector('.empty')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[data-testid="movies-grid"]')).toBeNull();
  });

  it('should ask the store to delete the movie that was confirmed', () => {
    fixture.detectChanges();
    load([movie1]);

    const dispatch = vi.spyOn(store, 'dispatch');

    const deleteButton = fixture.nativeElement.querySelector('.actions .danger');
    deleteButton.click();
    fixture.detectChanges();

    const confirmButton = fixture.nativeElement.querySelector('.dialog .danger');
    confirmButton.click();

    expect(dispatch).toHaveBeenCalledWith(MovieActions.removeMovie({ id: movie1.id }));
  });
});
