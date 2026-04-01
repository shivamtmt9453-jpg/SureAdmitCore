using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;  

namespace SureAdmitCore.Areas.Admin.Models
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (json == null)
                return default;
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}