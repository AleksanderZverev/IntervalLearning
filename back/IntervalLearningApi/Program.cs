using DB;
using IntervalLearningApi;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Mapping;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JsonWebTokenKeys"));

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

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(o =>
{
    o.AddPolicy("Debug", b =>
    {
        b
            .WithOrigins("http://localhost:3000", "http://localhost:5249", "https://localhost:7249")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    o.AddPolicy("Default", b => b.AllowCredentials());
});

builder.Services.AddControllers().AddNewtonsoftJson(opts =>
{
    opts.SerializerSettings.ContractResolver = new DefaultContractResolver()
        {NamingStrategy = new CamelCaseNamingStrategy()};
    opts.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
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

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    var connectionString =
#if DEBUG
     builder.Configuration.GetConnectionString("DefaultConnection");
#else
     Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
#endif
    options.UseNpgsql(connectionString);
});

builder.Services.AddJwtTokenServices(builder.Configuration);
builder.Services.AddWeb(builder.Configuration);

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
