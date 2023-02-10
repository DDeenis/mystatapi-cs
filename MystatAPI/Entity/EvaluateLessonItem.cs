using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class EvaluateLessonItem
    {
        [JsonPropertyName("date_visit")]
        public string VisitDate { get; set; }

        [JsonPropertyName("fio_teach")]
        public string TeacherFullName { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("spec_name")]
        public string SpecName { get; set; }
    }
}
