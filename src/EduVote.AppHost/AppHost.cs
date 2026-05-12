var builder = DistributedApplication.CreateBuilder(args);

// Postgres
var pgUser = builder
    .AddParameter("pguser", "user");

var pgPassword = builder
    .AddParameter("pgpassword",  "123", secret: true);

var postgres = builder
    .AddPostgres("postgres", pgUser, pgPassword)
    .WithImage("postgres:17.6")
    .WithContainerName("voting-db")
    .WithDataVolume("voting-db");

var db = postgres
    .AddDatabase("eduvote-db");

// Minio
var minioUser = builder
    .AddParameter("minio-user", "user");

var minioPassword = builder
    .AddParameter("minio-password", "admin123");

var minio = builder
    .AddMinioContainer("minio", minioUser, minioPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// Voting API
var votingApi = builder
    .AddProject<Projects.EduVote_API>("voting-api")
    .WithHttpsEndpoint(port: 5959)
    .WithReference(db)
    .WaitFor(db)
    .WithReference(minio)
    .WaitFor(minio);

var pgAdmin = builder
    .AddContainer("pgadmin", "dpage/pgadmin4")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@admin.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin")
    .WithContainerName("pgadmin")
    .WithHttpEndpoint(5050, 80)
    .WithReference(postgres);
    
var frontend = builder.AddViteApp("frontend", "../EduVote.Web")
    .WithReference(votingApi)
    .WaitFor(votingApi);

votingApi.PublishWithContainerFiles(frontend, "wwwroot");

builder.Build().Run();
