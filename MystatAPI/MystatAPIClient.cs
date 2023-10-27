using System;
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

namespace MystatAPI
{
    public class MystatAPIClient
    {
        private static string[] archiveTypes = new string[] { ".zip", ".rar", ".7z" };

        const string applicationKey = "6a56a5df2667e65aab73ce76d1dd737f7d1faef9c52e8b8c55ac75f565d8e8a6";
        int? groupId;
		public int? GroupId { get => groupId; set => groupId = value; }

		private string AccessToken { get; set; }
        public string Language { get; private set; }
        public UserLoginData LoginData { get; private set; }

        private static HttpClient sharedClient = new HttpClient()
        {
            BaseAddress = new Uri("https://msapi.itstep.org/api/v2/"),
        };

        public MystatAPIClient(UserLoginData loginData, string language = "ru")
        {
            LoginData = loginData;
            AccessToken = string.Empty;
            Language = language;
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

        private async Task<T> MakeRequest<T>(string url, bool retryOnUnathorized = true)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var response = await sharedClient.SendAsync(requestMessage);
            
            requestMessage.Dispose();

            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if(retryOnUnathorized)
                {
                    await UpdateAccessToken();
                    return await MakeRequest<T>(url, false);
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
            return await MakeRequest<ProfileInfo>("settings/user-info");
        }

        public async Task<DaySchedule[]> GetScheduleByDate(DateTime date)
        {
            return SortSchedule(await MakeRequest<DaySchedule[]>($"schedule/operations/get-by-date?date_filter={Utils.FormatDate(date)}"));
        }

        public async Task<DaySchedule[]> GetMonthSchedule(DateTime date)
        {
            return SortSchedule(await MakeRequest<DaySchedule[]>($"schedule/operations/get-month?date_filter={Utils.FormatDate(date)}"));
        }

        private DaySchedule[] SortSchedule(DaySchedule[] schedule)
        {
            return schedule.OrderBy(s => TimeOnly.Parse(s.StartedAt)).ToArray();
        }

        public async Task<Homework[]> GetHomework(int page = 1, HomeworkStatus status = HomeworkStatus.Active, int? specId = null, HomeworkType type = HomeworkType.Homework)
        {
            if(GroupId is null)
            {
                var profileInfo = await GetProfileInfo();
                GroupId = profileInfo.CurrentGroupId;
            }

            return await MakeRequest<Homework[]>($"homework/operations/list?page={page}&status={(int)status}&type={(int)type}&group_id={groupId}" +
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
                var fileName = fileInfo.Name;
                var fileBytes = File.ReadAllBytes(filePath);
                var fileFormContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
                if(archiveTypes.Contains(fileInfo.Extension))
                {
                    fileFormContent.Headers.Add("Content-Type", "application/octet-stream");
                }
                form.Add(fileFormContent, "file", fileName);
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
            if (archiveTypes.Contains(homeworkFile.Extension))
            {
                fileFormContent.Headers.Add("Content-Type", "application/octet-stream");
            }
            form.Add(fileFormContent, "file", homeworkFile.Name);

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

        public async Task<HomeworkCount[]> GetHomeworkCount(int? specId = null)
        {
            return await MakeRequest<HomeworkCount[]>("count/homework" + (specId == null ? "" : $"?spec_id={specId}"));
        }
		
		public async Task<Spec[]> GetSpecsList()
        {
            return await MakeRequest<Spec[]>("settings/group-specs");
        }

        public async Task<Exam[]> GetAllExams()
        {
            return await MakeRequest<Exam[]>("progress/operations/student-exams");
        }

        public async Task<Exam[]> GetFutureExams()
        {
            return await MakeRequest<Exam[]>("dashboard/info/future-exams");
        }

        public async Task<Activity[]> GetActivities()
        {
            return await MakeRequest<Activity[]>("dashboard/progress/activity");
        }

        public async Task<ActivityLog[]> GetActivitiesLog()
        {
            return await MakeRequest<ActivityLog[]>("dashboard/progress/activity-web");
        }

        public async Task<GroupInfo[]> GetGroupInfo()
        {
            return await MakeRequest<GroupInfo[]>("homework/settings/group-history");
        }

        public async Task<Student[]> GetGroupLeaders()
        {
            return await MakeRequest<Student[]>("dashboard/progress/leader-group");
        }

        public async Task<Student[]> GetStreamLeaders()
        {
            return await MakeRequest<Student[]>("dashboard/progress/leader-stream");
        }

        public async Task<LessonVisit[]> GetVisitHistory()
        {
            return await MakeRequest<LessonVisit[]>("progress/operations/student-visits");
        }

        public async Task<LeadersSummary> GetGroupLeadersSummary()
        {
            return await MakeRequest<LeadersSummary>("dashboard/progress/leader-group-points");
        }

        public async Task<LeadersSummary> GetStreamLeadersSummary()
        {
            return await MakeRequest<LeadersSummary>("dashboard/progress/leader-stream-points");
        }

        public async Task<ProgressMonthInfo[]> GetAttendanceInfo()
        {
            return await MakeRequest<ProgressMonthInfo[]>("dashboard/chart/attendance");
        }

        public async Task<ProgressMonthInfo[]> GetGradesInfo()
        {
            return await MakeRequest<ProgressMonthInfo[]>("dashboard/chart/average-progress");
        }

        public async Task<EvaluateLessonItem[]> GetEvaluateLessonList()
        {
            return await MakeRequest<EvaluateLessonItem[]>("feedback/students/evaluate-lesson-list");
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
    }

    internal static class Utils
    {
        public static string FormatDate(DateTime date) => $"{date.Year}-{date.Month:00}-{date.Day:00}";
    }
}
