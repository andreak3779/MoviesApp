import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api-config';
import {
  Movie,
  MovieCreatePayload,
  MovieQuery,
  MovieUpdatePayload,
  PagedResponse,
} from '../models/movie';

/**
 * HTTP client for the Movies resource on the Web API. Returns Observables
 * to match the existing NgRx effects shape.
 */
@Injectable({ providedIn: 'root' })
export class MovieService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  search(query: MovieQuery = {}): Observable<PagedResponse<Movie>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value === undefined || value === null || value === '') continue;
      params = params.set(this.camelToPascal(key), String(value));
    }
    return this.http.get<PagedResponse<Movie>>(`${this.baseUrl}/movies`, { params });
  }

  getById(id: string): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/movies/${id}`);
  }

  create(payload: MovieCreatePayload): Observable<Movie> {
    return this.http.post<Movie>(`${this.baseUrl}/movies`, payload);
  }

  update(id: string, payload: MovieUpdatePayload): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/movies/${id}`, payload);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/movies/${id}`);
  }

  /**
   * The API expects PascalCase query keys (Search / Genre / MediaType / Format
   * / SortBy / SortDir / PageNumber / PageSize). Convert from the camelCase
   * shape used by the rest of the Angular app.
   */
  private camelToPascal(key: string): string {
    return key.charAt(0).toUpperCase() + key.slice(1);
  }
}
