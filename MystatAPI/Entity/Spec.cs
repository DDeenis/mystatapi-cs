using System.Text.Json.Serialization;

namespace MystatAPI.Entity
{
    public class Spec
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = null!;
    }
}
