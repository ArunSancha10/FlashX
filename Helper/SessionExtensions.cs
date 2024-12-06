using Newtonsoft.Json;

namespace outofoffice.Helper
{
    public static class SessionExtensions
    {
        public static void Sets<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Gets<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
