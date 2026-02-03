using System.Text.Json;
using ShazamIO.Constants;
using ShazamIO.Enums;
using ShazamIO.Exceptions;
using ShazamIO.Interfaces;

namespace ShazamIO.Services;

/// <summary>
/// Service for retrieving geographic-based playlists and chart data.
/// </summary>
public class GeoService
{
    private readonly IHttpClient _client;
    private JsonDocument? _cachedLocations;

    public GeoService(IHttpClient client)
    {
        _client = client;
    }

    private async Task<JsonElement> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        _cachedLocations ??= await _client.GetAsync(ShazamUrls.Locations, cancellationToken: cancellationToken);
        return _cachedLocations.RootElement;
    }

    /// <summary>
    /// Gets the playlist ID for a specific country.
    /// </summary>
    /// <param name="countryCode">ISO 3166-3 alpha-2 code (e.g., US, GB, NL)</param>
    public async Task<string> GetCountryPlaylistAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsAsync(cancellationToken);
        
        if (locations.TryGetProperty("countries", out var countries))
        {
            foreach (var country in countries.EnumerateArray())
            {
                if (country.TryGetProperty("id", out var id) && 
                    id.GetString()?.Equals(countryCode, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (country.TryGetProperty("listid", out var listId))
                    {
                        return listId.GetString() ?? throw new BadCountryNameException("Country listid is null");
                    }
                }
            }
        }

        throw new BadCountryNameException($"Country not found: {countryCode}");
    }

    /// <summary>
    /// Gets the playlist ID for a specific city in a country.
    /// </summary>
    /// <param name="countryCode">ISO 3166-3 alpha-2 code (e.g., US, GB, NL)</param>
    /// <param name="cityName">City name (e.g., "New York", "London")</param>
    public async Task<string> GetCityPlaylistAsync(string countryCode, string cityName, CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsAsync(cancellationToken);
        
        if (locations.TryGetProperty("countries", out var countries))
        {
            foreach (var country in countries.EnumerateArray())
            {
                if (country.TryGetProperty("id", out var id) && 
                    id.GetString()?.Equals(countryCode, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (country.TryGetProperty("cities", out var cities))
                    {
                        foreach (var city in cities.EnumerateArray())
                        {
                            if (city.TryGetProperty("name", out var name) && 
                                name.GetString()?.Equals(cityName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                if (city.TryGetProperty("listid", out var listId))
                                {
                                    return listId.GetString() ?? throw new BadCityNameException("City listid is null");
                                }
                            }
                        }
                    }
                }
            }
        }

        throw new BadCityNameException($"City not found: {cityName} in {countryCode}");
    }

    /// <summary>
    /// Gets the global genre playlist ID.
    /// </summary>
    public async Task<string> GetGenreAsync(GenreMusic genre, CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsAsync(cancellationToken);
        var genreUrlName = genre.ToUrlName();
        
        if (locations.TryGetProperty("global", out var global))
        {
            if (global.TryGetProperty("genres", out var genres))
            {
                foreach (var genreItem in genres.EnumerateArray())
                {
                    if (genreItem.TryGetProperty("urlName", out var urlName) && 
                        urlName.GetString()?.Equals(genreUrlName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (genreItem.TryGetProperty("listid", out var listId))
                        {
                            return listId.GetString() ?? throw new BadParseDataException("Genre listid is null");
                        }
                    }
                }
            }
            throw new BadParseDataException("Genres key not found in shazam locations");
        }

        throw new BadParseDataException("Global key not found in shazam locations");
    }

    /// <summary>
    /// Gets the top global playlist ID.
    /// </summary>
    public async Task<string> GetTopAsync(CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsAsync(cancellationToken);
        
        if (locations.TryGetProperty("global", out var global))
        {
            if (global.TryGetProperty("top", out var top))
            {
                if (top.TryGetProperty("listid", out var listId))
                {
                    return listId.GetString() ?? throw new BadParseDataException("Top listid is null");
                }
            }
            throw new BadParseDataException("Top key not found in shazam locations");
        }

        throw new BadParseDataException("Global key not found in shazam locations");
    }

    /// <summary>
    /// Gets the genre playlist ID for a specific country.
    /// </summary>
    public async Task<string> GetGenreFromCountryAsync(string countryCode, GenreMusic genre, CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsAsync(cancellationToken);
        var genreUrlName = genre.ToUrlName();
        
        if (locations.TryGetProperty("countries", out var countries))
        {
            foreach (var country in countries.EnumerateArray())
            {
                if (country.TryGetProperty("id", out var id) && 
                    id.GetString()?.Equals(countryCode, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (country.TryGetProperty("genres", out var genres))
                    {
                        foreach (var genreItem in genres.EnumerateArray())
                        {
                            if (genreItem.TryGetProperty("urlName", out var urlName) && 
                                urlName.GetString()?.Equals(genreUrlName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                if (genreItem.TryGetProperty("listid", out var listId))
                                {
                                    return listId.GetString() ?? throw new BadCityNameException("Genre listid is null");
                                }
                            }
                        }
                    }
                    throw new BadParseDataException("Genres key not found for country");
                }
            }
        }

        throw new BadCityNameException($"Genre {genreUrlName} not found in country {countryCode}");
    }
}
