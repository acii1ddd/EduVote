using EduVote.API;
using EduVote.API.WebAppExtensions;
using EduVote.DAL.Postgresql;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddGrpcServices()
    .AddSwaggerConf()
    .AddDbContext(builder)
    .AddApiServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddAuthorizationPolitics()
    .AddDbInitializer()
    .AddRepositories();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    // options.ListenLocalhost(5858, o =>
    // {
    //     o.UseHttps();
    //     o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    // });
    
    options.ConfigureEndpointDefaults(opt =>
    {
        opt.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

await app.MapServicesAsync();

await app.RunAsync();