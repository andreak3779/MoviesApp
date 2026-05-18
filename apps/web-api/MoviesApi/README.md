# Movie Collection API

This project is a minimal API for managing a collection of movies. It allows users to perform CRUD operations on movie entities.

## Project Structure

- **Program.cs**: Entry point of the application, sets up the minimal API and defines HTTP endpoints.
- **Movie.cs**: Defines the `Movie` class representing the movie entity with properties such as Id, Title, Genre, and ReleaseYear.
- **MovieCollectionApi.csproj**: Project file containing configuration, dependencies, and target framework.
- **Properties/launchSettings.json**: Contains settings for launching the application, including environment variables and profiles.

## Getting Started

To run the MovieCollectionApi, follow these steps:

1. Clone the repository:
   ```
   git clone <repository-url>
   cd MovieCollectionApi
   ```

2. Restore the dependencies:
   ```
   dotnet restore
   ```

3. Run the application:
   ```
   dotnet run
   ```

4. The API will be available at `http://localhost:5000` (or the specified port).

## API Endpoints

The following endpoints are available:

- `GET /movies` - Retrieve a list of all movies.
- `GET /movies/{id}` - Retrieve a specific movie by ID.
- `POST /movies` - Add a new movie to the collection.
- `PUT /movies/{id}` - Update an existing movie.
- `DELETE /movies/{id}` - Remove a movie from the collection.

### Example Movie Object

```json
{
  "id": 1,
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010
}
```

## Running Tests

To run the test suite for this project, execute:

```
dotnet test
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.