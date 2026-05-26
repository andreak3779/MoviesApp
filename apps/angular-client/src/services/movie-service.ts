import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { Movie } from '../models/movie';
import { MediaType } from '../models/media-type';
import { MovieFormat } from '../models/movie-format';

@Injectable({ providedIn: 'root' })
export class MovieService {
  private db: Movie[] = [
    {
      Id: 1,
      Title: 'The Matrix',
      Description: 'A computer hacker learns about the true nature of reality.',
      Genre: 'Sci-Fi',
      Criterion: false,
      CriterionNumber: undefined,
      MovieMediaType: MediaType.BluRay,
      MovieFormat: MovieFormat.Movie,
      Year: 1999,
      Favourite: false,
    },
    {
      Id: 2,
      Title: 'Dark',
      Description: 'A family saga with a supernatural twist.',
      Genre: 'Thriller',
      Criterion: true,
      CriterionNumber: 1,
      MovieMediaType: MediaType.UltraHd,
      MovieFormat: MovieFormat.TVSeries,
      Year: 2017,
      Favourite: true,
    },
    {
      Id: 3,
      Title: 'The Lord of the Rings Collection',
      Description: 'Epic fantasy box set.',
      Genre: 'Fantasy',
      Criterion: false,
      CriterionNumber: undefined,
      MovieMediaType: MediaType.Dvd,
      MovieFormat: MovieFormat.BoxSet,
      Year: 2003,
      Favourite: false,
    },
  ];

  private nextId = 4;

  getAllMovies(page = 1, pageSize = 10): Observable<{ items: Movie[]; total: number }> {
    const total = this.db.length;
    const items = this.db.slice((page - 1) * pageSize, (page - 1) * pageSize + pageSize);
    return of({ items, total }).pipe(delay(200));
  }

  getMovie(id: number): Observable<Movie | undefined> {
    return of(this.db.find((m) => m.Id === id)).pipe(delay(150));
  }

  addMovie(movie: Omit<Movie, 'Id'>): Observable<Movie> {
    const saved: Movie = { ...movie, Id: this.nextId++ };
    this.db = [...this.db, saved];
    return of(saved).pipe(delay(200));
  }

  updateMovie(movie: Movie): Observable<Movie> {
    this.db = this.db.map((m) => (m.Id === movie.Id ? { ...movie } : m));
    return of({ ...movie }).pipe(delay(150));
  }

  removeMovie(id: number): Observable<void> {
    this.db = this.db.filter((m) => m.Id !== id);
    return of(void 0).pipe(delay(150));
  }
}
