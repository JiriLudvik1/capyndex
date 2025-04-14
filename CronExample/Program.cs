// See https://aka.ms/new-console-template for more information

using Capyndex;
using Capyndex.Database;
using Capyndex.Features.Upload;
using Capyndex.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using StackExchange.Redis;

var builder = new HostApplicationBuilder();

// Register necessary services
builder.Services.AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);
builder.Services.AddSingleton<RedisIndexService>();

var connectionString = "Host=localhost;Port=5432;Database=documents_db;Username=postgres;Password=password";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
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

// Build the host
var host = builder.Build();

// Create an instance of the endpoint directly
var uploadHandler = ActivatorUtilities.CreateInstance<UploadHandler>(host.Services);

// Prepare the request
var request = new UploadRequest { Content = "Hello World" };
var commandRequest = new UploadDocument { Request = request };


var result = await uploadHandler.ExecuteAsync(commandRequest, default);

Console.WriteLine(result);
return;

//will be used in API like this:
var resp = await commandRequest.ExecuteAsync(default);
Console.WriteLine(resp);