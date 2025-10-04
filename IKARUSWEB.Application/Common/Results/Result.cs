// Application/Common/Results/Result.cs  (namespace'ini senin projene göre kullan)
using IKARUSWEB.Application.Abstractions.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

public class Result<T>
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    // Validation / bilinen hatalar için (null ise JSON'a yazılmaz)
    public Dictionary<string, string[]>? Errors { get; init; }

    // HTTP status (null ise success->200, fail->400 varsayılır)
    public int? Status { get; init; }

    // ---- Fabrikalar ----
    public static Result<T> Success(T data, string? msg = null, int status = StatusCodes.Status200OK)
        => new() { Succeeded = true, Data = data, Message = msg, Status = status };

    public static Result<T> Created(T data, string? msg = null)
        => Success(data, msg, StatusCodes.Status201Created);

    public static Result<T> Failure(string msg, int status = StatusCodes.Status400BadRequest)
        => new() { Succeeded = false, Message = msg, Status = status };

    public static Result<T> Failure(IDictionary<string, string[]> errors, string? msg = null, int status = StatusCodes.Status422UnprocessableEntity)
        => new()
        {
            Succeeded = false,
            Message = msg,
            Errors = errors?.ToDictionary(kv => kv.Key, kv => kv.Value ?? System.Array.Empty<string>()),
            Status = status
        };

    public static Result<T> Validation(IDictionary<string, string[]> errors, string? msg = MessageCodes.Common.Validation)
        => Failure(errors, msg, StatusCodes.Status422UnprocessableEntity);

    public static Result<T> NotFound(string msg = MessageCodes.Common.NotFound)
        => Failure(msg, StatusCodes.Status404NotFound);

    public static Result<T> Conflict(string msg = MessageCodes.Common.Conflict)
        => Failure(msg, StatusCodes.Status409Conflict);

    public static Result<T> Forbidden(string msg = MessageCodes.Common.Forbidden)
        => Failure(msg, StatusCodes.Status403Forbidden);

    public static Result<T> Unauthorized(string msg = MessageCodes.Common.Unauthorized)
        => Failure(msg, StatusCodes.Status401Unauthorized);

    // ---- En kritik kısım: implicit dönüşüm ----
    // Controller return type'ı ActionResult veya ActionResult<Result<T>> olsun; 'return result;' yeter.
    public static implicit operator ActionResult(Result<T> result)
        => new ObjectResult(result)
        {
            StatusCode = result.Status ?? (result.Succeeded ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest)
        };

    public static implicit operator ActionResult<Result<T>>(Result<T> result)
        => new ObjectResult(result)
        {
            StatusCode = result.Status ?? (result.Succeeded ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest)
        };
}
