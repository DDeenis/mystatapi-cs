using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class DaySchedule
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; }

        [JsonPropertyName("finished_at")]
        public string FinishedAt { get; set; }

        [JsonPropertyName("lesson")]
        public int Lesson { get; set; }

        [JsonPropertyName("room_name")]
        public string RoomName { get; set; }

        [JsonPropertyName("subject_name")]
        public string SubjectName { get; set; }

        [JsonPropertyName("teacher_name")]
        public string TeacherFullName { get; set; }
    }
}
