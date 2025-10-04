// IKARUSWEB.API/Filters/ResultLocalizationFilter.cs
using IKARUSWEB.API.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using System.Globalization;

public sealed class ResultLocalizationFilter : IAsyncResultFilter
{
    private readonly IStringLocalizer<CommonResource> _LCommon;
    private readonly IStringLocalizer<ValidationResource> _LVal;

    public ResultLocalizationFilter(
        IStringLocalizer<CommonResource> lCommon,
        IStringLocalizer<ValidationResource> lVal)
    {
        _LCommon = lCommon;
        _LVal = lVal;
    }

    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult obj && obj.Value is not null)
        {
            var vt = obj.Value.GetType();
            if (vt.IsGenericType && vt.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Message (genel başlık) — tek key olabilir
                var msgProp = vt.GetProperty(nameof(Result<object>.Message));
                if (msgProp?.GetValue(obj.Value) is string msgKey && !string.IsNullOrWhiteSpace(msgKey))
                {
                    // "Common.NotFound||Oda Türü" gibi geldiyse args'ı da parse et
                    var (key, args) = SplitKeyArgs(msgKey);
                    var loc = _LCommon[key];
                    msgProp.SetValue(obj.Value, Format(loc, args));
                }

                // Errors — field -> [ "Validation.MaxLength||32", ... ]
                var errProp = vt.GetProperty(nameof(Result<object>.Errors));
                if (errProp?.GetValue(obj.Value) is Dictionary<string, string[]> errs && errs.Count > 0)
                {
                    var localized = new Dictionary<string, string[]>(errs.Count);
                    foreach (var (field, msgs) in errs)
                    {
                        var arr = new List<string>(msgs?.Length ?? 0);
                        foreach (var raw in msgs ?? Array.Empty<string>())
                        {
                            var (key, args) = SplitKeyArgs(raw);
                            var loc = _LVal[key];
                            arr.Add(Format(loc, args) ?? key); // bulunamazsa key'i geri döndür
                        }
                        localized[field] = arr.ToArray();
                    }
                    errProp.SetValue(obj.Value, localized);
                }
            }
        }
        return next();
    }

    private static (string key, object[] args) SplitKeyArgs(string raw)
    {
        // "Key||42||abc" → ("Key", ["42","abc"] -> uygun tipe çevrilir)
        var parts = raw.Split(new[] { "||" }, StringSplitOptions.None);
        if (parts.Length == 1) return (parts[0], Array.Empty<object>());

        var args = parts.Skip(1).Select(Coerce).ToArray();
        return (parts[0], args);

        static object Coerce(string s)
        {
            // int/long/decimal/bool/date parse etmeye çalış; olmazsa string bırak
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
            if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var l)) return l;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (bool.TryParse(s, out var b)) return b;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
            return s;
        }
    }

    private static string Format(LocalizedString loc, object[] args)
    {
        // Resource'ta parametreli şablon ise: "Maksimum karakter sayısı {0}'dir."
        return (args is { Length: > 0 })
            ? string.Format(CultureInfo.CurrentCulture, loc?.Value ?? string.Empty, args)
            : (loc?.Value ?? string.Empty);
    }
}
