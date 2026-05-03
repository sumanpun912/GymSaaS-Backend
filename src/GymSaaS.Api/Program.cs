using GymSaaS.Application;
using GymSaaS.Infrastructure;
using GymSaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options => { options.Title = "Gym-SaaS-ERP API"; })
        .AllowAnonymous();
}

app.MapControllers();

app.Run();
