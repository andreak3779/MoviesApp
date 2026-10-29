import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../api-config';
import { LookupService } from './lookup.service';

describe('LookupService', () => {
  let service: LookupService;
  let controller: HttpTestingController;
  const baseUrl = 'http://api.test/api/v1';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });

    service = TestBed.inject(LookupService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should load all three lists', async () => {
    const loaded = service.refreshAll();

    controller.expectOne(`${baseUrl}/FilmGenres`).flush([{ id: 1, name: 'Drama' }]);
    controller.expectOne(`${baseUrl}/MediaTypes`).flush([{ id: 2, name: 'DVD' }]);
    controller.expectOne(`${baseUrl}/Formats`).flush([{ id: 3, name: 'Movie' }]);

    await loaded;

    expect(service.filmGenres()).toEqual([{ id: 1, name: 'Drama' }]);
    expect(service.mediaTypes()).toEqual([{ id: 2, name: 'DVD' }]);
    expect(service.formats()).toEqual([{ id: 3, name: 'Movie' }]);
  });

  it('should find the name of an item by id', async () => {
    const loaded = service.refreshAll();

    controller.expectOne(`${baseUrl}/FilmGenres`).flush([{ id: 1, name: 'Drama' }]);
    controller.expectOne(`${baseUrl}/MediaTypes`).flush([]);
    controller.expectOne(`${baseUrl}/Formats`).flush([]);

    await loaded;

    expect(service.name('filmGenres', 1)).toBe('Drama');
    expect(service.name('mediaTypes', 1)).toBeUndefined();
    expect(service.name('formats', null)).toBeUndefined();
    expect(service.name('filmGenres', undefined)).toBeUndefined();
  });
});
