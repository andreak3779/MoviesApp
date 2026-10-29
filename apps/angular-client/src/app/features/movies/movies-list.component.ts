import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MovieActions } from '../../store/movies/movies.actions';
import {
  selectAllMovies,
  selectMoviesError,
  selectMoviesLoading,
  selectMoviesHasNextPage,
  selectMoviesTotalPages,
} from '../../store/movies/movies.selectors';
import { LookupService } from '../../core/services/lookup.service';
import { DeleteConfirmationComponent } from '../shared/delete-confirmation.component';
import { Movie } from '../../models/movie';

interface DeleteTarget {
  id: string;
  title: string;
}

@Component({
  selector: 'app-movies-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, DeleteConfirmationComponent],
  template: `
    <header class="bar">
      <h1>Movies</h1>
      <button type="button" class="primary" (click)="openAdd()">+ Add Movie</button>
    </header>

    <section class="filters">
      <input
        type="search"
        placeholder="Search movies…"
        [ngModel]="search()"
        (ngModelChange)="onSearchChange($event)"
        aria-label="Search movies"
      />

      <select [ngModel]="filterGenre()" (ngModelChange)="filterGenre.set($event); reload()">
        <option value="">All genres</option>
        @for (g of lookup.filmGenres(); track g.id) {
          <option [value]="g.name">{{ g.name }}</option>
        }
      </select>

      <select [ngModel]="filterFormat()" (ngModelChange)="filterFormat.set($event); reload()">
        <option value="">All formats</option>
        @for (f of lookup.formats(); track f.id) {
          <option [value]="f.name">{{ f.name }}</option>
        }
      </select>

      <select [ngModel]="filterMediaType()" (ngModelChange)="filterMediaType.set($event); reload()">
        <option value="">All media types</option>
        @for (m of lookup.mediaTypes(); track m.id) {
          <option [value]="m.name">{{ m.name }}</option>
        }
      </select>

      <select [ngModel]="sortBy()" (ngModelChange)="sortBy.set($event); reload()">
        <option value="year">Sort: Year</option>
        <option value="title">Sort: Title</option>
      </select>
    </section>

    @if (loading()) {
      <p class="status">Loading…</p>
    } @else if (error()) {
      <p class="status error">{{ error() }}</p>
    } @else if (movies().length === 0) {
      <p class="empty">No movies match your filters.</p>
    } @else {
      <ul class="grid" data-testid="movies-grid">
        @for (m of movies(); track m.id) {
          <li class="card" [attr.data-id]="m.id">
            <div class="card-header">
              <div class="tile" [style.background]="tileColor(m.id)">{{ tileLetter(m.title) }}</div>
              @if (m.criterion) {
                <span class="criterion">CRITERION #{{ m.criterionNumber }}</span>
              }
            </div>
            <h3>{{ m.title }}</h3>
            <div class="year">{{ m.releaseYear }}</div>
            @if (m.description) {
              <p class="description">{{ m.description }}</p>
            }
            <div class="badges">
              @if (m.genre) { <span class="badge">{{ m.genre }}</span> }
              @if (m.favourite) { <span class="favourite">★ Favourite</span> }
            </div>
            <div class="actions">
              <button type="button" class="link" (click)="openEdit(m)">Edit</button>
              <button type="button" class="link danger" (click)="requestDelete(m)">Delete</button>
            </div>
          </li>
        }
      </ul>

      <footer class="paginator">
        <button type="button" class="secondary" [disabled]="page() <= 1" (click)="prevPage()">Previous</button>
        <span>Page {{ page() }} of {{ totalPages() }}</span>
        <button type="button" class="secondary" [disabled]="!hasNextPage()" (click)="nextPage()">Next</button>
      </footer>
    }

    <app-delete-confirmation
      [visible]="!!deleteTarget()"
      [title]="deleteTarget() ? 'Delete movie?' : ''"
      [message]="deleteMessage()"
      (confirm)="confirmDelete()"
      (cancel)="deleteTarget.set(null)"
    />
  `,
  styles: [`
    :host { display: block; padding: 0; }
    .bar { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; flex-wrap: wrap; gap: 16px; }
    .bar h1 { margin: 0; font-size: 28px; font-weight: 700; }
    .filters { display: flex; flex-wrap: wrap; gap: 12px; margin-bottom: 28px; }
    .filters input[type="search"] { flex: 1; min-width: 180px; }
    input, select {
      padding: 10px 14px;
      border-radius: 10px;
      border: 1px solid var(--surface-border, #e6e6e6);
      background: var(--surface, #fff);
      font-size: 14px;
      color: var(--ink, #222);
    }
    button {
      padding: 10px 22px;
      border-radius: 999px;
      border: none;
      background: var(--primary, #7a1256);
      color: white;
      font: 500 14px Roboto, sans-serif;
      cursor: pointer;
    }
    button.secondary { background: var(--surface, #fff); color: var(--ink, #222); border: 1px solid var(--surface-border, #e6e6e6); }
    button.link { background: none; padding: 0; color: var(--primary, #7a1256); font: 500 13px Roboto, sans-serif; }
    button.link.danger { color: var(--danger, #b03b3b); }
    button[disabled] { opacity: 0.5; cursor: not-allowed; }
    .status { color: var(--ink-secondary, #555); }
    .status.error { color: var(--danger, #b03b3b); }
    .empty { padding: 60px 20px; text-align: center; color: var(--ink-secondary, #555); border: 1px dashed var(--surface-border, #e6e6e6); border-radius: 16px; margin-bottom: 28px; }
    .grid {
      list-style: none;
      padding: 0;
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 20px;
      margin: 0 0 28px;
    }
    .card {
      background: var(--surface, #fff);
      border: 1px solid var(--surface-border, #e6e6e6);
      border-radius: 16px;
      padding: 20px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    .card-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; }
    .tile {
      width: 44px; height: 44px;
      border-radius: 12px;
      color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 700; font-size: 18px;
      flex-shrink: 0;
    }
    .criterion { font-size: 11px; font-weight: 700; letter-spacing: 0.03em; color: var(--tertiary, #6f3c9e); background: var(--tertiary-tint, #efe6fb); padding: 4px 8px; border-radius: 8px; white-space: nowrap; }
    h3 { margin: 0 0 4px; font-size: 16px; font-weight: 700; line-height: 1.3; }
    .year { font-size: 13px; color: var(--ink-secondary, #555); }
    .description { margin: 0; font-size: 13px; color: var(--ink-secondary, #555); line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
    .badges { display: flex; gap: 6px; flex-wrap: wrap; }
    .badge { font-size: 12px; padding: 4px 10px; border-radius: 8px; background: var(--primary-tint, #f4e3ee); color: var(--primary, #7a1256); font-weight: 500; }
    .favourite { font-size: 12px; padding: 4px 10px; border-radius: 8px; background: var(--danger-tint, #fbe6e6); color: var(--danger, #b03b3b); font-weight: 500; }
    .actions { display: flex; gap: 16px; margin-top: auto; padding-top: 10px; border-top: 1px solid var(--surface-border, #e6e6e6); }
    .paginator { display: flex; align-items: center; justify-content: center; gap: 16px; }
  `],
})
export class MoviesListComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly lookup = inject(LookupService);

  // Local UI state — signals
  readonly search = signal('');
  readonly filterGenre = signal('');
  readonly filterFormat = signal('');
  readonly filterMediaType = signal('');
  readonly sortBy = signal<'title' | 'year'>('year');
  readonly page = signal(1);
  readonly pageSize = 6;
  readonly deleteTarget = signal<DeleteTarget | null>(null);

  // Store-driven state
  readonly movies = this.store.selectSignal(selectAllMovies);
  readonly loading = this.store.selectSignal(selectMoviesLoading);
  readonly error = this.store.selectSignal(selectMoviesError);

  /**
   * Whether another page exists. The API says so in its paging metadata;
   * counting the rows on screen would be wrong whenever the last page is
   * short.
   */
  readonly hasNextPage = this.store.selectSignal(selectMoviesHasNextPage);
  readonly totalPages = this.store.selectSignal(selectMoviesTotalPages);

  private readonly searchInput$ = new Subject<string>();

  private readonly tileColors = [
    'oklch(40% 0.19 350)',
    'oklch(42% 0.17 320)',
    'oklch(44% 0.15 295)',
    'oklch(46% 0.14 270)',
    'oklch(43% 0.16 10)',
  ];

  readonly deleteMessage = computed(() => {
    const t = this.deleteTarget();
    return t ? `"${t.title}" will be permanently removed. This can't be undone.` : '';
  });

  ngOnInit(): void {
    this.store.dispatch(MovieActions.loadMovies({ query: this.snapshotQuery() }));
    void this.lookup.refreshAll();
    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.reload());
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.page.set(1);
    this.searchInput$.next(value);
  }

  reload(): void {
    this.page.set(1);
    this.store.dispatch(MovieActions.loadMovies({ query: this.snapshotQuery() }));
  }

  prevPage(): void {
    if (this.page() <= 1) return;
    this.page.update((p) => p - 1);
    this.store.dispatch(MovieActions.loadMovies({ query: this.snapshotQuery() }));
  }

  nextPage(): void {
    if (!this.hasNextPage()) return;
    this.page.update((p) => p + 1);
    this.store.dispatch(MovieActions.loadMovies({ query: this.snapshotQuery() }));
  }

  openAdd(): void {
    void this.router.navigate(['/movies', 'new']);
  }

  openEdit(movie: Movie): void {
    void this.router.navigate(['/movies', movie.id, 'edit']);
  }

  requestDelete(movie: Movie): void {
    this.deleteTarget.set({ id: movie.id, title: movie.title });
  }

  confirmDelete(): void {
    const target = this.deleteTarget();
    if (!target) return;
    this.store.dispatch(MovieActions.removeMovie({ id: target.id }));
    this.deleteTarget.set(null);
  }

  tileLetter(title: string): string {
    return (title?.charAt(0) || '?').toUpperCase();
  }

  tileColor(id: string): string {
    let hash = 0;
    for (let i = 0; i < id.length; i++) hash = (hash + id.charCodeAt(i)) | 0;
    return this.tileColors[Math.abs(hash) % this.tileColors.length];
  }

  private snapshotQuery() {
    return {
      search: this.search() || undefined,
      genre: this.filterGenre() || undefined,
      format: this.filterFormat() || undefined,
      mediaType: this.filterMediaType() || undefined,
      sortBy: this.sortBy(),
      pageNumber: this.page(),
      pageSize: this.pageSize,
    };
  }
}
