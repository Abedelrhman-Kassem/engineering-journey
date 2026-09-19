using Application.Interfaces;
using Host.Middlewares;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure();

builder.Services.AddProblemDetails();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();


    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseRequestLogging();
app.UseExceptionHandler();

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/test", () => {throw new Exception("Test exception");});

app.MapControllers();

app.Run();
