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

namespace MystatAPI
{
    public class MystatAPIClient
    {
        const string applicationKey = "6a56a5df2667e65aab73ce76d1dd737f7d1faef9c52e8b8c55ac75f565d8e8a6";
        int? groupId;

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
            return await MakeRequest<DaySchedule[]>($"schedule/operations/get-by-date?date_filter={Utils.FormatDate(date)}");
        }

        public async Task<DaySchedule[]> GetMonthSchedule(DateTime date)
        {
            return await MakeRequest<DaySchedule[]>($"schedule/operations/get-month?date_filter={Utils.FormatDate(date)}");
        }

        public async Task<Homework[]> GetHomework(int page = 1, HomeworkStatus status = HomeworkStatus.Active, HomeworkType type = HomeworkType.Homework)
        {
            if(groupId is null)
            {
                var profileInfo = await GetProfileInfo();
                groupId = profileInfo.CurrentGroupId;
            }

            return await MakeRequest<Homework[]>($"homework/operations/list?page={page}&status={(int)status}&type={(int)type}&group_id={groupId}");
        }

        public async Task<UploadedHomeworkInfo> UploadHomework(int homeworkId, string? filePath, string? answerText = null, int spentTimeHour = 99, int spentTimeMin = 59)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(homeworkId.ToString()), "id");
            form.Add(new StringContent(spentTimeHour.ToString()), "spentTimeHour");
            form.Add(new StringContent(spentTimeMin.ToString()), "spentTimeMin");

            if(answerText is not null)
            {
                form.Add(new StringContent(answerText), "answerText");
            }

            if(filePath is not null)
            {
                var fileName = new FileInfo(filePath).Name;
                var fileBytes = File.ReadAllBytes(filePath);
                form.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "file", fileName);
            }

            return await PostRequest<UploadedHomeworkInfo>("homework/operations/create", form);
        }
        
        public async Task<UploadedHomeworkInfo> UploadHomeworkFile(int homeworkId, HomeworkFile homeworkFile, string? answerText = null, int spentTimeHour = 99, int spentTimeMin = 59)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(homeworkId.ToString()), "id");
            form.Add(new StringContent(spentTimeHour.ToString()), "spentTimeHour");
            form.Add(new StringContent(spentTimeMin.ToString()), "spentTimeMin");
            form.Add(new ByteArrayContent(homeworkFile.Bytes, 0, homeworkFile.Bytes.Length), "file", homeworkFile.Name);

            if(answerText is not null)
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

        public async Task<HomeworkCount[]> GetHomeworkCount()
        {
            return await MakeRequest<HomeworkCount[]>("count/homework");
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
    }

    public class HomeworkFile
    {
        public string Name { get; }
        public byte[] Bytes { get; }

        public HomeworkFile(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }
    }

    internal static class Utils
    {
        public static string FormatDate(DateTime date) => $"{date.Year}-{date.Month:00}-{date.Day:00}";
    }
}
