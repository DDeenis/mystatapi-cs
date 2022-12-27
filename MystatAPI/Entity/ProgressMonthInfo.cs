using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class ProgressMonthInfo
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("points")]
        public int? Points { get; set; }

        [JsonPropertyName("previous_points")]
        public int? PrevoiusPoint { get; set; }

        [JsonPropertyName("has_rasp")]
        public bool? HasSchedule { get; set; }
    }
}
