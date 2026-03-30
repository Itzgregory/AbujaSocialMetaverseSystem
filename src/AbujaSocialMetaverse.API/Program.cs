using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

// Load .env 
Env.Load();

// Build connection strings from env vars 
var dbConnection = $"Host={Env.GetString("DB_HOST")};Port={Env.GetString("DB_PORT")};Database={Env.GetString("DB_NAME")};Username={Env.GetString("DB_USER")};Password={Env.GetString("DB_PASSWORD")}";
var redisConnection = Env.GetString("REDIS_CONNECTION");

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
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/abuja-metaverse-.log", rollingInterval: RollingInterval.Day)
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
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseNpgsql(
//         dbConnection,
//         npgsql => npgsql.UseNetTopologySuite()));

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnityClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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

// Build 
var app = builder.Build();

// Middleware Pipeline 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowUnityClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SignalR Hubs 
// app.MapHub<AvatarHub>("/hubs/avatar");
// app.MapHub<ChatHub>("/hubs/chat");
// Commented until hubs are created in Phase 5

app.Run();