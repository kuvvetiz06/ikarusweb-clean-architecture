using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Services
{
    public interface ITempDataNotifier
    {
        void Success(Controller controller, string message);
        void Error(Controller controller, string message);
        void ErrorList(Controller controller, IEnumerable<string> messages);
    }

    public sealed class TempDataNotifier : ITempDataNotifier
    {
        private const string KEY_SUCCESS = "notify_success";
        private const string KEY_ERROR = "notify_error";

        public void Success(Controller c, string m) => c.TempData[KEY_SUCCESS] = m;
        public void Error(Controller c, string m) => c.TempData[KEY_ERROR] = m;
        public void ErrorList(Controller c, IEnumerable<string> ms)
            => c.TempData[KEY_ERROR] = string.Join("<br/>", ms);
    }
}
