using CalorieTracker.API.Data;
using CalorieTracker.API.Data.Repositories;
using CalorieTracker.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use SQLite for development
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register application services
builder.Services.AddScoped<ICalorieCalculatorService, CalorieCalculatorService>();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IDailyLogRepository, DailyLogRepository>();

// Configure CORS to allow requests from the Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins("http://localhost:4200") // Angular development server
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                // Production CORS - replace with your actual frontend domain
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "https://calorietracker-frontend.onrender.com/" };
                
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        });
});

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Calorie Tracker API",
        Version = "v1",
        Description = "API for tracking calories, managing goals, and log entries",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Calorie Tracker Team"
        }
    });
});

// Add global exception handling middleware
builder.Services.AddExceptionHandler(options =>
{
    // Log and transform exceptions into appropriate API responses
    options.ExceptionHandler = context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Calorie Tracker API V1");
        // Set Swagger UI as the startup page
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // Use exception handler middleware in production
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Initialize database for both development and production
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        
        // Create the database if it doesn't exist
        dbContext.Database.EnsureCreated();
        
        // Seed the database with initial data if empty
        await DataSeeder.SeedDataAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// Only use HTTPS redirection if not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowAngularFrontend");

app.UseAuthorization();

// Map controllers
app.MapControllers();

await app.RunAsync();
