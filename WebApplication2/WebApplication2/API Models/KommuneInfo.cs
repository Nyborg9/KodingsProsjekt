using System.Text.Json.Serialization;

namespace WebApplication2.API_Models
{
        public class KommuneInfo
        {
            [JsonPropertyName("fylkesnavn")]
            public string? Fylkesnavn { get; set; }

            [JsonPropertyName("kommunenavn")]
            public string? Kommunenavn { get; set; }

            [JsonPropertyName("kommunenummer")]
            public string? Kommunenummer { get; set; }
        }
    }