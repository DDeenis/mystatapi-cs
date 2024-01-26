﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using MystatAPI.Entity;
using System.Text;
using MystatAPI.Exceptions;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace MystatAPI
{
    public class MystatAPIClient
    {
        private static string[] archiveTypes = new string[] { ".zip", ".rar", ".7z" };
        private static string[] imageTypes = new string[] { ".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".gif", ".webp", ".avif", ".svg", ".ico" };

        const string applicationKey = "6a56a5df2667e65aab73ce76d1dd737f7d1faef9c52e8b8c55ac75f565d8e8a6";
        int? groupId;
		public int? GroupId { get => groupId; set => groupId = value; }

		private string AccessToken { get; set; }
        public string Language { get; private set; }
        public string ContentEncoding { get; set; }
        public bool BypassUploadRestrictions { get; set; } = false;
        public UserLoginData LoginData { get; private set; }

        private static HttpClient sharedClient = new HttpClient()
        {
            BaseAddress = new Uri("https://msapi.itstep.org/api/v2/"),
        };

        public MystatAPIClient(UserLoginData loginData, string language = "ru", string contentEncoding = "gzip, br")
        {
            LoginData = loginData;
            AccessToken = string.Empty;
            Language = language;
            ContentEncoding = contentEncoding;
            sharedClient.DefaultRequestHeaders.Add("x-language", Language);
        }

        public MystatAPIClient() : this(new())
        {
        }

        public void SetLoginData(UserLoginData loginData)
        {
            LoginData = loginData;
        }

        public void SetLanguage(string language)
        {
            Language = language;
            sharedClient.DefaultRequestHeaders.Remove("x-language");
            sharedClient.DefaultRequestHeaders.Add("x-language", Language);
        }

        private async Task UpdateAccessToken()
        {
            var response = await Login();
            MystatAuthSuccess? responseSuccess = response as MystatAuthSuccess;

            if(responseSuccess is null)
            {
                var responseError = response as MystatAuthError;
                throw new MystatAuthException(responseError);
            }

            string token = responseSuccess.AccessToken;
            AccessToken = token;
        }

        private async Task<T> GetRequest<T>(string url, bool retryOnUnathorized = true)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add("Accept-Encoding", ContentEncoding);
            //requestMessage.Version = new Version(2, 0);

            var response = await sharedClient.SendAsync(requestMessage);
            
            requestMessage.Dispose();

            var responseJson = string.Empty;

            try
            {
                string? encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                using (var memoryStream = new MemoryStream(bytes))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        if (encoding == "br")
                        {
                            using (var decompressStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
                            {
                                decompressStream.CopyTo(outputStream);
                            }
                        }
                        else if(encoding == "gzip" || encoding == "gz")
                        {
                            using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                            {
                                decompressStream.CopyTo(outputStream);
                            }
                        }
                        else
                        {
                            throw new Exception($"Unsupported Content-Encoding: {encoding}");
                        }

                        responseJson = Encoding.UTF8.GetString(outputStream.ToArray());
                    }
                }
            }
            catch { }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if(retryOnUnathorized)
                {
                    await UpdateAccessToken();
                    return await GetRequest<T>(url, false);
                }

                var responseError = JsonSerializer.Deserialize<MystatAuthError>(responseJson);
                throw new MystatAuthException(responseError);
            }

            var responseObject = JsonSerializer.Deserialize<T>(responseJson);

            return responseObject;
        }

        private async Task<T> PostRequest<T>(string url, MultipartFormDataContent form, bool retryOnUnatorized = true)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Content = form;

            var response = await sharedClient.SendAsync(requestMessage);

            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if(retryOnUnatorized)
                {
                    await UpdateAccessToken();
                    return await PostRequest<T>(url, form);
                }

                var responseError = JsonSerializer.Deserialize<MystatAuthError>(await response.Content.ReadAsStringAsync());
                throw new MystatAuthException(responseError);
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new MystatException();
            }

            requestMessage.Dispose();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<T>(responseJson);
            return responseObject;
        }

        public async Task<MystatAuthResponse> Login()
        {
            var jsonObject = new
            {
                application_key = applicationKey,
                username = LoginData.Username,
                password = LoginData.Password,
            };
            var content = new StringContent(JsonSerializer.Serialize(jsonObject), Encoding.UTF8, "application/json");
            var response = await sharedClient.PostAsync("auth/login", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            try
            {
                GroupId = null;
                var responseObject = JsonSerializer.Deserialize<MystatAuthSuccess>(responseJson);
                AccessToken = responseObject.AccessToken;
                return responseObject;
            }
            catch (Exception)
            {
                return JsonSerializer.Deserialize<MystatAuthError[]>(responseJson)[0];
            }
        }

        public async Task<ProfileInfo> GetProfileInfo()
        {
            return await GetRequest<ProfileInfo>("settings/user-info");
        }

        public async Task<DaySchedule[]> GetScheduleByDate(DateTime date)
        {
            return SortSchedule(await GetRequest<DaySchedule[]>($"schedule/operations/get-by-date?date_filter={Utils.FormatDate(date)}"));
        }

        public async Task<DaySchedule[]> GetMonthSchedule(DateTime date)
        {
            return SortSchedule(await GetRequest<DaySchedule[]>($"schedule/operations/get-month?date_filter={Utils.FormatDate(date)}"));
        }

        private DaySchedule[] SortSchedule(DaySchedule[] schedule)
        {
            return schedule.OrderBy(s => TimeOnly.Parse(s.StartedAt)).ToArray();
        }

        public async Task<HomeworkDTOWithStatus[]> GetHomeworkByType(int page = 1, int? specId = null, HomeworkType type = HomeworkType.Homework)
        {
            if(GroupId is null)
            {
                var profileInfo = await GetProfileInfo();
                GroupId = profileInfo.CurrentGroupId;
            }

            return await GetRequest<HomeworkDTOWithStatus[]>($"homework/operations/list?page={page}&type={(int)type}&group_id={groupId}" +
                                                 (specId == null ? "" : $"&spec_id={specId}"));
        }
        
        public async Task<HomeworkDTO> GetHomework(int page = 1, HomeworkStatus status = HomeworkStatus.Active, int? specId = null, HomeworkType type = HomeworkType.Homework)
        {
            if(GroupId is null)
            {
                var profileInfo = await GetProfileInfo();
                GroupId = profileInfo.CurrentGroupId;
            }

            return await GetRequest<HomeworkDTO>($"homework/operations/list?page={page}&status={(int)status}&type={(int)type}&group_id={groupId}" +
                                                 (specId == null ? "" : $"&spec_id={specId}"));
        }

        public async Task<UploadedHomeworkInfo> UploadHomework(int homeworkId, string? filePath, string? answerText = null, int spentTimeHour = 99, int spentTimeMin = 59)
        {
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new StringContent(homeworkId.ToString()), "id" },
                { new StringContent(spentTimeHour.ToString()), "spentTimeHour" },
                { new StringContent(spentTimeMin.ToString()), "spentTimeMin" }
            };

            if (answerText is not null)
            {
                form.Add(new StringContent(answerText), "answerText");
            }

            if(filePath is not null)
            {
                var fileInfo = new FileInfo(filePath);
                var fileBytes = File.ReadAllBytes(filePath);
                var fileFormContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
                form.Add(AttachFileContentType(fileFormContent, fileInfo.Extension), "file", fileInfo.Name);
            }

            return await PostRequest<UploadedHomeworkInfo>("homework/operations/create", form);
        }
        
        public async Task<UploadedHomeworkInfo> UploadHomeworkFile(int homeworkId, HomeworkFile homeworkFile, string? answerText = null, int spentTimeHour = 99, int spentTimeMin = 59)
        {
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new StringContent(homeworkId.ToString()), "id" },
                { new StringContent(spentTimeHour.ToString()), "spentTimeHour" },
                { new StringContent(spentTimeMin.ToString()), "spentTimeMin" },
            };

            var fileFormContent = new ByteArrayContent(homeworkFile.Bytes, 0, homeworkFile.Bytes.Length);
            form.Add(AttachFileContentType(fileFormContent, homeworkFile.Extension), "file", homeworkFile.Name);

            if (answerText is not null)
            {
                form.Add(new StringContent(answerText), "answerText");
            }

            return await PostRequest<UploadedHomeworkInfo>("homework/operations/create", form);
        }

        public async Task<bool> RemoveHomework(int homeworkId)
        {
            var body = new
            {
                id = homeworkId
            };

            const string url = "homework/operations/delete";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await sharedClient.SendAsync(requestMessage);

            requestMessage.Dispose();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await UpdateAccessToken();
                return await RemoveHomework(homeworkId);
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<HomeworkCount[]> GetHomeworkCount(int? specId = null, HomeworkType type = HomeworkType.Homework)
        {
            var args = $"?type={(int)type}" + (specId == null ? "" : $"&spec_id={specId}");
            return await GetRequest<HomeworkCount[]>($"count/homework{args}");
        }
		
		public async Task<Spec[]> GetSpecsList()
        {
            return await GetRequest<Spec[]>("settings/group-specs");
        }

        public async Task<Exam[]> GetAllExams()
        {
            return await GetRequest<Exam[]>("progress/operations/student-exams");
        }

        public async Task<Exam[]> GetFutureExams()
        {
            return await GetRequest<Exam[]>("dashboard/info/future-exams");
        }

        public async Task<Activity[]> GetActivities()
        {
            return await GetRequest<Activity[]>("dashboard/progress/activity");
        }

        public async Task<ActivityLog[]> GetActivitiesLog()
        {
            return await GetRequest<ActivityLog[]>("dashboard/progress/activity-web");
        }

        public async Task<GroupInfo[]> GetGroupInfo()
        {
            return await GetRequest<GroupInfo[]>("homework/settings/group-history");
        }

        public async Task<Student[]> GetGroupLeaders()
        {
            return await GetRequest<Student[]>("dashboard/progress/leader-group");
        }

        public async Task<Student[]> GetStreamLeaders()
        {
            return await GetRequest<Student[]>("dashboard/progress/leader-stream");
        }

        public async Task<LessonVisit[]> GetVisitHistory()
        {
            return await GetRequest<LessonVisit[]>("progress/operations/student-visits");
        }

        public async Task<LeadersSummary> GetGroupLeadersSummary()
        {
            return await GetRequest<LeadersSummary>("dashboard/progress/leader-group-points");
        }

        public async Task<LeadersSummary> GetStreamLeadersSummary()
        {
            return await GetRequest<LeadersSummary>("dashboard/progress/leader-stream-points");
        }

        public async Task<ProgressMonthInfo[]> GetAttendanceInfo()
        {
            return await GetRequest<ProgressMonthInfo[]>("dashboard/chart/attendance");
        }

        public async Task<ProgressMonthInfo[]> GetGradesInfo()
        {
            return await GetRequest<ProgressMonthInfo[]>("dashboard/chart/average-progress");
        }

        public async Task<EvaluateLessonItem[]> GetEvaluateLessonList()
        {
            return await GetRequest<EvaluateLessonItem[]>("feedback/students/evaluate-lesson-list");
        }
        
        public async Task<bool> EvaluateLesson(string key, int lessonMark, int teacherMark, string? comment = null)
        {
            var body = new
            {
                key,
                mark_lesson = lessonMark,
                mark_teach = teacherMark,
                comment
            };

            const string url = "feedback/students/evaluate-lesson";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await sharedClient.SendAsync(requestMessage);

            requestMessage.Dispose();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await UpdateAccessToken();
                return await EvaluateLesson(key, lessonMark, teacherMark);
            }

            return response.StatusCode == HttpStatusCode.ResetContent;
        }

        public async Task<bool> ChangeCurrentGroup(int groupId)
        {
            var body = new
            {
                id_tgroups = groupId
            };

            const string url = "settings/change-current-group";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await sharedClient.SendAsync(requestMessage);

            requestMessage.Dispose();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await UpdateAccessToken();
                return await ChangeCurrentGroup(groupId);
            }

            bool isSuccess = response.StatusCode == HttpStatusCode.NoContent;

            if(isSuccess)
            {
                GroupId = groupId;
            }

            return isSuccess;
        }

        // internal functions
        internal HttpContent AttachFileContentType(HttpContent content, string fileExtension)
        {
            if(BypassUploadRestrictions)
            {
                content.Headers.Add("Content-Type", "application/octet-stream");
                return content;
            }

            if (archiveTypes.Contains(fileExtension))
            {
                content.Headers.Add("Content-Type", "application/octet-stream");
            }
            else if(imageTypes.Contains(fileExtension))
            {
                // or 'image/{matchedImageExtension}'
                content.Headers.Add("Content-Type", "image/generic");
            }

            return content;
        }
    }

    internal static class Utils
    {
        public static string FormatDate(DateTime date) => $"{date.Year}-{date.Month:00}-{date.Day:00}";
    }
}
