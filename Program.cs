using DotNetEnv;
using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

builder.Services.AddHttpClient<MusicBrainzService>(client =>
{
    client.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        $"MyMusicApp/1.0 ( {"EMAIL"} )");
});

builder.Services.AddScoped<BandServices>();
builder.Services.AddScoped<CdServices>();

var app = builder.Build();

var client = new HttpClient();
var response = await client.GetAsync("https://musicbrainz.org/ws/2/");
var content = await response.Content.ReadAsStringAsync();

Console.WriteLine(content);

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
