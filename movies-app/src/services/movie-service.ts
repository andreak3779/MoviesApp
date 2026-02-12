import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay, map } from 'rxjs/operators';
import { Movie } from '../models/movie';
import { MediaType } from '../models/media-type';
import { MovieFormat } from '../models/movie-format';

@Injectable({
  providedIn: 'root',
})
export class MovieService {
  private readonly _movies$ = new BehaviorSubject<Movie[]>(this.getInitialMovies());

  // Return a snapshot observable of movies
  get movies$(): Observable<Movie[]> {
    return this._movies$.asObservable();
  }

  // Create (post)
  addMovie(movie: Omit<Movie, 'Id'>): Observable<Movie> {
    const current = [...this._movies$.value];
    const nextId = current.length ? Math.max(...current.map((m) => m.Id)) + 1 : 1;
    const newMovie: Movie = { ...movie, Id: nextId };
    current.push(newMovie);
    this._movies$.next(current);
    return of(newMovie).pipe(delay(200));
  }

  // Read single (get)
  getMovie(id: number): Observable<Movie | undefined> {
    const found = this._movies$.value.find((m) => m.Id === id);
    return of(found).pipe(delay(150));
  }

  // Read all with simple paging
  getAllMovies(page = 1, pageSize = 10): Observable<{ items: Movie[]; total: number }> {
    const all = this._movies$.value;
    const total = all.length;
    const start = (page - 1) * pageSize;
    const items = all.slice(start, start + pageSize);
    return of({ items, total }).pipe(delay(200));
  }

  // Get favourites
  getFavouriteMovies(): Observable<Movie[]> {
    return of(this._movies$.value.filter((m) => !!m.Favourite)).pipe(delay(150));
  }

  // Query movies by partial criteria
  queryMovies(query: Partial<{
    Title: string;
    Genre: string;
    Criterion: boolean;
    CriterionNumber: number;
    Year: number;
    Favourite: boolean;
  }>): Observable<Movie[]> {
    const results = this._movies$.value.filter((m) => {
      if (query.Title && !m.Title.toLowerCase().includes(query.Title.toLowerCase())) {
        return false;
      }
      if (query.Genre && m.Genre !== query.Genre) {
        return false;
      }
      if (typeof query.Criterion === 'boolean' && m.Criterion !== query.Criterion) {
        return false;
      }
      if (typeof query.CriterionNumber === 'number' && m.CriterionNumber !== query.CriterionNumber) {
        return false;
      }
      if (typeof query.Year === 'number' && m.Year !== query.Year) {
        return false;
      }
      if (typeof query.Favourite === 'boolean' && m.Favourite !== query.Favourite) {
        return false;
      }
      return true;
    });
    return of(results).pipe(delay(200));
  }

  // Update entire movie (put)
  updateMovie(movie: Movie): Observable<Movie> {
    const current = [...this._movies$.value];
    const idx = current.findIndex((m) => m.Id === movie.Id);
    if (idx === -1) {
      return of(movie).pipe(delay(150));
    }
    current[idx] = { ...movie };
    this._movies$.next(current);
    return of(current[idx]).pipe(delay(150));
  }

  // Change Criterion (patch)
  changeCriterion(id: number, criterion: boolean, criterionNumber?: number): Observable<Movie | undefined> {
    const current = [...this._movies$.value];
    const idx = current.findIndex((m) => m.Id === id);
    if (idx === -1) {
      return of(undefined).pipe(delay(150));
    }
    current[idx] = { ...current[idx], Criterion: criterion, CriterionNumber: criterionNumber };
    this._movies$.next(current);
    return of(current[idx]).pipe(delay(150));
  }

  // Change Favourite (patch)
  changeFavourite(id: number, favourite: boolean): Observable<Movie | undefined> {
    const current = [...this._movies$.value];
    const idx = current.findIndex((m) => m.Id === id);
    if (idx === -1) {
      return of(undefined).pipe(delay(150));
    }
    current[idx] = { ...current[idx], Favourite: favourite };
    this._movies$.next(current);
    return of(current[idx]).pipe(delay(150));
  }

  // Delete (remove movie)
  removeMovie(id: number): Observable<void> {
    const current = this._movies$.value.filter((m) => m.Id !== id);
    this._movies$.next(current);
    return of(void 0).pipe(delay(150));
  }

  // Initial mock data (used as "mockoon" responses substitute)
  private getInitialMovies(): Movie[] {
    return [
      {
        Title: 'The Matrix',
        Description: 'A computer hacker learns about the true nature of reality.',
        Id: 1,
        Genre: 'Sci-Fi',
        Criterion: false,
        CriterionNumber: undefined,
        MovieMediaType: MediaType.BluRay,
        MovieFormat: MovieFormat.Movie,
        Year: 1999,
        Favourite: false,
      },
      {
        Title: 'Dark',
        Description: 'A family saga with a supernatural twist.',
        Id: 2,
        Genre: 'Thriller',
        Criterion: true,
        CriterionNumber: 1,
        MovieMediaType: MediaType.UltraHd,
        MovieFormat: MovieFormat.TVSeries,
        Year: 2017,
        Favourite: true,
      },
      {
        Title: 'The Lord of the Rings Collection',
        Description: 'Epic fantasy box set.',
        Id: 3,
        Genre: 'Fantasy',
        Criterion: false,
        CriterionNumber: undefined,
        MovieMediaType: MediaType.Dvd,
        MovieFormat: MovieFormat.BoxSet,
        Year: 2003,
        Favourite: false,
      },
    ];
  }
}
