using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class LeadersSummary
    {
        [JsonPropertyName("totalCount")]
        public int? Count { get; set; }

        [JsonPropertyName("studentPosition")]
        public int? Position { get; set; }

        [JsonPropertyName("weekDiff")]
        public int WeekDifference { get; set; }

        [JsonPropertyName("monthDiff")]
        public int MonthDifference { get; set; }
    }
}
