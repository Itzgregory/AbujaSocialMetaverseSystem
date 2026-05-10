using AbujaSocialMetaverse.API.Extensions;
using AbujaSocialMetaverse.Modules.Core;
using AbujaSocialMetaverse.Shared.Configuration;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using DotNetEnv;

// Load .env from current or parent directories
Env.TraversePath().Load();

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

// Parse comma-separated CORS origins into configuration array format
var corsOrigins = Env.GetString("CORS_ALLOWED_ORIGINS")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
if (corsOrigins != null)
{
    for (int i = 0; i < corsOrigins.Length; i++)
    {
        envMappings[$"Cors:AllowedOrigins:{i}"] = corsOrigins[i].Trim();
    }
}

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

// ─── Service Registration ─────────────────────────────────────────
builder.AddSerilogLogging(loggingOptions);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalRWithRedis(redisOptions);
builder.Services.AddRedisServices(redisOptions);
builder.Services.AddRealTimeServices();
builder.Services.AddHangfireServices(dbOptions, hangfireOptions);
builder.Services.AddDatabaseServices(dbOptions);
builder.Services.AddCorsPolicy(corsOptions);
builder.Services.AddJwtAuthentication(jwtOptions);
builder.Services.AddEmailLinkGenerator();
builder.Services.AddApiRateLimiting(rateLimitOptions);
builder.Services.AddCoreModule();
builder.Services.AddHealthChecks();

// Middleware Pipeline 
var app = builder.Build();
app.UseMiddlewarePipeline(corsOptions, hangfireOptions);
app.Run();