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
 *
 * The route is the **singular** {@code /Movie}: the API's controller is
 * named {@code MovieController}, so {@code /api/v1/movies} is a 404.
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
    return this.http.get<PagedResponse<Movie>>(`${this.baseUrl}/Movie`, { params });
  }

  getById(id: string): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/Movie/${id}`);
  }

  create(payload: MovieCreatePayload): Observable<Movie> {
    return this.http.post<Movie>(`${this.baseUrl}/Movie`, payload);
  }

  update(id: string, payload: MovieUpdatePayload): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Movie/${id}`, payload);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Movie/${id}`);
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
