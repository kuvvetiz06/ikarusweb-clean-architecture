namespace IKARUSWEB.UI.Helper
{
    public static class GetJwtExpires
    {
        public static DateTimeOffset? Get(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return null;
                string Pad(string s) => s.Replace('-', '+').Replace('_', '/')
                                         .PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
                var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("exp", out var expEl) && expEl.TryGetInt64(out var sec))
                    return DateTimeOffset.FromUnixTimeSeconds(sec);
            }
            catch { }
            return null;
        }
    }
}
