namespace criacao_api4.Controllers;
using Microsoft.AspNetCore.Mvc;
using criacao_api4.Services;
using criacao_api4.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("bands")]
public class BandsController : ControllerBase
{
    private readonly BandServices _service;
    private readonly ILogger<BandsController> _logger;

    public BandsController(BandServices service, ILogger<BandsController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves all bands.
    /// </summary>
    /// <remarks>
    /// Returns the complete list of bands currently stored in the database.
    /// </remarks>
    /// <response code="200">Bands retrieved successfully.</response>
    [HttpGet]
    public ActionResult<List<Band>> GetAll()
    {
        return Ok(_service.GetAll());
    }
    /// <summary>
    /// Retrieves a band by ID.
    /// </summary>
    /// <remarks>
    /// Returns a single band when the provided ID exists.
    /// </remarks>
    /// <param name="id">Unique identifier of the band.</param>
    /// <response code="200">Band retrieved successfully.</response>
    /// <response code="404">No band exists with the provided ID.</response>
    [HttpGet("{id:int}")]
    public ActionResult<Band> GetById(int id)
    {
        var band = _service.GetById(id);
        if (band is null)
        {
            return NotFound($"No band exists with the provided ID: {id}.");
        }
        return Ok(band);
    }

    /// <summary>
    /// Retrieves a band by ID including related CDs.
    /// </summary>
    /// <remarks>
    /// Returns a single band with its associated CD collection when the ID exists.
    /// </remarks>
    /// <param name="id">Unique identifier of the band.</param>
    /// <response code="200">Band with related CDs retrieved successfully.</response>
    /// <response code="404">No band exists with the provided ID.</response>
    [HttpGet("with-cds/{id:int}")]
    public ActionResult<Band> GetWithCds(int id)
    {
        var bandWithCds = _service.GetWithCds(id);
        if (bandWithCds is null)
        {
            return NotFound($"No band exists with the provided ID: {id}.");
        }
        return Ok(bandWithCds);
    }

    /// <summary>
    /// Creates a new band.
    /// </summary>
    /// <remarks>
    /// Creates a band using the request payload and returns the created resource location.
    /// </remarks>
    /// <param name="band">Band data to create.</param>
    /// <response code="201">Band created successfully.</response>
    /// <response code="400">The request payload is invalid.</response>
    [HttpPost]
    public ActionResult<Band> Create(Band band)
    {
        try
        {
            var createdBand = _service.Create(band);
            return CreatedAtAction(nameof(GetById), new { id = createdBand.bandId }, createdBand);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Error while creating a band.");
            return BadRequest(exception.Message);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while creating a band.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating a band.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }

    /// <summary>
    /// Updates an existing band.
    /// </summary>
    /// <remarks>
    /// Updates the band identified by the route ID using the provided payload.
    /// </remarks>
    /// <param name="id">Unique identifier of the band.</param>
    /// <param name="band">Updated band data.</param>
    /// <response code="200">Band updated successfully.</response>
    /// <response code="400">The request payload is invalid.</response>
    /// <response code="404">No band exists with the provided ID.</response>
    [HttpPut("{id:int}")]
    public ActionResult<Band> Update(int id, Band band)
    {
        try
        {
            var updatedBand = _service.Update(id, band);
            if (updatedBand is null)
            {
                return NotFound($"No band exists with the provided ID: {id}.");
            }
            return Ok(updatedBand);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Error while updating band with id {BandId}.", id);
            return BadRequest(exception.Message);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while updating band with id {BandId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating band with id {BandId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }

    /// <summary>
    /// Deletes a band by ID.
    /// </summary>
    /// <remarks>
    /// Removes the band identified by the route ID if it exists.
    /// </remarks>
    /// <param name="id">Unique identifier of the band.</param>
    /// <response code="204">Band deleted successfully.</response>
    /// <response code="404">No band exists with the provided ID.</response>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) 
    {
        try
        {
            var deleted = _service.Delete(id);
            if (!deleted)
            {
                return NotFound($"No band exists with the provided ID: {id}.");
            }

            return NoContent();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error while deleting band with id {BandId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Database error.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting band with id {BandId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error.");
        }
    }
}
