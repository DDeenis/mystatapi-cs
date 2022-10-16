using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class Homework
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name_spec")]
        public string SubjectName { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("creation_time")]
        public string CreationTime { get; set; }

        [JsonPropertyName("completion_time")]
        public string CompletionTime { get; set; }

        [JsonPropertyName("overdue_time")]
        public string OverdueTime { get; set; }

        [JsonPropertyName("cover_image")]
        public string CoverImage { get; set; }

        [JsonPropertyName("file_path")]
        public string FilePath { get; set; }

        [JsonPropertyName("filename")]
        public string? FileName { get; set; }

        [JsonPropertyName("fio_teach")]
        public string TeacherFullName { get; set; }

        [JsonPropertyName("homework_comment")]
        public string? HomeworkComment { get; set; }

        [JsonPropertyName("homework_stud")]
        public UploadedHomeworkInfo UploadedHomework { get; set; }

        [JsonPropertyName("id_group")]
        public int GroupId { get; set; }

        [JsonPropertyName("id_spec")]
        public int SUbjectId { get; set; }

        [JsonPropertyName("id_teach")]
        public int TeacherId { get; set; }

        [JsonPropertyName("status")]
        public HomeworkStatus Status { get; set; }
    }

    public class UploadedHomeworkInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("creation_time")]
        public string CreationTime { get; set; }

        [JsonPropertyName("file_path")]
        public string? FilePath { get; set; }

        [JsonPropertyName("filename")]
        public string? FileName { get; set; }

        [JsonPropertyName("mark")]
        public int? Mark { get; set; }

        [JsonPropertyName("stud_answer")]
        public string? StudentAnswer { get; set; }

        [JsonPropertyName("tmp_file")]
        public string? TempFile { get; set; }

        [JsonPropertyName("auto_mark")]
        public bool AutoMark { get; set; }
    }
    
    public enum HomeworkStatus
    {
        Overdue,
        Checked,
        Uploaded,
        Active,
        Deleted = 5,
    }
}
