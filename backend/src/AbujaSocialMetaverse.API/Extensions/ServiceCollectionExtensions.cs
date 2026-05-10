using AbujaSocialMetaverse.API.Services;
using AbujaSocialMetaverse.Infrastructure.BackgroundJobs;
using AbujaSocialMetaverse.Infrastructure.Caching;
using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Infrastructure.RealTime;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Contracts;
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

namespace AbujaSocialMetaverse.API.Extensions;

/// <summary>
/// Extension methods for configuring application services in the DI container.
/// Each method encapsulates a single infrastructure concern following SRP.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Serilog logging with console and file sinks.
    /// </summary>
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder,
        LoggingOptions loggingOptions)
    {
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
        return builder;
    }

    /// <summary>
    /// Registers SignalR with Redis backplane for real-time communication.
    /// </summary>
    public static IServiceCollection AddSignalRWithRedis(
        this IServiceCollection services,
        RedisOptions redisOptions)
    {
        services.AddSignalR()
            .AddStackExchangeRedis(redisOptions.ConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = new RedisChannel(
                    redisOptions.ChannelPrefix,
                    RedisChannel.PatternMode.Literal);
            });

        return services;
    }

    /// <summary>
    /// Registers Redis connection multiplexer and cache services.
    /// </summary>
    public static IServiceCollection AddRedisServices(
        this IServiceCollection services,
        RedisOptions redisOptions)
    {
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ILocationCacheService, RedisLocationCacheService>();
        services.AddScoped<ICacheAdminService, RedisCacheAdminService>();

        return services;
    }

    /// <summary>
    /// Registers real-time services (connection tracking and SignalR).
    /// </summary>
    public static IServiceCollection AddRealTimeServices(this IServiceCollection services)
    {
        services.AddScoped<IConnectionTracker, RedisConnectionTracker>();
        services.AddScoped<IRealTimeService, SignalRRealTimeService>();

        return services;
    }

    /// <summary>
    /// Configures Hangfire with PostgreSQL storage for background job processing.
    /// </summary>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        DatabaseOptions dbOptions,
        HangfireOptions hangfireOptions)
    {
        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(dbOptions.ConnectionString)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = hangfireOptions.WorkerCount;
            options.Queues = hangfireOptions.Queues;
        });

        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with PostgreSQL, NTS, and snake_case naming.
    /// </summary>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        DatabaseOptions dbOptions)
    {
        services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseNpgsql(
                dbOptions.ConnectionString,
                npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .EnableDetailedErrors(dbOptions.EnableDetailedErrors)
            .EnableSensitiveDataLogging(dbOptions.EnableSensitiveDataLogging));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Configures CORS policy from application options.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        CorsOptions corsOptions)
    {
        services.AddCors(options =>
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

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        JwtOptions jwtOptions)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Registers the API-layer email link generator.
    /// </summary>
    public static IServiceCollection AddEmailLinkGenerator(this IServiceCollection services)
    {
        services.AddScoped<IEmailLinkGenerator, EmailLinkGenerator>();
        return services;
    }

    /// <summary>
    /// Configures fixed-window rate limiting for general and auth endpoints.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        RateLimitOptions rateLimitOptions)
    {
        services.AddRateLimiter(options =>
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

        return services;
    }
}
