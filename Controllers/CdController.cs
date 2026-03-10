namespace criacao_api4.Controllers;
using Microsoft.AspNetCore.Mvc;
using criacao_api4.Services;
using criacao_api4.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("cds")]
public class CdsController : ControllerBase
{
    private readonly CdServices _service;
    private readonly ILogger<CdsController> _logger;

    public CdsController(CdServices service, ILogger<CdsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all CDs.
    /// </summary>
    /// <remarks>
    /// Returns the complete list of CDs currently stored in the database.
    /// </remarks>
    /// <response code="200">CDs retrieved successfully.</response>
    [HttpGet]
    public ActionResult<List<Cd>> GetAll()
    {
        var cds = _service.GetAll();
        return Ok(cds);
    }

    /// <summary>
    /// Retrieves a CD by ID.
    /// </summary>
    /// <remarks>
    /// Returns a single CD when the provided ID exists.
    /// </remarks>
    /// <param name="id">Unique identifier of the CD.</param>
    /// <response code="200">CD retrieved successfully.</response>
    /// <response code="404">No CD exists with the provided ID.</response>
    [HttpGet("{id:int}")]
    public ActionResult<Cd> GetById(int id)
    {
        var cd = _service.GetById(id);
        if (cd is null)
        {
            return NotFound($"No CD exists with the provided ID: {id}.");
        }
        return Ok(cd);
    }

    /// <summary>
    /// Retrieves CDs by band ID.
    /// </summary>
    /// <remarks>
    /// Returns all CDs associated with the specified band identifier.
    /// </remarks>
    /// <param name="bandId">Unique identifier of the band.</param>
    /// <response code="200">CDs retrieved successfully.</response>
    [HttpGet("band/{bandId:int}")]
    public ActionResult<List<Cd>> GetByBand(int bandId)
    {
        var cds = _service.GetByBand(bandId);
        return Ok(cds);
    }

    /// <summary>
    /// Searches CDs by name.
    /// </summary>
    /// <remarks>
    /// Performs a name-based search and returns matching CDs.
    /// </remarks>
    /// <param name="name">Name or partial name used for filtering CDs.</param>
    /// <response code="200">Matching CDs retrieved successfully.</response>
    /// <response code="400">The name query parameter is missing or invalid.</response>
    [HttpGet("search")]
    public ActionResult<List<Cd>> GetByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("The parameter 'name' is mandatory!");
        }

        var cds = _service.GetByName(name);
        return Ok(cds);
    }

    /// <summary>
    /// Creates a new CD.
    /// </summary>
    /// <remarks>
    /// Creates a CD using the request payload and returns the created resource location.
    /// </remarks>
    /// <param name="cd">CD data to create.</param>
    /// <response code="201">CD created successfully.</response>
    /// <response code="400">The request payload is invalid.</response>
    [HttpPost]
    public ActionResult<Cd> Create(Cd cd)
    {
        try
        {
            var createdCd = _service.Create(cd);
            return CreatedAtAction(nameof(GetById), new { id = createdCd.cdId }, createdCd);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Error while creating a CD.");
            return BadRequest(exception.Message);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while creating a CD.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating a CD.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }

    /// <summary>
    /// Updates an existing CD.
    /// </summary>
    /// <remarks>
    /// Updates the CD identified by the route ID using the provided payload.
    /// </remarks>
    /// <param name="id">Unique identifier of the CD.</param>
    /// <param name="cd">Updated CD data.</param>
    /// <response code="200">CD updated successfully.</response>
    /// <response code="400">The request payload is invalid.</response>
    /// <response code="404">No CD exists with the provided ID.</response>
    [HttpPut("{id:int}")]
    public ActionResult<Cd> Update(int id, Cd cd)
    {
        try
        {
            var updatedCd = _service.Update(id, cd);
            if (updatedCd is null)
            {
                return NotFound($"No CD exists with the provided ID: {id}.");
            }
            return Ok(updatedCd);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Error while updating CD with id {CdId}.", id);
            return BadRequest(exception.Message);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while updating CD with id {CdId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating CD with id {CdId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }

    /// <summary>
    /// Deletes a CD by ID.
    /// </summary>
    /// <remarks>
    /// Removes the CD identified by the route ID if it exists.
    /// </remarks>
    /// <param name="id">Unique identifier of the CD.</param>
    /// <response code="204">CD deleted successfully.</response>
    /// <response code="404">No CD exists with the provided ID.</response>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var deleted = _service.Delete(id);
            if (!deleted)
            {
                return NotFound($"No CD exists with the provided ID: {id}.");
            }

            return NoContent();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while deleting CD with id {CdId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting CD with id {CdId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }
}
