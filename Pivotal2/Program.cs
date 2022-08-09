using Pivotal2;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/cities", () =>
{
    var info = "Usage: /cities/{city}/{maxCount}";
    return Results.Ok(info);
});

app.MapGet("/cities/{city}/{maxCount}", (string city, int maxCount) => 
{
    Cities Cities = new Cities();
    var jsonCities = Cities.GetValue(city, maxCount);
    if (jsonCities != null) return Results.Ok(jsonCities);
    return Results.BadRequest();
});

app.Run();
