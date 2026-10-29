import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/api-config';
import { FilmGenreDto } from '../../models/film-genre';
import { FormatDto } from '../../models/format';
import { MediaTypeDto } from '../../models/media-type';

export type LookupDto = FilmGenreDto | MediaTypeDto | FormatDto;
export type LookupKind = 'filmGenres' | 'mediaTypes' | 'formats';

const PATHS: Record<LookupKind, string> = {
  filmGenres: 'FilmGenres',
  mediaTypes: 'MediaTypes',
  formats: 'Formats',
};

/**
 * CRUD-flavoured lookup client used by the lookup list + form components.
 * Reads go through {@link LookupService} (cached) but writes hit the API
 * directly and then trigger a refresh on the cache.
 */
@Injectable({ providedIn: 'root' })
export class LookupAdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(kind: LookupKind): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.baseUrl}/${PATHS[kind]}`);
  }

  create(kind: LookupKind, body: { name: string; description?: string }): Observable<LookupDto> {
    return this.http.post<LookupDto>(`${this.baseUrl}/${PATHS[kind]}`, body);
  }

  update(kind: LookupKind, id: number, body: { name: string; description?: string }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${PATHS[kind]}/${id}`, body);
  }

  remove(kind: LookupKind, id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${PATHS[kind]}/${id}`);
  }
}
