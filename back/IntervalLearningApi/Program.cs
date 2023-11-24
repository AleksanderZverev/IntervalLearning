using Application.DI;
using DB;
using IntervalLearningApi;
using IntervalLearningApi.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using DB.DependencyInjection;
using IntervalLearningApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString =
#if DEBUG
    configuration.GetConnectionString("DefaultConnection");
#else
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
#endif

var bindJwtSettings = new JwtSettings();
configuration.Bind("JsonWebTokenKeys", bindJwtSettings);
var jwtSettings = configuration.GetSection("JsonWebTokenKeys").Get<JwtSettings>();

var services = builder.Services;

services.AddPersistence((o) =>
{
    o.UseNpgsql(connectionString);
});

services.AddApplication();

services.AddWeb(new SecretConfig()
{
    JwtSettings = jwtSettings,
});

// Add services to the container.

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.Request
                      | HttpLoggingFields.RequestBody
                      | HttpLoggingFields.RequestPath 
                      | HttpLoggingFields.RequestMethod
                      | HttpLoggingFields.Response 
                      | HttpLoggingFields.ResponseBody 
                      | HttpLoggingFields.ResponseStatusCode;
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("Debug", b =>
    {
        b
            .WithOrigins("http://localhost:4001", "http://localhost:5249", "https://localhost:7249")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    o.AddPolicy("Default", b => b.AllowCredentials());
});

//.AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("Debug");
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "api/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "My Cool API V1");
        options.RoutePrefix = "api/swagger";
    });
}
else
{
    app.UseCors("Default");

    //TODO: In Docker remove
    //app.UseHttpsRedirection();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<JwtMiddleware>();


app.MapControllers();

//app.MapFallbackToFile("index.html"); ;

app.Run();

public partial class Program { }
