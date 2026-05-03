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

// Voting API
var votingApi = builder
    .AddProject<Projects.EduVote_API>("voting-api")
    .WithHttpsEndpoint(port: 5959)
    .WithReference(db);

var pgAdmin = builder
    .AddContainer("pgadmin", "dpage/pgadmin4")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@admin.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin")
    .WithContainerName("pgadmin")
    .WithHttpEndpoint(5050, 80)
    .WithReference(postgres);
    
builder.Build().Run();
