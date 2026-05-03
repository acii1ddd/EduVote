using EduVote.API.Services;
using EduVote.DAL.Postgresql.Context;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc()
    .AddJsonTranscoding();

builder.Services.AddGrpcReflection();

// swagger conf
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "gRPC transcoding", Version = "v1"
    });
});

builder.Services.AddDbContext<EduVoteDbContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("eduvote-db");
    
    options.UseNpgsql(connString);
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

    Console.WriteLine("Swagger with gRPC transcoding is available on: https://localhost:5858/swagger/index.html");
    
    app.MapGrpcReflectionService()
        .AllowAnonymous();
}

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();