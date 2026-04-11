var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IMoviesService, MoviesService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MoviesApi:BaseUrl"] ?? string.Empty);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();


app.MapGet("/health", () => new { status = "ok" });

app.MapStaticAssets();

app.MapDefaultControllerRoute();

app.Run();
