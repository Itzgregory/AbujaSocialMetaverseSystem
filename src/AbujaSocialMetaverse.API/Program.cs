using AbujaSocialMetaverse.API;
using AbujaSocialMetaverse.API.Middleware;
using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Infrastructure.Caching;
using AbujaSocialMetaverse.Infrastructure.RealTime;
using AbujaSocialMetaverse.Infrastructure.BackgroundJobs;
using AbujaSocialMetaverse.Shared.Configuration;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
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

// Map env vars into IConfiguration
var envMappings = new Dictionary<string, string?>
{
    ["Database:Host"] = Env.GetString("DB_HOST"),
    ["Database:Port"] = Env.GetString("DB_PORT"),
    ["Database:Name"] = Env.GetString("DB_NAME"),
    ["Database:Username"] = Env.GetString("DB_USER"),
    ["Database:Password"] = Env.GetString("DB_PASSWORD"),
    ["Redis:Host"] = Env.GetString("REDIS_HOST"),
    ["Redis:Port"] = Env.GetString("REDIS_PORT"),
    ["Redis:Password"] = Env.GetString("REDIS_PASSWORD"),
    ["Jwt:SecretKey"] = Env.GetString("JWT_SECRET_KEY"),
    ["Jwt:Issuer"] = Env.GetString("JWT_ISSUER"),
    ["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE"),
    ["Jwt:ExpiryMinutes"] = Env.GetString("JWT_EXPIRY_MINUTES"),
    ["Jwt:RefreshExpiryDays"] = Env.GetString("JWT_REFRESH_EXPIRY_DAYS"),
    ["Mapbox:AccessToken"] = Env.GetString("MAPBOX_ACCESS_TOKEN"),
    ["Mapbox:BaseUrl"] = Env.GetString("MAPBOX_BASE_URL"),
    ["Stripe:SecretKey"] = Env.GetString("STRIPE_SECRET_KEY"),
    ["Stripe:WebhookSecret"] = Env.GetString("STRIPE_WEBHOOK_SECRET"),
    ["Paystack:SecretKey"] = Env.GetString("PAYSTACK_SECRET_KEY"),
    ["Paystack:BaseUrl"] = Env.GetString("PAYSTACK_BASE_URL"),
    ["Cors:AllowedOrigins"] = Env.GetString("CORS_ALLOWED_ORIGINS"),
    ["Logging:MinimumLevel"] = Env.GetString("LOG_LEVEL"),
    ["Logging:FilePath"] = Env.GetString("LOG_FILE_PATH"),
    ["RateLimit:PermitLimit"] = Env.GetString("RATE_LIMIT_PERMIT_LIMIT"),
    ["RateLimit:WindowSeconds"] = Env.GetString("RATE_LIMIT_WINDOW_SECONDS"),
    ["RateLimit:QueueLimit"] = Env.GetString("RATE_LIMIT_QUEUE_LIMIT"),
    ["Email:Provider"] = Env.GetString("EMAIL_PROVIDER"),
    ["Email:Host"] = Env.GetString("EMAIL_HOST"),
    ["Email:Port"] = Env.GetString("EMAIL_PORT"),
    ["Email:Username"] = Env.GetString("EMAIL_USERNAME"),
    ["Email:Password"] = Env.GetString("EMAIL_PASSWORD"),
    ["Email:FromEmail"] = Env.GetString("EMAIL_FROM"),
    ["Email:FromName"] = Env.GetString("EMAIL_FROM_NAME"),
    ["Email:BaseUrl"] = Env.GetString("EMAIL_BASE_URL"),
};

var builder = WebApplication.CreateBuilder(args);

// Inject env mappings into IConfiguration
builder.Configuration.AddInMemoryCollection(
    envMappings.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)));

// Register and validate all options
builder.Services.AddApplicationOptions(builder.Configuration);

// SUPPRESS THE WARNING ONLY FOR THIS SPECIFIC BLOCK
#pragma warning disable ASP0000
var sp = builder.Services.BuildServiceProvider();
var dbOptions = sp.GetRequiredService<DatabaseOptions>();
var redisOptions = sp.GetRequiredService<RedisOptions>();
var jwtOptions = sp.GetRequiredService<JwtOptions>();
var corsOptions = sp.GetRequiredService<CorsOptions>();
var rateLimitOptions = sp.GetRequiredService<RateLimitOptions>();
var hangfireOptions = sp.GetRequiredService<HangfireOptions>();
var loggingOptions = sp.GetRequiredService<LoggingOptions>();
#pragma warning restore ASP0000

// Serilog (no change needed - this part is fine)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(
        loggingOptions.MinimumLevel, ignoreCase: true))
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
        Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Hangfire", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "AbujaSocialMetaverse")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        loggingOptions.FilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: loggingOptions.RetainedFileCount,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{Environment}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// SignalR + Redis Backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisOptions.ConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = new RedisChannel(
            redisOptions.ChannelPrefix,
            RedisChannel.PatternMode.Literal);
    });

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ILocationCacheService, RedisLocationCacheService>();
builder.Services.AddScoped<ICacheAdminService, RedisCacheAdminService>();

// Real-Time Service
builder.Services.AddScoped<IConnectionTracker, RedisConnectionTracker>();
builder.Services.AddScoped<IRealTimeService, SignalRRealTimeService>();

// Hangfire
builder.Services.AddHangfire(config =>
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(dbOptions.ConnectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = hangfireOptions.WorkerCount;
    options.Queues = hangfireOptions.Queues;
});

builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

// Database
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(
        dbOptions.ConnectionString,
        npgsql => npgsql.UseNetTopologySuite())
    .UseSnakeCaseNamingConvention()
    .EnableDetailedErrors(dbOptions.EnableDetailedErrors)
    .EnableSensitiveDataLogging(dbOptions.EnableSensitiveDataLogging));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsOptions.PolicyName, policy =>
    {
        policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (corsOptions.AllowCredentials)
            policy.AllowCredentials();
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
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
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
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitOptions.PermitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = rateLimitOptions.QueueLimit;
    });

    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitOptions.AuthEndpointPermitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
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
    app.MapOpenApi();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
});

app.UseHttpsRedirection();
app.UseCors(corsOptions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard(hangfireOptions.DashboardPath,
        new DashboardOptions { Authorization = [] });
}

app.MapControllers();
app.MapHealthChecks("/health");

// SignalR Hubs (uncomment when hubs are created)
// app.MapHub<AvatarHub>("/hubs/avatar");
// app.MapHub<ChatHub>("/hubs/chat");

app.Run();