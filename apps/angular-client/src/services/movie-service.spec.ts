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

  it('should add, get, update and remove a movie', async () => {
    const added: any = await firstValueFrom(
      service.addMovie({
        Title: 'Test Movie',
        Description: 'desc',
        Genre: 'Test',
        Criterion: false,
        CriterionNumber: undefined,
        MovieMediaType: MediaType.Dvd,
        MovieFormat: MovieFormat.Movie,
        Year: 2025,
        Favourite: false,
      })
    );

    expect(added).toBeTruthy();
    expect(added.Id).toBeGreaterThan(0);

    const fetched: any = await firstValueFrom(service.getMovie(added.Id));
    expect(fetched).toBeTruthy();
    expect(fetched.Title).toBe('Test Movie');

    // update
    fetched.Title = 'Test Movie Updated';
    const updated: any = await firstValueFrom(service.updateMovie(fetched));
    expect(updated.Title).toBe('Test Movie Updated');

    // remove
    await firstValueFrom(service.removeMovie(fetched.Id));

    const afterRemove: any = await firstValueFrom(service.getMovie(fetched.Id));
    expect(afterRemove).toBeUndefined();
  });

  it('should return favourite movies', async () => {
    const favs: any = await firstValueFrom(service.getFavouriteMovies());
    expect(Array.isArray(favs)).toBe(true);
    expect(favs.length).toBeGreaterThanOrEqual(1);
  });

  it('should query movies by partial criteria', async () => {
    const results: any = await firstValueFrom(service.queryMovies({ Title: 'matrix' }));
    expect(results.length).toBeGreaterThanOrEqual(1);
  });

  it('should change criterion and favourite flags', async () => {
    const changed: any = await firstValueFrom(service.changeCriterion(1, true, 5));
    expect(changed).toBeTruthy();
    expect(changed.Criterion).toBe(true);
    expect(changed.CriterionNumber).toBe(5);

    const favChanged: any = await firstValueFrom(service.changeFavourite(1, true));
    expect(favChanged).toBeTruthy();
    expect(favChanged.Favourite).toBe(true);
  });

  it('should support paging via getAllMovies', async () => {
    const page: any = await firstValueFrom(service.getAllMovies(1, 2));
    expect(page).toBeTruthy();
    expect(page.items.length).toBeLessThanOrEqual(2);
    expect(page.total).toBeGreaterThanOrEqual(page.items.length);
  });
});
