using StoreBoost.Api.Middleware;
using StoreBoost.Persistence;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//  Swagger setup with XML comments support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StoreBoost API",
        Version = "v1",
        Description = "Appointment booking system with Clean Architecture and CQRS"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

//  Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

//  Swagger middleware (enabled in all environments)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoreBoost API v1");
    c.RoutePrefix = string.Empty; // Makes Swagger UI available at root (/)
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
