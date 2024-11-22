using System.Text.Json.Serialization;

namespace WebApplication2.API_Models
{
        public class MunicipalityInfo
        {
            [JsonPropertyName("fylkesnavn")]
            public string? CountyName { get; set; }

            [JsonPropertyName("kommunenavn")]
            public string? MunicipalityName { get; set; }

            [JsonPropertyName("kommunenummer")]
            public string? MunicipalityNumber { get; set; }
        }
    }