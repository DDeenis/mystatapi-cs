using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MystatAPI.Entity
{
    // dummy class to return MystatAuthResponseSuccess or MystatAuthError
    public abstract class MystatAuthResponse { }

    public class MystatAuthSuccess : MystatAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in_access")]
        public int ExpiresInAccess { get; set; }

        [JsonPropertyName("expires_in_refresh")]
        public int ExpiresInRefresh { get; set; }

        [JsonPropertyName("user_type")]
        public int UserType { get; set; }

        [JsonPropertyName("city_data")]
        public CityData CityData { get; set; }
    }

    public class MystatAuthError : MystatAuthResponse
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
