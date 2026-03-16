using criacao_api4.Models;
using criacao_api4.Dtos;
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

    public PagedResult<Cd> GetAll(PaginationQuery pagination)
    {
        var (pageNumber, pageSize, skip) = pagination.Normalize();
        var query = _context.Cds.AsNoTracking().OrderBy(cd => cd.cdId);
        var totalCount = query.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Cd>
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            items = query.Skip(skip).Take(pageSize).ToList()
        };
    }

    public Cd? GetById(int id)
    {
        return _context.Cds.FirstOrDefault(cd => cd.cdId == id);
    }

    public PagedResult<Cd> GetByBand(int bandId, PaginationQuery pagination)
    {
        var (pageNumber, pageSize, skip) = pagination.Normalize();
        var query = _context.Cds.AsNoTracking().Where(cd => cd.bandId == bandId).OrderBy(cd => cd.cdId);
        var totalCount = query.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Cd>
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            items = query.Skip(skip).Take(pageSize).ToList()
        };
    }

    public PagedResult<Cd> GetByName(string name, PaginationQuery pagination)
    {
        var (pageNumber, pageSize, skip) = pagination.Normalize();
        if (string.IsNullOrWhiteSpace(name))
        {
            return new PagedResult<Cd>
            {
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalCount = 0,
                totalPages = 0,
                items = new List<Cd>()
            };
        }

        var normalizedName = name.Trim().ToLower();

        var query = _context.Cds.AsNoTracking()
            .Where(cd => cd.name.ToLower().Contains(normalizedName))
            .OrderBy(cd => cd.cdId);

        var totalCount = query.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Cd>
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            items = query.Skip(skip).Take(pageSize).ToList()
        };
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
        if (tableName == "Cds" && idColumn == "cdId")
        {
            return (
                "DELETE FROM sqlite_sequence WHERE name='Cds';",
                "INSERT INTO sqlite_sequence(name, seq) SELECT 'Cds', IFNULL(MAX(cdId), 0) FROM Cds;"
            );
        }

        throw new ArgumentException("Invalid SQLite table or id column.");
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
