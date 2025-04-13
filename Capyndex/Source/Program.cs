using Capyndex.Infrastructure;
using Capyndex.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services
       .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Auth:JwtKey"])
       .AddAuthorization()
       .AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All)
       .SwaggerDocument();

builder.Services.AddSingleton<SearchIndex>();
builder.Services.AddSingleton<RedisIndexService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

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