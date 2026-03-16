using criacao_api4.Models;
using criacao_api4.Dtos;
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

    public PagedResult<Band> GetAll(PaginationQuery pagination)
    {
        var (pageNumber, pageSize, skip) = pagination.Normalize();
        var query = _context.Bands.AsNoTracking().OrderBy(b => b.bandId);
        var totalCount = query.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Band>
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            items = query.Skip(skip).Take(pageSize).ToList()
        };
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
            .AsNoTracking()
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
            var (deleteSql, insertSql) = GetSqliteSequenceResetSql(tableName, idColumn);
            _context.Database.ExecuteSqlRaw(deleteSql);
            _context.Database.ExecuteSqlRaw(insertSql);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not reset SQLite sequence for table {TableName}.", tableName);
        }
    }

    private static (string DeleteSql, string InsertSql) GetSqliteSequenceResetSql(string tableName, string idColumn)
    {
        if (tableName == "Bands" && idColumn == "bandId")
        {
            return (
                "DELETE FROM sqlite_sequence WHERE name='Bands';",
                "INSERT INTO sqlite_sequence(name, seq) SELECT 'Bands', IFNULL(MAX(bandId), 0) FROM Bands;"
            );
        }

        if (tableName == "Cds" && idColumn == "cdId")
        {
            return (
                "DELETE FROM sqlite_sequence WHERE name='Cds';",
                "INSERT INTO sqlite_sequence(name, seq) SELECT 'Cds', IFNULL(MAX(cdId), 0) FROM Cds;"
            );
        }

        throw new ArgumentException("Invalid SQLite table or id column.");
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
