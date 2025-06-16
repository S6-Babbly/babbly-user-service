using babbly_user_service.Data;
using babbly_user_service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Get connection string from environment variable or fallback to configuration
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? 
                          builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured. Please set ConnectionStrings__DefaultConnection environment variable or configure it in appsettings.json");
    }
    
    options.UseNpgsql(connectionString);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Get CORS origins from configuration (supports environment variables)
        var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? 
                         new[] { builder.Configuration["CorsOrigins:0"] ?? "http://localhost:3000" };
        
        // Filter out null or empty values
        corsOrigins = corsOrigins.Where(origin => !string.IsNullOrWhiteSpace(origin)).ToArray();
        
        if (corsOrigins.Length == 0)
        {
            corsOrigins = new[] { "http://localhost:3000" };
        }
        
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply migrations automatically - only in development or when explicitly enabled
if (app.Environment.IsDevelopment() || 
    Environment.GetEnvironmentVariable("ENABLE_AUTO_MIGRATION")?.ToLower() == "true")
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
        
        if (app.Environment.IsProduction())
        {
            throw; // Re-throw in production to prevent startup with database issues
        }
    }
}

app.Run();
