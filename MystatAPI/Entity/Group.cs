using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class Group
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("group_status")]
        public int GroupStatus { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_primary")]
        public bool IsPrimary { get; set; }
    }

    public class GroupInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("specs")]
        public GroupSpec[] Specs { get; set; }
    }

    public class GroupSpec
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }
    }
}
