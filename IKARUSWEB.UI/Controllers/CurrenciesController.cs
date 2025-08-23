using IKARUSWEB.UI.Models.Api;
using IKARUSWEB.UI.Models.Currencies;
using IKARUSWEB.UI.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;

namespace IKARUSWEB.UI.Controllers
{
    public class CurrenciesController : Controller
    {
        private readonly IViewLocalizer _l;
        public CurrenciesController( IViewLocalizer l) { _l = l; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, CancellationToken ct)
        {
            return View(new CurrencyViewModel());
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateCurrencyRequest());

        //[HttpPost]
        //public async Task<IActionResult> Create(CreateCurrencyRequest req, CancellationToken ct)
        //{
        //    try
        //    {
        //        var res = await _api.CreateCurrencyAsync(req, ct);
        //        if (!res.Succeeded)
        //        {
        //            ModelState.AddModelError(string.Empty, res.Message);
        //            return View(req);
        //        }
        //        TempData["ok"] = _l["CreatedSuccessfully"];
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (ApiException ex)
        //    {
        //        // Field bazlı hataları ModelState’e bas
        //        if (ex.Errors is { Count: > 0 })
        //            foreach (var e in ex.Errors)
        //                ModelState.AddModelError(NormalizeFieldFor<CreateCurrencyRequest>(e.Field),
        //                    e.Message ?? e.Code ?? (ex.Problem?.Detail ?? ex.Problem?.Title ?? "Invalid"));
        //        else
        //            ModelState.AddModelError(string.Empty, ex.Message);
        //        return View(req);
        //    }
        //}

        [HttpGet]
        public IActionResult EditRate(Guid id, string code, decimal rate)
          => View((id, code, rate));

        //[HttpPost]
        //public async Task<IActionResult> EditRate(Guid id, decimal rate, CancellationToken ct)
        //{
        //    try
        //    {
        //        var res = await _api.UpdateCurrencyRateAsync(id, rate, ct);
        //        if (!res.Succeeded)
        //        {
        //            TempData["err"] = res.Message;
        //        }
        //        else
        //        {
        //            TempData["ok"] = _l["UpdatedSuccessfully"];
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (ApiException ex)
        //    {
        //        TempData["err"] = ex.Message;
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        //private static string NormalizeFieldFor<T>(string? incoming)
        //{
        //    if (string.IsNullOrWhiteSpace(incoming)) return string.Empty;
        //    var last = incoming.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
        //    var p = typeof(T).GetProperties()
        //      .FirstOrDefault(pp => string.Equals(pp.Name, last, StringComparison.OrdinalIgnoreCase));
        //    return p?.Name ?? string.Empty;
        //}
    }
}
