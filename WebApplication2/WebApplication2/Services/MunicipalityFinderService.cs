using System.Text.Json;

public class MunicipalityFinderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MunicipalityFinderService> _logger;
    private readonly string _apiBaseUrl;

    public MunicipalityFinderService(
        HttpClient httpClient,
        ILogger<MunicipalityFinderService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ApiSettings:KommuneInfoApiBaseUrl"];
    }

    public async Task<(string MunicipalityNumber, string MunicipalityName, string CountyName)> FindMunicipalityFromGeoJsonAsync(string geoJson)
    {
        try
        {
            // Log the incoming GeoJSON for debugging
            _logger.LogInformation("Received GeoJSON: {GeoJson}", geoJson);

            // Parse the GeoJSON to extract coordinates
            using JsonDocument doc = JsonDocument.Parse(geoJson);
            JsonElement root = doc.RootElement;

            double[] coordinates = null;

            // Check if the root is a FeatureCollection
            if (root.GetProperty("type").GetString() == "FeatureCollection")
            {
                // Assuming we want the first feature's coordinates
                if (root.TryGetProperty("features", out JsonElement features) && features.GetArrayLength() > 0)
                {
                    var firstFeature = features[0];
                    if (firstFeature.TryGetProperty("geometry", out JsonElement geometry) &&
                        geometry.TryGetProperty("type", out JsonElement geometryType))
                    {
                        coordinates = ExtractCoordinatesFromGeometry(geometry, geometryType);
                    }
                }
            }
            else if (root.GetProperty("type").GetString() == "Feature")
            {
                // Handle single Feature
                if (root.TryGetProperty("geometry", out JsonElement geometry) &&
                    geometry.TryGetProperty("type", out JsonElement geometryType))
                {
                    coordinates = ExtractCoordinatesFromGeometry(geometry, geometryType);
                }
            }
            else if (root.GetProperty("type").GetString() == "Point")
            {
                // Handle direct Point
                coordinates = root.GetProperty("coordinates").EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray();
            }

            if (coordinates == null || coordinates.Length < 2)
            {
                _logger.LogWarning("Could not extract coordinates from GeoJSON");
                return (null, null, null);
            }

            // Assuming coordinates[0] is longitude (ost) and coordinates[1] is latitude (nord)
            double longitude = coordinates[0]; // ost
            double latitude = coordinates[1];   // nord

            // Make API call to find municipality
            var response = await _httpClient.GetAsync(
                $"{_apiBaseUrl}/punkt?nord={latitude}&ost={longitude}&koordsys=4258");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using JsonDocument respDoc = JsonDocument.Parse(content);

                var kommuneNummer = respDoc.RootElement
                    .GetProperty("kommunenummer").GetString();

                var kommuneNavn = respDoc.RootElement
                    .GetProperty("kommunenavn").GetString();

                var fylkesNavn = respDoc.RootElement
                    .GetProperty("fylkesnavn").GetString();

                // Note the capitalization of CountyName
                return (kommuneNummer, kommuneNavn, fylkesNavn);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API call failed with status code: {response.StatusCode}, Response: {errorContent}");

                // Return null values for all three elements
                return (null, null, null);
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON parsing error while finding municipality from GeoJSON");
            return (null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding municipality from GeoJSON");
            return (null, null, null);
        }
    }

    private double[] ExtractCoordinatesFromGeometry(JsonElement geometry, JsonElement geometryType)
    {
        string type = geometryType.GetString();

        try
        {
            return type switch
            {
                "Point" => geometry.GetProperty("coordinates").EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                "LineString" => geometry.GetProperty("coordinates")[0].EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                "Polygon" => geometry.GetProperty("coordinates")[0][0].EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                _ => throw new ArgumentException($"Unsupported geometry type: {type}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Could not extract coordinates for geometry type: {type}");
            return null;
        }
    }
}