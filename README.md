# laughing-broccoli
Example web api using rest for managing my movie collection

## Running Tests

To run the test suite, use the following command in the project root:

```
dotnet test
```

## API Endpoints

The API exposes the following endpoints for managing movies:

- `GET /api/movies` - Get all movies
- `GET /api/movies/{id}` - Get a movie by ID
- `POST /api/movies` - Create a new movie
- `PUT /api/movies/{id}` - Update an existing movie
- `DELETE /api/movies/{id}` - Delete a movie

### Example Movie Object

```json
{
  "id": 1,
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010
}
```
