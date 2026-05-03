var builder = DistributedApplication.CreateBuilder(args);

// Postgres
var postgres = builder
    .AddPostgres("postgres")
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

// var pgUser = builder.AddParameter("pguser", defaultValue: "postgres");
// var pgPassword = builder.AddParameter("pgpassword", defaultValue: "p@ssword123", secret: true);

// var postgres = builder
//     // 2. Передаем параметры в метод
//     .AddPostgres("postgres", pgUser, pgPassword)
//     .WithDataVolume("voting-db")
//     .WithContainerName("voting-postgres-container")
//     // 3. Рекомендуется зафиксировать порт, чтобы не вводить случайный порт в DBeaver
//     .WithHostPort(5432);
//
// var db = postgres.AddDatabase("voting-db");

// var pgAdmin = builder.AddContainer("pgadmin", "dpage/pgadmin4")
//     .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@admin.com")
//     .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin")
//     .WithHttpEndpoint(80, 5050);
    
builder.Build().Run();
