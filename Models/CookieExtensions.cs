using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SureAdmitCore.Models
{
 

    public static class CookieExtensions
    {
        public static void SetObject(this IResponseCookies cookies, string key, object value, int? expireDays = null)
        {
            var json = JsonSerializer.Serialize(value);
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Expires = expireDays.HasValue ? DateTimeOffset.Now.AddDays(expireDays.Value) : null
            };
            cookies.Append(key, json, options);
        }

        public static T? GetObject<T>(this IRequestCookieCollection cookies, string key)
        {
            if (!cookies.ContainsKey(key)) return default;
            var json = cookies[key];
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
