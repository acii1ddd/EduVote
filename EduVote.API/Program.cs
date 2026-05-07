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
    .AddDbInitializer()
    .AddRepositories();

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
await app.MapServicesAsync();

await app.RunAsync();