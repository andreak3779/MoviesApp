import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { FilmGenreDto } from '../../models/film-genre';
import { FormatDto } from '../../models/format';
import { MediaTypeDto } from '../../models/media-type';

export type LookupKind = 'filmGenres' | 'mediaTypes' | 'formats';

/**
 * Read-only lookup service for the three list-of-values the mockup needs.
 * Reads are anonymous (the controllers are {@code [AllowAnonymous]} on GET),
 * so this works before login.
 */
@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private readonly _filmGenres = signal<FilmGenreDto[]>([]);
  private readonly _mediaTypes = signal<MediaTypeDto[]>([]);
  private readonly _formats = signal<FormatDto[]>([]);

  readonly filmGenres = this._filmGenres.asReadonly();
  readonly mediaTypes = this._mediaTypes.asReadonly();
  readonly formats = this._formats.asReadonly();

  /** Force-fetch all three lists. Safe to call repeatedly. */
  async refreshAll(): Promise<void> {
    const [fg, mt, fm] = await Promise.all([
      firstValueFrom(this.http.get<FilmGenreDto[]>(`${this.baseUrl}/FilmGenres`)),
      firstValueFrom(this.http.get<MediaTypeDto[]>(`${this.baseUrl}/MediaTypes`)),
      firstValueFrom(this.http.get<FormatDto[]>(`${this.baseUrl}/Formats`)),
    ]);
    this._filmGenres.set(fg);
    this._mediaTypes.set(mt);
    this._formats.set(fm);
  }

  /** Look up an item's display name by id, or undefined if not loaded. */
  name(kind: LookupKind, id: number | null | undefined): string | undefined {
    if (id == null) return undefined;
    const list = kind === 'filmGenres' ? this._filmGenres()
      : kind === 'mediaTypes' ? this._mediaTypes()
      : this._formats();
    return list.find((x) => x.id === id)?.name;
  }
}
