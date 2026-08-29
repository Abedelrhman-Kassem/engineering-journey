using Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var elapsedTime = Stopwatch.StartNew();
    
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Status Code: {context.Response.StatusCode}");

    var elapsedMilliseconds = elapsedTime.ElapsedMilliseconds;
    Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
