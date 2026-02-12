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