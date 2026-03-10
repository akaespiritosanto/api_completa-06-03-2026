using criacao_api4.Models;
using Microsoft.EntityFrameworkCore;

namespace criacao_api4.Services;

public class BandServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<BandServices> _logger;

    public BandServices(AppDbContext context, ILogger<BandServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Band> GetAll()
    {
        return _context.Bands
            .OrderBy(b => b.bandId)
            .ToList();
    }

    public Band? GetById(int id)
    {
        return _context.Bands.FirstOrDefault(b => b.bandId == id);
    }

    public Band? GetWithCds(int id)
    {
        var band = _context.Bands.FirstOrDefault(b => b.bandId == id);
        if (band is null)
        {
            return null;
        }

        var cds = _context.Cds
            .Where(cd => cd.bandId == id)
            .OrderBy(cd => cd.cdId)
            .ToList();

        return new BandWithCds
        {
            bandId = band.bandId,
            name = band.name,
            description = band.description,
            cds = cds
        };
    }

    public Band Create(Band band)
    {
        ValidateBand(band);

        band.bandId = 0;

        _context.Bands.Add(band);
        _context.SaveChanges();
        return band;
    }

    public Band? Update(int id, Band band)
    {
        ValidateBand(band);

        var existingBand = _context.Bands.FirstOrDefault(b => b.bandId == id);
        if (existingBand is null)
        {
            return null;
        }

        existingBand.name = band.name.Trim();
        existingBand.description = (band.description ?? string.Empty).Trim();

        _context.SaveChanges();
        return existingBand;
    }

    public bool Delete(int id)
    {
        var band = _context.Bands.FirstOrDefault(b => b.bandId == id);
        if (band is null)
        {
            return false;
        }

        _context.Bands.Remove(band);
        _context.SaveChanges();
        ResetSqliteSequence("Bands", "bandId");
        ResetSqliteSequence("Cds", "cdId");
        return true;
    }

    private void ResetSqliteSequence(string tableName, string idColumn)
    {
        if (!_context.Database.IsSqlite())
        {
            return;
        }

        try
        {
            _context.Database.ExecuteSqlRaw($"DELETE FROM sqlite_sequence WHERE name='{tableName}';");
            _context.Database.ExecuteSqlRaw(
                $"INSERT INTO sqlite_sequence(name, seq) SELECT '{tableName}', IFNULL(MAX({idColumn}), 0) FROM {tableName};");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not reset SQLite sequence for table {TableName}.", tableName);
        }
    }

    private void ValidateBand(Band band)
    {
        if (band is null)
        {
            _logger.LogError("Validation error in BandServices: Band is required.");
            throw new ArgumentException("Band is required.");
        }

        if (string.IsNullOrWhiteSpace(band.name))
        {
            _logger.LogError("Validation error in BandServices: Band name is required.");
            throw new ArgumentException("Band name is required.");
        }
    }
}
