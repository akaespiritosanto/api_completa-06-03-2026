namespace criacao_api4.Services;

using criacao_api4.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

public class MusicBrainzService
{
    private readonly HttpClient _httpClient;

    public MusicBrainzService(HttpClient httpClient){
        _httpClient = httpClient;
    }

    public async Task<List<MusicBrainz>> GetRelease(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The parameter 'name' is mandatory.");
        }

        var encodedName = Uri.EscapeDataString(name.Trim());
        var url = $"release/?query=release:{encodedName}&fmt=json";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"MusicBrainz request failed with status code {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<MusicBrainzResponse>();
        if (result is null)
        {
            throw new InvalidOperationException("Invalid response from MusicBrainz.");
        }

        return result?.Releases ?? new List<MusicBrainz>();
    }

}
