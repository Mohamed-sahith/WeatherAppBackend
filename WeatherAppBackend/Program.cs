using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WeatherAppBackend.Helpers;
using WeatherAppBackend.Models;
using WeatherAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configurations
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.MongoDB.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Validate configurations
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var weatherApiKey = builder.Configuration["WeatherApi:ApiKey"];
var unsplashAccessKey = builder.Configuration["Unsplash:AccessKey"];
if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) ||
    string.IsNullOrEmpty(weatherApiKey) || string.IsNullOrEmpty(unsplashAccessKey))
{
    throw new InvalidOperationException("Missing required configuration: JWT (Key, Issuer, Audience) or API keys (WeatherApi, Unsplash).");
}

// Add services
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7027")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Added to handle preflight requests
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(jwtKey!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                builder.Services.BuildServiceProvider().GetService<ILogger<Program>>()?.LogError("JWT Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddScoped<UserDataService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddSingleton<FavouriteCityService>();
builder.Services.AddScoped<UnsplashService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add HttpClientFactory
builder.Services.AddHttpClient();

// Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather API",
        Version = "v1",
        Description = "Integrated JWT Authentication API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT with Bearer format: `Bearer {your_token}`"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure middleware and initialize indexes
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
        c.EnablePersistAuthorization();
    });

    using (var scope = app.Services.CreateScope())
    {
        var userDataService = scope.ServiceProvider.GetRequiredService<UserDataService>();
        var user = new User();
        user.EnsureIndexes(userDataService.UsersCollection); // Access via reflection or make _users accessible
    }
}

app.UseCors("AllowBlazorClient");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();