import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { LookupService } from '../../core/services/lookup.service';
import {
  LookupAdminService,
  LookupKind,
} from './lookup.service';

const LABELS: Record<LookupKind, { singular: string; plural: string; route: string }> = {
  filmGenres: { singular: 'Genre', plural: 'Genres', route: 'genres' },
  mediaTypes: { singular: 'Media Type', plural: 'Media Types', route: 'media-types' },
  formats: { singular: 'Format', plural: 'Formats', route: 'formats' },
};

/**
 * One form component per lookup kind. Supports add and edit via the
 * `:id` route param.
 */
@Component({
  selector: 'app-lookup-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="form-shell">
      <button type="button" class="back" (click)="cancel()">← Back to {{ title() }}</button>
      <h1>{{ formTitle() }}</h1>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="card">
          <label>
            <span>Name</span>
            <input type="text" formControlName="name" />
          </label>
          <label>
            <span>Description</span>
            <textarea formControlName="description" rows="3"></textarea>
          </label>
        </div>

        @if (submitError()) {
          <p class="error">{{ submitError() }}</p>
        }

        <div class="actions">
          <button type="button" class="secondary" (click)="cancel()">Cancel</button>
          <button type="submit" class="primary" [disabled]="form.invalid || submitting()">
            {{ submitting() ? 'Saving…' : 'Save ' + singular() }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-shell { max-width: 520px; }
    .back { background: none; border: none; padding: 0; margin-bottom: 16px; color: var(--ink-secondary, #555); font: 500 13px Roboto, sans-serif; cursor: pointer; }
    h1 { margin: 0 0 28px; font-size: 26px; font-weight: 700; }
    .card { background: var(--surface, #fff); border: 1px solid var(--surface-border, #e6e6e6); border-radius: 16px; padding: 24px; margin-bottom: 24px; }
    form { display: flex; flex-direction: column; gap: 0; }
    label { display: flex; flex-direction: column; gap: 6px; font-size: 13px; font-weight: 500; color: var(--ink-secondary, #555); margin-bottom: 16px; }
    input, textarea {
      padding: 10px 14px;
      border-radius: 10px;
      border: 1px solid var(--surface-border, #e6e6e6);
      font-size: 14px;
      color: var(--ink, #222);
      background: var(--surface, #fff);
      font-family: inherit;
    }
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
export class LookupFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly admin = inject(LookupAdminService);
  private readonly lookup = inject(LookupService);

  readonly kind = signal<LookupKind>('filmGenres');
  readonly editingId = signal<number | null>(null);
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly title = computed(() => LABELS[this.kind()].plural);
  readonly singular = computed(() => LABELS[this.kind()].singular);
  readonly route_ = computed(() => LABELS[this.kind()].route);
  readonly formTitle = computed(() => (this.editingId() != null ? `Edit ${this.singular()}` : `Add ${this.singular()}`));

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(1)]],
    description: [''],
  });

  async ngOnInit(): Promise<void> {
    const kind = (this.route.snapshot.data['lookupKind'] as LookupKind) ?? 'filmGenres';
    this.kind.set(kind);

    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : null;
    this.editingId.set(id);

    if (id != null) {
      try {
        const item = await firstValueFrom(this.admin.list(kind));
        // We don't expose GET /{id} for lookups because the API uses integer ids and the
        // list endpoint is tiny; filter on the client. Adequate for the mockup's seed sizes.
        const found = item.find((x) => x.id === id);
        if (found) {
          this.form.patchValue({ name: found.name, description: found.description ?? '' });
        }
      } catch {
        // Ignore — leave the form blank; user can re-enter.
      }
    }
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);
    this.submitError.set(null);
    const body = {
      name: this.form.controls.name.value,
      description: this.form.controls.description.value || undefined,
    };
    const id = this.editingId();
    const kind = this.kind();
    const onSuccess = () => {
      this.submitting.set(false);
      void this.lookup.refreshAll();
      this.cancel();
    };
    const onError = (err: { error?: { message?: string } }) => {
      this.submitting.set(false);
      this.submitError.set(err.error?.message ?? 'Save failed. Please try again.');
    };
    // `create` and `update` return different payload types, so they are subscribed
    // separately instead of being combined into a union of observables.
    if (id == null) {
      this.admin.create(kind, body).subscribe({ next: onSuccess, error: onError });
    } else {
      this.admin.update(kind, id, body).subscribe({ next: onSuccess, error: onError });
    }
  }

  cancel(): void {
    void this.router.navigate(['/', this.route_()]);
  }
}
