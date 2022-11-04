using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class Activity
    {
        [JsonPropertyName("achievements_id")]
        public int AchievementsId { get; set; }

        [JsonPropertyName("achievements_type")]
        public int AchievementsType { get; set; }

        [JsonPropertyName("action")]
        public int Action { get; set; }

        [JsonPropertyName("badge")]
        public int Badge { get; set; }

        [JsonPropertyName("current_point")]
        public int CurrentPoint { get; set; }

        [JsonPropertyName("point_types_id")]
        public int PointTypesId { get; set; }
        
        [JsonPropertyName("subject_mark")]
        public int? SubjectMark { get; set; }
        
        [JsonPropertyName("achievements_name")]
        public string AchievementsName { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("point_types_name")]
        public string PointTypesName { get; set; }

        [JsonPropertyName("subject_name")]
        public string? SubjectName { get; set; }
        
        [JsonPropertyName("old_competition")]
        public bool OldCompetition { get; set; }
    }

    public static class AchievementNames
    {
        public static string LessonRate => "EVALUATION_LESSON_MARK";
        public static string PairVisit => "PAIR_VISIT";
        public static string Assesment => "ASSESMENT";
        public static string HomeworkCompleted => "HOMETASK_INTIME";
    }

    public static class PointTypesNames
    {
        public static string Diamond => "DIAMOND";
        public static string Coin => "COIN";
    }
}
