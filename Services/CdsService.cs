using criacao_api4.Models;
using Microsoft.EntityFrameworkCore;

namespace criacao_api4.Services;

public class CdServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<CdServices> _logger;

    public CdServices(AppDbContext context, ILogger<CdServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Cd> GetAll()
    {
        return _context.Cds
            .OrderBy(cd => cd.cdId)
            .ToList();
    }

    public Cd? GetById(int id)
    {
        return _context.Cds.FirstOrDefault(cd => cd.cdId == id);
    }

    public List<Cd> GetByBand(int bandId)
    {
        return _context.Cds
            .Where(cd => cd.bandId == bandId)
            .OrderBy(cd => cd.cdId)
            .ToList();
    }

    public List<Cd> GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new List<Cd>();
        }

        var normalizedName = name.Trim().ToLower();

        return _context.Cds
            .Where(cd => cd.name.ToLower().Contains(normalizedName))
            .OrderBy(cd => cd.cdId)
            .ToList();
    }

    public Cd Create(Cd cd)
    {
        ValidateCd(cd);
        EnsureBandExists(cd.bandId);

        cd.cdId = 0;
        cd.name = cd.name.Trim();

        _context.Cds.Add(cd);
        _context.SaveChanges();
        return cd;
    }

    public Cd? Update(int id, Cd cd)
    {
        ValidateCd(cd);
        EnsureBandExists(cd.bandId);

        var existingCd = _context.Cds.FirstOrDefault(item => item.cdId == id);
        if (existingCd is null)
        {
            return null;
        }

        existingCd.name = cd.name.Trim();
        existingCd.bandId = cd.bandId;
        existingCd.rating = cd.rating;

        _context.SaveChanges();
        return existingCd;
    }

    public bool Delete(int id)
    {
        var cd = _context.Cds.FirstOrDefault(item => item.cdId == id);
        if (cd is null)
        {
            return false;
        }

        _context.Cds.Remove(cd);
        _context.SaveChanges();
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

    private void ValidateCd(Cd cd)
    {
        if (cd is null)
        {
            _logger.LogError("Validation error in CdServices: CD is required.");
            throw new ArgumentException("CD is required.");
        }

        if (string.IsNullOrWhiteSpace(cd.name))
        {
            _logger.LogError("Validation error in CdServices: CD name is required.");
            throw new ArgumentException("CD name is required.");
        }

        if (cd.rating < 1 || cd.rating > 5)
        {
            _logger.LogError("Validation error in CdServices: Rating must be between 1 and 5.");
            throw new ArgumentException("Rating must be between 1 and 5.");
        }
    }

// ############################################################################################################

    private void EnsureBandExists(int bandId)
    {
        var bandExists = _context.Bands.Any(b => b.bandId == bandId);
        if (!bandExists)
        {
            _logger.LogError("Validation error in CdServices: The band associated with this CD does not exist.");
            throw new ArgumentException("The band associated with this CD does not exist.");
        }
    }
}
