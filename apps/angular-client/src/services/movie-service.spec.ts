import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { MovieService } from './movie-service';
import { MediaType } from '../models/media-type';
import { MovieFormat } from '../models/movie-format';

describe('MovieService', () => {
  let service: MovieService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MovieService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAllMovies', () => {
    it('should return first page with total', async () => {
      const result = await firstValueFrom(service.getAllMovies(1, 2));
      expect(result.items.length).toBeLessThanOrEqual(2);
      expect(result.total).toBeGreaterThanOrEqual(result.items.length);
    });

    it('should return all seed movies on a large page size', async () => {
      const result = await firstValueFrom(service.getAllMovies(1, 100));
      expect(result.items.length).toBe(3);
      expect(result.total).toBe(3);
    });
  });

  describe('getMovie', () => {
    it('should return movie by id', async () => {
      const movie = await firstValueFrom(service.getMovie(1));
      expect(movie).toBeTruthy();
      expect(movie!.Title).toBe('The Matrix');
    });

    it('should return undefined for unknown id', async () => {
      const movie = await firstValueFrom(service.getMovie(9999));
      expect(movie).toBeUndefined();
    });
  });

  describe('addMovie', () => {
    it('should assign a new id and persist the movie', async () => {
      const added = await firstValueFrom(
        service.addMovie({
          Title: 'Inception',
          Description: 'A dream within a dream.',
          Genre: 'Sci-Fi',
          Criterion: false,
          CriterionNumber: undefined,
          MovieMediaType: MediaType.BluRay,
          MovieFormat: MovieFormat.Movie,
          Year: 2010,
          Favourite: false,
        }),
      );

      expect(added.Id).toBeGreaterThan(0);

      const fetched = await firstValueFrom(service.getMovie(added.Id));
      expect(fetched?.Title).toBe('Inception');
    });
  });

  describe('updateMovie', () => {
    it('should persist the updated fields', async () => {
      const original = await firstValueFrom(service.getMovie(1));
      const updated = await firstValueFrom(
        service.updateMovie({ ...original!, Title: 'The Matrix Reloaded' }),
      );

      expect(updated.Title).toBe('The Matrix Reloaded');

      const refetched = await firstValueFrom(service.getMovie(1));
      expect(refetched?.Title).toBe('The Matrix Reloaded');
    });
  });

  describe('removeMovie', () => {
    it('should remove the movie so it is no longer findable', async () => {
      const added = await firstValueFrom(
        service.addMovie({
          Title: 'Temp',
          Description: '',
          Genre: 'Drama',
          Criterion: false,
          CriterionNumber: undefined,
          MovieMediaType: MediaType.Dvd,
          MovieFormat: MovieFormat.Movie,
          Year: 2000,
          Favourite: false,
        }),
      );

      await firstValueFrom(service.removeMovie(added.Id));

      const after = await firstValueFrom(service.getMovie(added.Id));
      expect(after).toBeUndefined();
    });
  });
});
