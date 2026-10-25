import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, filter, switchMap, take } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MovieActions } from '../../store/movies/movies.actions';
import {
  selectAllMovies,
} from '../../store/movies/movies.selectors';
import { LookupService } from '../../core/services/lookup.service';
import { MovieService } from '../../services/movie-service';
import { Movie } from '../../models/movie';

@Component({
  selector: 'app-movie-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="form-shell">
      <button type="button" class="back" (click)="cancel()">← Back to Movies</button>
      <h1>{{ isEdit() ? 'Edit Movie' : 'Add Movie' }}</h1>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <section class="card">
          <h2 class="section-title">Details</h2>

          <label>
            <span>Title</span>
            <input type="text" formControlName="title" />
          </label>
          @if (form.controls.title.touched && form.controls.title.invalid) {
            <p class="error">Title is required.</p>
          }

          <label>
            <span>Year</span>
            <input type="number" formControlName="year" min="1888" max="2100" />
          </label>
          @if (form.controls.year.touched && form.controls.year.invalid) {
            <p class="error">Year must be between 1888 and 2100.</p>
          }

          <label>
            <span>Description</span>
            <textarea formControlName="description" rows="3"></textarea>
          </label>
        </section>

        <section class="card">
          <h2 class="section-title">Classification</h2>

          <div class="row">
            <label class="grow">
              <span>Genre</span>
              <select formControlName="genre">
                <option [ngValue]="null">Select genre</option>
                @for (g of lookup.filmGenres(); track g.id) {
                  <option [ngValue]="g.id">{{ g.name }}</option>
                }
              </select>
            </label>
            <a class="manage" routerLink="/film-genres" (click)="$event.preventDefault()">Manage →</a>
          </div>

          <div class="row">
            <label class="grow">
              <span>Format</span>
              <select formControlName="format">
                <option [ngValue]="null">Select format</option>
                @for (f of lookup.formats(); track f.id) {
                  <option [ngValue]="f.id">{{ f.name }}</option>
                }
              </select>
            </label>
            <a class="manage" routerLink="/formats" (click)="$event.preventDefault()">Manage →</a>
          </div>

          <div class="row">
            <label class="grow">
              <span>Media Type</span>
              <select formControlName="mediaType">
                <option [ngValue]="null">Select media type</option>
                @for (m of lookup.mediaTypes(); track m.id) {
                  <option [ngValue]="m.id">{{ m.name }}</option>
                }
              </select>
            </label>
            <a class="manage" routerLink="/media-types" (click)="$event.preventDefault()">Manage →</a>
          </div>
        </section>

        <section class="card">
          <h2 class="section-title">Collection Info</h2>

          <label class="check">
            <input type="checkbox" formControlName="criterion" />
            Criterion Collection
          </label>

          @if (form.controls.criterion.value) {
            <label class="narrow">
              <span>Criterion Number</span>
              <input type="number" formControlName="criterionNumber" min="1" />
            </label>
            @if (form.controls.criterionNumber.touched && form.controls.criterionNumber.invalid) {
              <p class="error">Criterion number is required when Criterion Collection is checked.</p>
            }
          }

          <label class="check">
            <input type="checkbox" formControlName="favourite" />
            Favourite
          </label>
        </section>

        @if (submitError()) {
          <p class="error">{{ submitError() }}</p>
        }

        <div class="actions">
          <button type="button" class="secondary" (click)="cancel()">Cancel</button>
          <button type="submit" class="primary" [disabled]="form.invalid || submitting()">
            {{ submitting() ? 'Saving…' : 'Save Movie' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-shell { max-width: 620px; }
    .back { background: none; border: none; padding: 0; margin-bottom: 16px; color: var(--ink-secondary, #555); font: 500 13px Roboto, sans-serif; cursor: pointer; }
    h1 { margin: 0 0 28px; font-size: 26px; font-weight: 700; }
    .card { background: var(--surface, #fff); border: 1px solid var(--surface-border, #e6e6e6); border-radius: 16px; padding: 24px; margin-bottom: 20px; }
    .section-title { margin: 0 0 16px; font-size: 13px; font-weight: 700; color: var(--ink-secondary, #555); text-transform: uppercase; letter-spacing: 0.05em; }
    form { display: flex; flex-direction: column; gap: 0; }
    label { display: flex; flex-direction: column; gap: 6px; font-size: 13px; font-weight: 500; color: var(--ink-secondary, #555); margin-bottom: 16px; }
    label.grow { flex: 1; }
    label.narrow { max-width: 200px; }
    label.check { flex-direction: row; align-items: center; gap: 10px; font-size: 14px; color: var(--ink, #222); cursor: pointer; }
    .row { display: flex; align-items: flex-end; gap: 10px; }
    .manage { font-size: 13px; padding-bottom: 10px; color: var(--primary, #7a1256); text-decoration: none; }
    input, select, textarea {
      padding: 10px 14px;
      border-radius: 10px;
      border: 1px solid var(--surface-border, #e6e6e6);
      font-size: 14px;
      color: var(--ink, #222);
      background: var(--surface, #fff);
      font-family: inherit;
    }
    input[type="checkbox"] { width: 18px; height: 18px; accent-color: var(--primary, #7a1256); }
    .error { color: var(--danger, #b03b3b); font-size: 13px; margin: -8px 0 16px; }
    .actions { display: flex; gap: 12px; justify-content: flex-end; }
    button.primary, button.secondary {
      padding: 10px 22px;
      border-radius: 999px;
      border: none;
      font: 500 14px Roboto, sans-serif;
      cursor: pointer;
    }
    button.primary { background: var(--primary, #7a1256); color: white; }
    button.primary[disabled] { opacity: 0.5; cursor: not-allowed; }
    button.secondary { background: transparent; color: var(--ink, #222); border: 1px solid var(--surface-border, #e6e6e6); }
  `],
})
export class MovieFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly movieService = inject(MovieService);
  protected readonly lookup = inject(LookupService);

  readonly isEdit = signal(false);
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  // Reactive form
  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    year: [2024, [Validators.required, Validators.min(1888), Validators.max(2100)]],
    description: [''],
    genre: this.fb.control<number | null>(null),
    format: this.fb.control<number | null>(null),
    mediaType: this.fb.control<number | null>(null),
    criterion: [false],
    criterionNumber: this.fb.control<number | null>(null),
    favourite: [false],
  });

  private readonly reload$ = new Subject<void>();

  ngOnInit(): void {
    void this.lookup.refreshAll();

    this.route.paramMap
      .pipe(
        takeUntilDestroyed(),
        switchMap((pm) => {
          const id = pm.get('id');
          this.editingId.set(id ?? null);
          this.isEdit.set(!!id);
          if (!id) return [null];
          // Try the in-store cache first; fall back to HTTP.
          return this.store.select(selectAllMovies).pipe(
            take(1),
            switchMap((cached) => {
              const found = cached.find((m) => m.Id === id);
              if (found) return [found];
              return this.movieService.getById(id);
            }),
          );
        }),
        filter((m): m is Movie => !!m),
        takeUntilDestroyed(),
      )
      .subscribe((m) => this.populate(m));

    // Apply / remove the conditional criterionNumber validator.
    this.form.controls.criterion.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((enabled) => this.applyCriterionValidator(!!enabled));
  }

  private populate(m: Movie): void {
    this.form.patchValue({
      title: m.Title,
      year: m.ReleaseYear,
      description: m.Description ?? '',
      genre: m.FilmGenres[0]?.id ?? null,
      format: m.Formats[0]?.id ?? null,
      mediaType: m.MediaTypes[0]?.id ?? null,
      criterion: m.Criterion,
      criterionNumber: m.CriterionNumber ?? null,
      favourite: m.Favourite,
    });
  }

  private applyCriterionValidator(enabled: boolean): void {
    const ctrl = this.form.controls.criterionNumber;
    ctrl.clearValidators();
    if (enabled) {
      ctrl.setValidators([Validators.required, Validators.min(1)]);
    }
    ctrl.updateValueAndValidity();
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);
    this.submitError.set(null);
    const v = this.form.getRawValue();
    const payload = {
      Title: v.title,
      ReleaseYear: v.year,
      Description: v.description || null,
      Favourite: v.favourite,
      Criterion: v.criterion,
      CriterionNumber: v.criterion ? v.criterionNumber : null,
      FilmGenreId: v.genre,
      MediaTypeId: v.mediaType,
      FormatId: v.format,
    };
    if (this.isEdit() && this.editingId()) {
      this.store.dispatch(MovieActions.updateMovie({ movie: this.assembleFullMovie(payload) }));
    } else {
      this.store.dispatch(MovieActions.addMovie({ movie: payload }));
    }
    // Navigate optimistically. Errors show up via the form's submitError signal if needed.
    setTimeout(() => this.cancel(), 0);
  }

  cancel(): void {
    void this.router.navigate(['/movies']);
  }

  private assembleFullMovie(payload: object): Movie {
    // The reducer's update path uses the full Movie shape from the action; merge the
    // existing entity's collections with the new payload so we don't accidentally
    // wipe related collections.
    const existing = this.store.selectSignal(selectAllMovies)().find((m) => m.Id === this.editingId());
    const fallback: Movie = {
      Id: this.editingId() ?? '',
      Title: '',
      ReleaseYear: 0,
      Description: null,
      Favourite: false,
      Criterion: false,
      CriterionNumber: null,
      OwnerId: '',
      CreatedAt: new Date().toISOString(),
      UpdatedAt: null,
      Genre: null,
      MediaType: null,
      Format: null,
      FilmGenres: [],
      MediaTypes: [],
      Formats: [],
    };
    return { ...(existing ?? fallback), ...(payload as Partial<Movie>) };
  }
}
