import { FilmGenreDto } from './film-genre';
import { FormatDto } from './format';
import { MediaTypeDto } from './media-type';

/**
 * Mirrors the API's {@code MovieResponseDto}. The mockup consumes the
 * denormalised single-value fields (Genre / MediaType / Format) for the
 * filter row and the card; the full collections are kept for callers that
 * want to render rich detail.
 */
export interface Movie {
  Id: string;
  Title: string;
  ReleaseYear: number;
  Description?: string | null;
  Favourite: boolean;
  Criterion: boolean;
  CriterionNumber?: number | null;
  OwnerId: string;
  CreatedAt: string;
  UpdatedAt?: string | null;

  // Denormalised single-value fields for the mockup.
  Genre?: string | null;
  MediaType?: string | null;
  Format?: string | null;

  // Full collections.
  FilmGenres: FilmGenreDto[];
  MediaTypes: MediaTypeDto[];
  Formats: FormatDto[];
}

/**
 * Payload sent to {@code POST /api/v1/movies}. Mirrors the API's
 * {@code MovieCreateDto}.
 */
export interface MovieCreatePayload {
  Title: string;
  ReleaseYear: number;
  Description?: string | null;
  Favourite: boolean;
  Criterion: boolean;
  CriterionNumber?: number | null;
  FilmGenreId?: number | null;
  MediaTypeId?: number | null;
  FormatId?: number | null;
}

/**
 * Payload sent to {@code PUT /api/v1/movies/{id}}. Mirrors the API's
 * {@code MovieUpdateDto} — all fields optional.
 */
export type MovieUpdatePayload = Partial<MovieCreatePayload>;

/**
 * Query parameters for {@code GET /api/v1/movies}. Mirrors the API's
 * {@code MovieQuery}.
 */
export interface MovieQuery {
  search?: string;
  genre?: string;
  mediaType?: string;
  format?: string;
  sortBy?: 'title' | 'year';
  sortDir?: 'asc' | 'desc';
  pageNumber?: number;
  pageSize?: number;
}

/**
 * Generic paged response wrapper. Mirrors the API's {@code PagedResponse<T>}.
 */
export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
