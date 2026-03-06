namespace criacao_api4.Controllers;

using criacao_api4.Services;
using criacao_api4.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("releases")]
public class MusicBrainzController : ControllerBase
{
    private readonly MusicBrainzService _musicService;
    private readonly ILogger<MusicBrainzController> _logger;

    public MusicBrainzController(MusicBrainzService musicService, ILogger<MusicBrainzController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves releases from MusicBrainz by release name.
    /// </summary>
    /// <remarks>
    /// Returns a list of matching releases from the external MusicBrainz API.
    /// </remarks>
    /// <param name="name">Release name used to search on MusicBrainz.</param>
    /// <response code="200">Releases retrieved successfully.</response>
    /// <response code="400">The provided release name is invalid.</response>
    /// <response code="502">MusicBrainz is unavailable or returned an invalid response.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{name}")]
    public async Task<ActionResult<List<MusicBrainz>>> GetRelease(string name)
    {
        try
        {
            var releases = await _musicService.GetRelease(name);
            return Ok(releases);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Invalid release name provided: {ReleaseName}", name);
            return BadRequest(exception.Message);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            _logger.LogError(exception, "Error while calling MusicBrainz for release name {ReleaseName}", name);
            return StatusCode(StatusCodes.Status502BadGateway, "Unable to fetch data from MusicBrainz right now.");
        }
    }
}
