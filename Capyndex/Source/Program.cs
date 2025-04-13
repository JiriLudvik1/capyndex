using Capyndex.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Auth:JwtKey"])
    .AddAuthorization()
    .AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All)
    .SwaggerDocument();

builder.Services.AddSingleton<SearchIndex>();

var app = builder.Build();
app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints(
        c =>
        {
            c.Binding.ReflectionCache.AddFromCapyndex();
            c.Errors.UseProblemDetails();
        })
    .UseSwaggerGen();
app.Run();

public partial class Program;