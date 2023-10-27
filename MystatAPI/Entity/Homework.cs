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
        public HomeworkComment? HomeworkComment { get; set; }

        [JsonPropertyName("homework_stud")]
        public UploadedHomeworkInfo UploadedHomework { get; set; }

        [JsonPropertyName("id_group")]
        public int GroupId { get; set; }

        [JsonPropertyName("id_spec")]
        public int SubjectId { get; set; }

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

    public class HomeworkComment
    {
        [JsonPropertyName("text_comment")]
        public string? Text { get; set; }

        [JsonPropertyName("attachment")]
        public string? Attachment { get; set; }

        [JsonPropertyName("attachment_path")]
        public string? AttachmentPath { get; set; }

        [JsonPropertyName("date_updated")]
        public string UpdateDate { get; set; }
    }

    public class HomeworkCount
    {
        [JsonPropertyName("counter_type")]
        public HomeworkStatus Status { get; set; }

        [JsonPropertyName("counter")]
        public int Count { get; set; }
    }

    public enum HomeworkStatus
    {
        Checked = 1,
        Uploaded = 2,
        Active = 3,
        Deleted = 5,
        Overdue = 6,
    }

    public enum HomeworkType
    {
        Homework,
        Lab,
    }

    public class HomeworkFile
    {
        public string Name { get; }
        public string Extension { get; }
        public byte[] Bytes { get; }

        public HomeworkFile(string name, string extension, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
            Extension = extension;
        }
    }
}
