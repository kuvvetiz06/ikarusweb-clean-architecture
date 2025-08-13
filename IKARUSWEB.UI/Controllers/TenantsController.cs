using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Services;
using IKARUSWEB.UI.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Controllers
{
    public sealed class TenantsController : Controller
    {
        private readonly IApiClient _api;
        private readonly ITempDataNotifier _notify;

        public TenantsController(IApiClient api, ITempDataNotifier notify)
        { _api = api; _notify = notify; }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTenantDto dto)
        {
            try
            {
                var t = await _api.CreateTenantAsync(dto);
                if (t is null)
                {
                    _notify.Error(this, "Beklenmeyen bir hata oluştu.");
                    return View(dto);
                }
                _notify.Success(this, "Tenant oluşturuldu."); // UI metnini .resx’e alabilirsin
                return RedirectToAction("Detail", new { id = t.Id });
            }
            catch (ApiException ex)
            {
                // Field-level mesajları topla (API zaten TR/EN döndürüyor)
                var list = ex.Errors?.Select(e => $"{e.Field}: {e.Message}") ?? Array.Empty<string>();
                if (list.Any()) _notify.ErrorList(this, list);
                else _notify.Error(this, ex.Message ?? "İşlem başarısız.");

                return View(dto);
            }
        }
    }
}
