using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class ProfileInfo
    {
        [JsonPropertyName("student_id")]
        public int Id { get; set; }

        [JsonPropertyName("current_group_id")]
        public int CurrentGroupId { get; set; }

        [JsonPropertyName("current_group_status")]
        public int CurrentGroupStatus { get; set; }

        [JsonPropertyName("stream_id")]
        public int StreamId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("achieves_count")]
        public int AchievesCount { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonPropertyName("stream_name")]
        public string StreamName { get; set; }

        [JsonPropertyName("groups")]
        public Group[] Groups { get; set; }

        [JsonPropertyName("visibility")]
        public VisibilityInfo Visibility { get; set; }
    }

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

    public class VisibilityInfo
    {
        [JsonPropertyName("is_birthday")]
        public bool IsBirthday { get; set; }

        [JsonPropertyName("is_debtor")]
        public bool IsDebtor { get; set; }

        [JsonPropertyName("is_design")]
        public bool IsDesign { get; set; }

        [JsonPropertyName("is_dz_group_issue")]
        public bool IsHomeworkGroupIssue { get; set; }

        [JsonPropertyName("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [JsonPropertyName("is_news_popup")]
        public bool IsNewsPopup { get; set; }

        [JsonPropertyName("is_only_profile")]
        public bool IsOnlyProfile { get; set; }

        [JsonPropertyName("is_phone_verified")]
        public bool IsPhoneVerified { get; set; }

        [JsonPropertyName("is_promo")]
        public bool IsPromo { get; set; }

        [JsonPropertyName("is_quizzes_expired")]
        public bool IsQuizzesExpired { get; set; }

        [JsonPropertyName("is_referral_program")]
        public bool IsReferralProgram { get; set; }

        [JsonPropertyName("is_school")]
        public bool IsSchool { get; set; }

        [JsonPropertyName("is_signal")]
        public bool IsSignal { get; set; }

        [JsonPropertyName("is_tehnotable_news")]
        public bool IsTehnotableNews { get; set; }

        [JsonPropertyName("is_test")]
        public bool IsTest { get; set; }

        [JsonPropertyName("is_vacancy")]
        public bool IsVacancy { get; set; }
    }
}
