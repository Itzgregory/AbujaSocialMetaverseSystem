using AbujaSocialMetaverse.Shared.Configuration.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace AbujaSocialMetaverse.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql
                .UseNetTopologySuite()
                .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options, NullLogger<ApplicationDbContext>.Instance);
    }

    private static string ResolveConnectionString()
    {
        var direct = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(direct))
            return direct.Trim();

        var db = LoadDatabaseOptions();
        db.Validate();
        return db.ConnectionString;
    }

    /// <summary>
    /// Binds the same <c>Database</c> section as the API (<c>appsettings*.json</c>),
    /// then applies <c>DB_*</c> env overrides the same way <c>Program.cs</c> maps into configuration at runtime.
    /// </summary>
    private static DatabaseOptions LoadDatabaseOptions()
    {
        var apiContentRoot = ResolveApiContentRoot();
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(apiContentRoot)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var db = new DatabaseOptions();
        config.GetSection(db.SectionName).Bind(db);

        ApplyDatabaseEnvOverrides(db);

        return db;
    }

    private static void ApplyDatabaseEnvOverrides(DatabaseOptions db)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        if (!string.IsNullOrWhiteSpace(host))
            db.Host = host.Trim();

        var port = Environment.GetEnvironmentVariable("DB_PORT");
        if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port.Trim(), out var p))
            db.Port = p;

        var name = Environment.GetEnvironmentVariable("DB_NAME");
        if (!string.IsNullOrWhiteSpace(name))
            db.Name = name.Trim();

        var user = Environment.GetEnvironmentVariable("DB_USER");
        if (!string.IsNullOrWhiteSpace(user))
            db.Username = user.Trim();

        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (password != null)
            db.Password = password;
    }

    private static string ResolveApiContentRoot()
    {
        var assemblyLocation = typeof(ApplicationDbContext).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Could not resolve Infrastructure assembly directory.");

        // .../src/Infrastructure/bin/Debug/net10.0 -> .../src/AbujaSocialMetaverse.API
        var srcDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
        return Path.Combine(srcDir, "AbujaSocialMetaverse.API");
    }
}
