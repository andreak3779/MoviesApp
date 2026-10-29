import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../core/api-config';
import { MovieService } from './movie-service';

describe('MovieService', () => {
  let service: MovieService;
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

    service = TestBed.inject(MovieService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should call the singular /Movie route', () => {
    // The API's controller is MovieController, so the route is /api/v1/Movie.
    // /api/v1/movies is a 404.
    service.search().subscribe();

    const request = controller.expectOne(`${baseUrl}/Movie`);
    expect(request.request.method).toBe('GET');
    request.flush({ items: [] });
  });

  it('should send the query as PascalCase parameters', () => {
    service
      .search({
        search: 'alien',
        genre: 'Science Fiction',
        sortBy: 'title',
        pageNumber: 2,
        pageSize: 6,
      })
      .subscribe();

    const request = controller.expectOne((r) => r.url === `${baseUrl}/Movie`);
    expect(request.request.params.get('Search')).toBe('alien');
    expect(request.request.params.get('Genre')).toBe('Science Fiction');
    expect(request.request.params.get('SortBy')).toBe('title');
    expect(request.request.params.get('PageNumber')).toBe('2');
    expect(request.request.params.get('PageSize')).toBe('6');
    request.flush({ items: [] });
  });

  it('should leave out empty query values', () => {
    service.search({ search: '', genre: undefined, pageNumber: 1 }).subscribe();

    const request = controller.expectOne((r) => r.url === `${baseUrl}/Movie`);
    expect(request.request.params.has('Search')).toBe(false);
    expect(request.request.params.has('Genre')).toBe(false);
    request.flush({ items: [] });
  });

  it('should return the paging metadata with the items', () => {
    let received: unknown;
    service.search().subscribe((response) => (received = response));

    controller.expectOne(`${baseUrl}/Movie`).flush({
      items: [],
      pageNumber: 2,
      pageSize: 6,
      totalCount: 13,
      totalPages: 3,
      hasPrevious: true,
      hasNext: true,
    });

    expect(received).toEqual({
      items: [],
      pageNumber: 2,
      pageSize: 6,
      totalCount: 13,
      totalPages: 3,
      hasPrevious: true,
      hasNext: true,
    });
  });

  it('should get a movie by id', () => {
    const id = '00000000-0000-0000-0000-000000000001';
    service.getById(id).subscribe();

    const request = controller.expectOne(`${baseUrl}/Movie/${id}`);
    expect(request.request.method).toBe('GET');
    request.flush({});
  });

  it('should create a movie', () => {
    service
      .create({
        title: 'The Matrix',
        releaseYear: 1999,
        favourite: false,
        criterion: false,
      })
      .subscribe();

    const request = controller.expectOne(`${baseUrl}/Movie`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body.title).toBe('The Matrix');
    request.flush({});
  });

  it('should update a movie', () => {
    const id = '00000000-0000-0000-0000-000000000001';
    service.update(id, { title: 'The Matrix Reloaded' }).subscribe();

    const request = controller.expectOne(`${baseUrl}/Movie/${id}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ title: 'The Matrix Reloaded' });
    request.flush(null);
  });

  it('should delete a movie', () => {
    const id = '00000000-0000-0000-0000-000000000001';
    service.remove(id).subscribe();

    const request = controller.expectOne(`${baseUrl}/Movie/${id}`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
