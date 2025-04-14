using Capyndex.Database;
using Capyndex.Infrastructure;
using Capyndex.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services
       .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Auth:JwtKey"])
       .AddAuthorization()
       .AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All)
       .SwaggerDocument();

// Application services
builder.Services.AddSingleton<SearchIndex>();
builder.Services.AddSingleton<RedisIndexService>();

// Redis, Postgres
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // The minimum number of connections in the pool
            MinPoolSize = 5,

            // The maximum number of connections in the pool
            MaxPoolSize = 100,

            // Connection Lifetime (seconds) - how long a connection can remain unused before being discarded
            ConnectionIdleLifetime = 300,

            // Enable connection pruning for long-running applications
            ConnectionPruningInterval = 10,

            // Timeout for command execution (seconds)
            CommandTimeout = 30
        };

        options.UseNpgsql(
            connectionStringBuilder.ConnectionString,
            npgsqlOptions =>
            {
                // Enable retrying on connection failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
    });

var app = builder.Build();
app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints(
       c =>
       {
           c.Binding.ReflectionCache.AddFromCapyndex();
           c.Errors.UseProblemDetails();
       })
   .UseCustomExceptionHandler()
   .UseSwaggerGen();
app.Run();

public partial class Program;