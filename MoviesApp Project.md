# MoviesApp

> **This is the original brief.** It is kept verbatim below as the source of
> truth for *what was asked for*. It has not been rewritten to match the
> implementation — the gap between it and the code is itself information.
>
> **Status as of 2026-10-25** (details in
> [docs/PROJECT-STATUS.md](docs/PROJECT-STATUS.md)):
>
> | Brief item | Status |
> | --- | --- |
> | Angular front end with NgRx | **Done** — Angular 22, NgRx 22, standalone components, signals |
> | Storing the movie collection | **Done** — CRUD against `/api/v1/Movie` |
> | Updating favourites | **Partial** — the `Favourite` flag is modelled and editable, but there is no favourites *list* endpoint |
> | Query by title, genre, year | **Done** — `search`, `genre`, `mediaType`, `format`, `sortBy`, `sortDir`, `pageNumber`, `pageSize` |
> | Query by criterion / criterion number / favourite list | **Not done** |
> | Movie watch list | **Not done** — no model, endpoint, store slice or component exists |
> | Movie header component (count, search bar, title) | **Partial** — the list view has a search bar and a total count; there is no separate header component |
> | Movies list component with paging and a query bar | **Done** — search, three filters, sort, paging |
> | Standalone components, signals, NgRx effects for all calls | **Done** |
> | `PATCH` for criterion and favourite | **Not done** — both are updated through `PUT /api/v1/Movie/{id}` |
> | MovieService returning observables | **Done** |
> | MongoDB + SQLAlchemy | **Superseded** — the implementation uses EF Core + SQLite on .NET 10 |
>
> Note the brief says "Angular 21" and "unique id number". The implementation
> uses Angular 22 and GUID identifiers, because the Web API issues GUIDs and
> the brief's own security guidance in `docs/` prefers them over enumerable
> integers.

# MoviesApp
The front end uses angular 21
TypeScript
State should be managed with NgRx

The front end needs to allow
storing your movie collection,
update favourite movies
query movies using title, genre, criterion, criterion number, year, favourite list
Movie Watch list

##### The movies need to have 
- title string,
- unique id number
- description string
- genre string
- Criterion Y/N boolean
- Criterion Number number:
- MediaType enum: bluray, bluray Ultra, dvd
- Format enum: Movie, TV Series, Box set
- Year released number
- favourite boolean

## To do list
1. use MongoDb to store the Movies based on 
2. SQLAlchemy  as an ORM 
3. Create movieService

## Front end of MovieApp 
- use angular 21 
### App components
- All components should be standalone
- All pages should use angular Signal
- All calls to the MovieService should use effect from NgRx

#### Movies list compoent
- support paging on page
- have a query bar
show a list of movies based on the movie interface. 

#### Movie Watch List component

#### Movie header component
- Show the number of movies
- have a search bar
- show the app title 

### MovieService
The movieService should use mokoon to create the initial responses to the api calls.
Rxjs should be used and all methods should return observables
- Store a movie (post)
- Get a movie (get)
- get all movies (get)
- get favourite movies(get)
- query for a collection of movies (get) query params
- Change Movie (put)
- Change Criterion (patch) changes criterion flag and number.
- Change favourite (patch)
- Remove Movie (delete)

### MovieWatchListService
The MovieWatchListService should use mokoon to create the initial responses to the api calls.
Rxjs should be used and all methods should return observables
- Add Watch List(post)
- Remove watch list (delete )
- Get Watch List (get)