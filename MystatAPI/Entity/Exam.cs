using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class Exam
    {
        [JsonPropertyName("exam_id")]
        public int Id { get; set; }

        [JsonPropertyName("spec")]
        public string SubjectName { get; set; }

        [JsonPropertyName("teacher")]
        public string? TeacherFullName { get; set; }

        [JsonPropertyName("mark")]
        public int? Mark { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("mark_type")]
        public int? MarkType { get; set; }

        [JsonPropertyName("comment_teacher")]
        public string? TeacherComment { get; set; }

        [JsonPropertyName("file_path")]
        public string? FilePath { get; set; }

        [JsonPropertyName("id_file")]
        public int? FileId { get; set; }
    }
}
