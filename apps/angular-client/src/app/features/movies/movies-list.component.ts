import { Component, inject, OnInit } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Store } from '@ngrx/store';
import { MovieActions } from '../../store/movies/movies.actions';
import {
  selectAllMovies,
  selectMoviesError,
  selectMoviesLoading,
} from '../../store/movies/movies.selectors';

@Component({
  selector: 'app-movies-list',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  template: `
    <div class="movies-list">
      <h1>Movies</h1>

      <div *ngIf="loading$ | async" class="status">Loading…</div>
      <div *ngIf="error$ | async as error" class="error">{{ error }}</div>

      <ul *ngIf="!(loading$ | async)">
        <li *ngFor="let movie of movies$ | async" class="movie-item">
          <strong>{{ movie.Title }}</strong> ({{ movie.Year }})
          <span class="genre">{{ movie.Genre }}</span>
          <span *ngIf="movie.Favourite" class="favourite">★</span>
          <button (click)="remove(movie.Id)">Remove</button>
        </li>
      </ul>
    </div>
  `,
  styles: [`
    .movies-list { padding: 1rem; }
    .movie-item { display: flex; gap: 1rem; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid #eee; }
    .genre { color: #666; font-size: 0.85rem; }
    .favourite { color: gold; }
    .error { color: red; }
    .status { color: #666; }
  `],
})
export class MoviesListComponent implements OnInit {
  private readonly store = inject(Store);

  movies$ = this.store.select(selectAllMovies);
  loading$ = this.store.select(selectMoviesLoading);
  error$ = this.store.select(selectMoviesError);

  ngOnInit(): void {
    this.store.dispatch(MovieActions.loadMovies());
  }

  remove(id: number): void {
    this.store.dispatch(MovieActions.removeMovie({ id }));
  }
}
