using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var enableHttpsRedirection = app.Configuration.GetValue("EnableHttpsRedirection", true);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

/* // Define endpoints
app.MapGet("/movies", () => movies);

app.MapGet("/movies/{id}", (int id) => 
{
    var movie = movies.FirstOrDefault(m => m.Id == id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

app.MapPost("/movies", (Movie movie) => 
{
    movie.Id = movies.Count > 0 ? movies.Max(m => m.Id) + 1 : 1;
    movies.Add(movie);
    return Results.Created($"/movies/{movie.Id}", movie);
});

app.MapPut("/movies/{id}", (int id, Movie updatedMovie) => 
{
    var index = movies.FindIndex(m => m.Id == id);
    if (index == -1) return Results.NotFound();

    updatedMovie.Id = id;
    movies[index] = updatedMovie;
    return Results.NoContent();
});

app.MapDelete("/movies/{id}", (int id) => 
{
    var index = movies.FindIndex(m => m.Id == id);
    if (index == -1) return Results.NotFound();

    movies.RemoveAt(index);
    return Results.NoContent();
}); */

app.Run();