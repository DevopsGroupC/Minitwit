namespace csharp_minitwit.Utils
{
    public static class SessionExtensions
    {
        public static void SetInt64(this ISession session, string key, long value)
        {
            var byteArray = BitConverter.GetBytes(value);
            session.Set(key, byteArray);
        }

        public static long? GetInt64(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null || data.Length != sizeof(long))
            {
                return null;
            }

            return BitConverter.ToInt64(data, 0);
        }
    }

}
