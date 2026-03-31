using AbujaSocialMetaverse.API;
using AbujaSocialMetaverse.API.Middleware;
using AbujaSocialMetaverse.Infrastructure.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

// Load .env 
Env.Load();

// Validate required env vars 
StartupValidation.Validate();
StartupValidation.ValidateJwtKey();

// Build connection strings from env vars 
var dbConnection = $"Host={Env.GetString("DB_HOST")};Port={Env.GetString("DB_PORT")};Database={Env.GetString("DB_NAME")};Username={Env.GetString("DB_USER")};Password={Env.GetString("DB_PASSWORD")}";
var redisConnection = Env.GetString("REDIS_CONNECTION");
var corsOrigins = Env.GetString("CORS_ALLOWED_ORIGINS")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var builder = WebApplication.CreateBuilder(args);

// Override config with env values 
builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnection;
builder.Configuration["ConnectionStrings:Redis"] = redisConnection;
builder.Configuration["Jwt:Key"] = Env.GetString("JWT_KEY");
builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE");
builder.Configuration["Jwt:ExpiryMinutes"] = Env.GetString("JWT_EXPIRY_MINUTES");
builder.Configuration["Jwt:RefreshExpiryDays"] = Env.GetString("JWT_REFRESH_EXPIRY_DAYS");
builder.Configuration["Mapbox:AccessToken"] = Env.GetString("MAPBOX_ACCESS_TOKEN");
builder.Configuration["Mapbox:BaseUrl"] = Env.GetString("MAPBOX_BASE_URL");
builder.Configuration["Stripe:SecretKey"] = Env.GetString("STRIPE_SECRET_KEY");
builder.Configuration["Stripe:WebhookSecret"] = Env.GetString("STRIPE_WEBHOOK_SECRET");
builder.Configuration["Paystack:SecretKey"] = Env.GetString("PAYSTACK_SECRET_KEY");
builder.Configuration["Paystack:BaseUrl"] = Env.GetString("PAYSTACK_BASE_URL");

// Serilog 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(
        Env.GetString("LOG_LEVEL", "Information")))
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        Env.GetString("LOG_FILE_PATH", "logs/abuja-metaverse-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers 
builder.Services.AddControllers();

// OpenAPI 
builder.Services.AddOpenApi();

// SignalR + Redis Backplane 
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.ChannelPrefix = new RedisChannel(
            "AbujaSocialMetaverse",
            RedisChannel.PatternMode.Literal);
    });

// Database 
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(
        dbConnection,
        npgsql => npgsql.UseNetTopologySuite())
    .UseSnakeCaseNamingConvention()
    .EnableDetailedErrors(builder.Environment.IsDevelopment())
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnityClient", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Authentication 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Rate Limiting 
var permitLimit = Env.GetInt("RATE_LIMIT_PERMIT_LIMIT", 100);
var windowSeconds = Env.GetInt("RATE_LIMIT_WINDOW_SECONDS", 60);
var queueLimit = Env.GetInt("RATE_LIMIT_QUEUE_LIMIT", 10);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = permitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = queueLimit;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://httpstatuses.com/429",
            title = "Too Many Requests",
            status = 429,
            detail = "Rate limit exceeded. Please slow down.",
            instance = context.HttpContext.Request.Path.ToString(),
            traceId = context.HttpContext.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.HttpContext.Response.WriteAsync(json, cancellationToken);
    };
});

// Health Checks
builder.Services.AddHealthChecks();


// Build 
var app = builder.Build();

// Middleware Pipeline 
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
});

app.UseHttpsRedirection();
app.UseCors("AllowUnityClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// SignalR Hubs 
// app.MapHub<AvatarHub>("/hubs/avatar");
// app.MapHub<ChatHub>("/hubs/chat");
// Commented until hubs are created in Phase 5

app.Run();