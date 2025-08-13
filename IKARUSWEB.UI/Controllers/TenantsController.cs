using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Services;
using IKARUSWEB.UI.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.UI.Controllers
{
  
    public sealed class TenantsController : Controller
    {
        private readonly IApiClient _api;
        private readonly ITempDataNotifier _notify;

        public TenantsController(IApiClient api, ITempDataNotifier notify)
        {
            _api = api; _notify = notify;
        }

        [HttpGet]
        public IActionResult Create()
        {
            // varsayılan değerlerle açmak istersen:
            var model = new CreateTenantDto(
                Name: "",
                Country: "Türkiye",
                City: "Ankara",
                Street: "Kızılay",
                DefaultCurrency: "TRY",
                TimeZone: "Europe/Istanbul",
                DefaultCulture: "tr-TR"
            );
            return View(model);
        }

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

                _notify.Success(this, "Tenant oluşturuldu.");
                return RedirectToAction("Detail", new { id = t.Id });
            }
            catch (ApiException ex)
            {
                // alan bazlı hataları input altına bas
                if (ex.Errors is { Count: > 0 })
                {
                    foreach (var e in ex.Errors)
                    {
                        // API tarafındaki PropertyName ile UI model alan adın aynı: Name, Country, City...
                        ModelState.AddModelError(e.Field ?? string.Empty, e.Message ?? "Geçersiz değer.");
                    }
                }
                else
                {
                    _notify.Error(this, ex.Message ?? "İşlem başarısız.");
                }
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var t = await _api.GetTenantAsync(id);
            if (t is null) return NotFound();
            return View(t);
        }
    }
}
