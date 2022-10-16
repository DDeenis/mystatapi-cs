using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using MystatAPI.Entity;
using System.Text;
using MystatAPI.Exceptions;
using System.Net.Http.Headers;

namespace MystatAPI
{
    public class MystatAPIClient
    {
        const string applicationKey = "6a56a5df2667e65aab73ce76d1dd737f7d1faef9c52e8b8c55ac75f565d8e8a6";

        private string AccessToken { get; set; }
        public UserLoginData LoginData { get; }

        private static HttpClient sharedClient = new HttpClient()
        {
            BaseAddress = new Uri("https://msapi.itstep.org/api/v2/"),
        };

        public MystatAPIClient(UserLoginData loginData)
        {
            LoginData = loginData;
            AccessToken = string.Empty;
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

        private async Task<T> MakeRequest<T>(string url, bool retryOnUnaothorized = true)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var response = await sharedClient.SendAsync(requestMessage);
            
            requestMessage.Dispose();

            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if(retryOnUnaothorized)
                {
                    await UpdateAccessToken();
                    return await MakeRequest<T>(url, false);
                }

                var responseError = JsonSerializer.Deserialize<MystatAuthError>(responseJson);
                throw new MystatAuthException(responseError);
            }

            // TODO: check error

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
                return responseObject;
            }
            catch (Exception)
            {
                return JsonSerializer.Deserialize<MystatAuthError[]>(responseJson)[0];
            }

        }

        public async Task<DaySchedule[]> GetScheduleByDate(DateTime date)
        {
            return await MakeRequest<DaySchedule[]>($"schedule/operations/get-by-date?date_filter={Utils.FormatDate(date)}");
        }

        public async Task<DaySchedule[]> GetMonthSchedule(DateTime date)
        {
            return await MakeRequest<DaySchedule[]>($"schedule/operations/get-month?date_filter={Utils.FormatDate(date)}");
        }
    }

    internal static class Utils
    {
        public static string FormatDate(DateTime date) => $"{date.Year}-{date.Month:00}-{date.Day:00}";

    }
}
