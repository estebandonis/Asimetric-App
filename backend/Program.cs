using backend;
using backend.Algorithms;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("AllowAll");

app.MapControllers();

// app.MapGet("/test", () => "Hello World!");
//
// app.MapGet("/users", async(AppDbContext db) => await db.Users.ToListAsync());
//
// app.MapPost("/users", async([FromBody] User user, AppDbContext db) =>
// {
//     Console.WriteLine("User Email: " + user.email);
//     Console.WriteLine("User Password: " + user.password);
//     Console.WriteLine("User key: " + user.public_key);
//     
//     user.password = SHAImplementation.Hash(user.password);
//     
//     db.Users.Add(user);
//     await db.SaveChangesAsync();
//     return Results.Created($"/users/{user.email}", user);
// });

app.Run();