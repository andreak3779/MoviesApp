import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LookupService } from '../../core/services/lookup.service';
import {
  LookupAdminService,
  LookupKind,
  LookupDto,
} from './lookup.service';
import { DeleteConfirmationComponent } from '../shared/delete-confirmation.component';

const LABELS: Record<LookupKind, { singular: string; plural: string; route: string }> = {
  filmGenres: { singular: 'Genre', plural: 'Genres', route: 'genres' },
  mediaTypes: { singular: 'Media Type', plural: 'Media Types', route: 'media-types' },
  formats: { singular: 'Format', plural: 'Formats', route: 'formats' },
};

interface DeleteT {
  id: number;
  name: string;
}

/**
 * One component per lookup kind, parameterised by the {@link LOOKUP_KIND}
 * token on the route. Lists the items and offers add / edit / delete.
 */
@Component({
  selector: 'app-lookup-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, DeleteConfirmationComponent],
  template: `
    <header class="bar">
      <h1>{{ title() }}</h1>
      <button type="button" class="primary" (click)="openAdd()">+ Add {{ singular() }}</button>
    </header>

    <div class="card">
      @if (items().length === 0) {
        <p class="empty">No items yet.</p>
      } @else {
        <ul class="rows">
          @for (it of items(); track it.id) {
            <li class="row">
              <div class="meta">
                <div class="name">{{ it.name }}</div>
                @if (it.description) {
                  <div class="desc">{{ it.description }}</div>
                }
              </div>
              <button type="button" class="link" (click)="openEdit(it)">Edit</button>
              <button type="button" class="link danger" (click)="requestDelete(it)">Delete</button>
            </li>
          }
        </ul>
      }
    </div>

    <app-delete-confirmation
      [visible]="!!deleteTarget()"
      [title]="deleteTarget() ? 'Delete ' + singular().toLowerCase() + '?' : ''"
      [message]="deleteMessage()"
      (confirm)="confirmDelete()"
      (cancel)="deleteTarget.set(null)"
    />
  `,
  styles: [`
    .bar { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; flex-wrap: wrap; gap: 16px; }
    .bar h1 { margin: 0; font-size: 28px; font-weight: 700; }
    .card { background: var(--surface, #fff); border: 1px solid var(--surface-border, #e6e6e6); border-radius: 16px; overflow: hidden; max-width: 680px; }
    .rows { list-style: none; padding: 0; margin: 0; }
    .row { display: flex; align-items: center; gap: 16px; padding: 16px 20px; border-bottom: 1px solid var(--surface-border, #e6e6e6); }
    .row:last-child { border-bottom: none; }
    .meta { flex: 1; min-width: 0; }
    .name { font-size: 15px; font-weight: 700; }
    .desc { font-size: 13px; color: var(--ink-secondary, #555); margin-top: 2px; }
    .empty { padding: 40px 20px; text-align: center; color: var(--ink-secondary, #555); margin: 0; }
    button {
      padding: 10px 22px;
      border-radius: 999px;
      border: none;
      background: var(--primary, #7a1256);
      color: white;
      font: 500 14px Roboto, sans-serif;
      cursor: pointer;
    }
    button.link { background: none; padding: 0; color: var(--primary, #7a1256); font: 500 13px Roboto, sans-serif; }
    button.link.danger { color: var(--danger, #b03b3b); }
  `],
})
export class LookupListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lookup = inject(LookupService);
  private readonly admin = inject(LookupAdminService);

  readonly kind = signal<LookupKind>('filmGenres');
  readonly title = computed(() => LABELS[this.kind()].plural);
  readonly singular = computed(() => LABELS[this.kind()].singular);
  readonly route_ = computed(() => LABELS[this.kind()].route);

  readonly items = computed<LookupDto[]>(() => {
    const k = this.kind();
    return k === 'filmGenres' ? this.lookup.filmGenres()
      : k === 'mediaTypes' ? this.lookup.mediaTypes()
      : this.lookup.formats();
  });

  readonly deleteTarget = signal<DeleteT | null>(null);
  readonly deleteMessage = computed(() => {
    const t = this.deleteTarget();
    return t ? `"${t.name}" will be permanently removed. This can't be undone.` : '';
  });

  ngOnInit(): void {
    this.route.data.subscribe((d) => {
      this.kind.set((d['lookupKind'] as LookupKind) ?? 'filmGenres');
    });
    void this.lookup.refreshAll();
  }

  openAdd(): void {
    void this.router.navigate(['/', this.route_(), 'new']);
  }

  openEdit(item: LookupDto): void {
    void this.router.navigate(['/', this.route_(), item.id, 'edit']);
  }

  requestDelete(item: LookupDto): void {
    this.deleteTarget.set({ id: item.id, name: item.name });
  }

  confirmDelete(): void {
    const t = this.deleteTarget();
    if (!t) return;
    this.admin.remove(this.kind(), t.id).subscribe({
      next: () => {
        this.deleteTarget.set(null);
        void this.lookup.refreshAll();
      },
      error: () => this.deleteTarget.set(null),
    });
  }
}
