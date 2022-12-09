using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class LessonVisit
    {
        [JsonPropertyName("spec_id")]
        public int SpecId { get; set; }

        [JsonPropertyName("lesson_number")]
        public int LessonNumber { get; set; }

        [JsonPropertyName("status_was")]
        public int StatusWas { get; set; }

        [JsonPropertyName("class_work_mark")]
        public int? ClassworkMark { get; set; }

        [JsonPropertyName("control_work_mark")]
        public int? ControlWorkMark { get; set; }

        [JsonPropertyName("home_work_mark")]
        public int? HomeworkMark { get; set; }

        [JsonPropertyName("lab_work_mark")]
        public int? LabWorkMark { get; set; }

        [JsonPropertyName("date_visit")]
        public string Date { get; set; }

        [JsonPropertyName("spec_name")]
        public string SpecName { get; set; }

        [JsonPropertyName("lesson_theme")]
        public string LessonTheme { get; set; }

        [JsonPropertyName("teacher_name")]
        public string TeacherFullName { get; set; }
    }
}
