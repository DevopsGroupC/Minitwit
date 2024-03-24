namespace csharp_minitwit.Utils
{
    public static class MetricsHelpers
    {

        public static string SanitizePath(string path)
        {
            // Normalize and simplify the path
            var normalizedPath = path.Trim('/').ToLower();
            switch (normalizedPath)
            {
                case "public":
                case "register":
                case "login":
                case "logout":
                case "add_message":
                case "follow":
                case "unfollow":

                    return normalizedPath;
                default:
                    return "other"; // Generalize other paths to reduce cardinality
            }
        }
    }
}