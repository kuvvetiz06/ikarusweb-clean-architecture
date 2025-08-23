namespace IKARUSWEB.UI.Services.Api
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessOrThrowAsync(this HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
                throw await ApiException.FromResponseAsync(resp);
        }
    }
}
