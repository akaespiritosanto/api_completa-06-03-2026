using DotNetEnv;
using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

Env.Load();

static string ResolveSqliteConnectionString(string rawConnectionString, string contentRootPath)
{
    if (string.IsNullOrWhiteSpace(rawConnectionString))
    {
        throw new InvalidOperationException("Missing SQLite connection string. Set CONNECTION_STRING (or ConnectionStrings:Default).");
    }

    var sqliteBuilder = new SqliteConnectionStringBuilder(rawConnectionString);

    if (!string.IsNullOrWhiteSpace(sqliteBuilder.DataSource) && !Path.IsPathRooted(sqliteBuilder.DataSource))
    {
        // Prefer a mounted /data folder when running in a container so the DB persists on the host/volume.
        if (string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase)
            && Directory.Exists("/data"))
        {
            sqliteBuilder.DataSource = Path.Combine("/data", sqliteBuilder.DataSource);
        }
        else if (!string.IsNullOrWhiteSpace(contentRootPath))
        {
            sqliteBuilder.DataSource = Path.Combine(contentRootPath, sqliteBuilder.DataSource);
        }
    }

    if (!string.IsNullOrWhiteSpace(sqliteBuilder.DataSource))
    {
        var directory = Path.GetDirectoryName(sqliteBuilder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Cannot create directory for SQLite database: {directory}", exception);
            }
        }
    }

    return sqliteBuilder.ToString();
}

var configuredConnectionString =
    Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration["ConnectionStrings:Default"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnectionString(configuredConnectionString ?? string.Empty, builder.Environment.ContentRootPath)));

builder.Services.AddHttpClient<MusicBrainzService>(client =>
{
    var email = Environment.GetEnvironmentVariable("EMAIL") ?? "unknown";
    client.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        $"MyMusicApp/1.0 ({email})");
});

builder.Services.AddScoped<BandServices>();
builder.Services.AddScoped<CdServices>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://musicbrainz.org/ws/2/");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "MusicBrainz startup check failed.");
    }
}

// Create SQLite database automatically on first run.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment() && !string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
