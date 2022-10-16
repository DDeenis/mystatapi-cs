using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MystatAPI.Entity
{
    public class CityData
    {
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }

        [JsonPropertyName("timezone_name")]
        public string TimezoneName { get; set; }

        [JsonPropertyName("translate_key")]
        public string TranslateKey { get; set; }

        [JsonPropertyName("id_city")]
        public int IdCity { get; set; }

        [JsonPropertyName("market_status")]
        public int MarketStatus { get; set; }
    }
}
