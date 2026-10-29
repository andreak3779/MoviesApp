import { FilmGenreDto } from './film-genre';
import { FormatDto } from './format';
import { MediaTypeDto } from './media-type';

/**
 * Mirrors the API's {@code MovieResponseDto}. The mockup consumes the
 * denormalised single-value fields (genre / mediaType / format) for the
 * filter row and the card; the full collections are kept for callers that
 * want to render rich detail.
 *
 * The field names are camelCase because that is what the API emits: the
 * ASP.NET Core JSON defaults are camelCase, and the wire format is the
 * contract. A PascalCase model here would leave every field undefined.
 */
export interface Movie {
  id: string;
  title: string;
  releaseYear: number;
  description?: string | null;
  favourite: boolean;
  criterion: boolean;
  criterionNumber?: number | null;
  ownerId: string;
  createdAt: string;
  updatedAt?: string | null;

  // Denormalised single-value fields for the mockup.
  genre?: string | null;
  mediaType?: string | null;
  format?: string | null;

  // Full collections.
  filmGenres: FilmGenreDto[];
  mediaTypes: MediaTypeDto[];
  formats: FormatDto[];
}

/**
 * Payload sent to {@code POST /api/v1/Movie}. Mirrors the API's
 * {@code MovieCreateDto}.
 */
export interface MovieCreatePayload {
  title: string;
  releaseYear: number;
  description?: string | null;
  favourite: boolean;
  criterion: boolean;
  criterionNumber?: number | null;
  filmGenreId?: number | null;
  mediaTypeId?: number | null;
  formatId?: number | null;
}

/**
 * Payload sent to {@code PUT /api/v1/Movie/{id}}. Mirrors the API's
 * {@code MovieUpdateDto} — all fields optional.
 */
export type MovieUpdatePayload = Partial<MovieCreatePayload>;

/**
 * Query parameters for {@code GET /api/v1/Movie}. Mirrors the API's
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
 * The paging half of a paged response. Mirrors the API's
 * {@code PagedResponse<T>} without the items.
 */
export interface MoviePage {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/**
 * Generic paged response wrapper. Mirrors the API's {@code PagedResponse<T>}.
 */
export interface PagedResponse<T> extends MoviePage {
  items: T[];
}
