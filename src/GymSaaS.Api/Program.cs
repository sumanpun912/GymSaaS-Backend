using GymSaaS.Api.Authentication;
using GymSaaS.Api.Exceptions;
using GymSaaS.Api.Middleware;
using GymSaaS.Application;
using GymSaaS.Infrastructure;
using GymSaaS.Infrastructure.Persistence;
using GymSaaS.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7151;
});

builder.Services.AddControllers();
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi("v1");


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(CancellationToken.None);

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        SuppressDiagnosticsCallback = _ => false
    });
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference(options => { options.Title = "Gym-SaaS-ERP API"; })
        .AllowAnonymous();
}

app.MapControllers();

app.Run();
