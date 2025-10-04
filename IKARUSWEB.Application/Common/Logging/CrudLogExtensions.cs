using Microsoft.Extensions.Logging;

namespace IKARUSWEB.Application.Common.Logging;

public static partial class CrudLogExtensions
{
    // ---- GENERATED (core) ----
    // Erişim belirleyici YOK; extension DEĞİL; partial void
    [LoggerMessage(EventId = AppLog.Created, Level = LogLevel.Information,
        Message = "{Entity} created. Id={Id}")]
    static partial void CreatedCore(ILogger logger, string Entity, Guid Id);

    [LoggerMessage(EventId = AppLog.CreateFailed, Level = LogLevel.Error,
        Message = "{Entity} not created. Reason={Reason}")]
    static partial void CreateFailedCore(ILogger logger, string Entity, string Reason, Exception? exception);

    [LoggerMessage(EventId = AppLog.Retrieved, Level = LogLevel.Information,
        Message = "{Entity} retrieved. Id={Id}")]
    static partial void RetrievedCore(ILogger logger, string Entity, Guid Id);

    [LoggerMessage(EventId = AppLog.RetrievedFailed, Level = LogLevel.Information,
        Message = "{Entity} retrieved. Id={Id}")]
    static partial void RetrievedNotCore(ILogger logger, string Entity, Guid Id);

    [LoggerMessage(EventId = AppLog.Updated, Level = LogLevel.Information,
        Message = "{Entity} updated. Id={Id}")]
    static partial void UpdatedCore(ILogger logger, string Entity, Guid Id);

    [LoggerMessage(EventId = AppLog.UpdateFailed, Level = LogLevel.Error,
        Message = "{Entity} not updated. Reason={Reason}")]
    static partial void UpdateFailedCore(ILogger logger, string Entity, string Reason, Exception? exception);

    [LoggerMessage(EventId = AppLog.Deleted, Level = LogLevel.Information,
        Message = "{Entity} deleted. Id={Id}")]
    static partial void DeletedCore(ILogger logger, string Entity, Guid Id);

    [LoggerMessage(EventId = AppLog.DeleteFailed, Level = LogLevel.Error,
        Message = "{Entity} not deleted. Reason={Reason}")]
    static partial void DeleteFailedCore(ILogger logger, string Entity, string Reason, Exception? exception);

    [LoggerMessage(EventId = AppLog.ValidationFailed, Level = LogLevel.Warning,
        Message = "Validation failed for {Entity}. Errors={Errors}")]
    static partial void ValidationFailedCore(ILogger logger, string Entity, string Errors, Exception? exception);

    [LoggerMessage(EventId = AppLog.BusinessError, Level = LogLevel.Error,
        Message = "Business error for {Entity}. Message={Message}")]
    static partial void BusinessErrorCore(ILogger logger, string Entity, string Message, Exception? exception);

    [LoggerMessage(EventId = AppLog.Unexpected, Level = LogLevel.Critical,
        Message = "Unexpected error for {Entity}")]
    static partial void UnexpectedCore(ILogger logger, string Entity, Exception? exception);

    // ---- PUBLIC WRAPPERS (extension) ----
    public static void Created(this ILogger logger, string entity, Guid id)
        => CreatedCore(logger, entity, id);

    public static void CreateFailed(this ILogger logger, string entity, string reason, Exception? ex = null)
        => CreateFailedCore(logger, entity, reason, ex);

    public static void Retrieved(this ILogger logger, string entity, Guid id)
        => RetrievedCore(logger, entity, id);

    public static void RetrievedFailed(this ILogger logger, string entity, Guid id)
       => RetrievedFailed(logger, entity, id);

    public static void Updated(this ILogger logger, string entity, Guid id)
        => UpdatedCore(logger, entity, id);

    public static void UpdateFailed(this ILogger logger, string entity, string reason, Exception? ex = null)
        => UpdateFailedCore(logger, entity, reason, ex);

    public static void Deleted(this ILogger logger, string entity, Guid id)
        => DeletedCore(logger, entity, id);

    public static void DeleteFailed(this ILogger logger, string entity, string reason, Exception? ex = null)
        => DeleteFailedCore(logger, entity, reason, ex);

    public static void ValidationFailed(this ILogger logger, string entity, string errors, Exception? ex = null)
        => ValidationFailedCore(logger, entity, errors, ex);

    public static void BusinessError(this ILogger logger, string entity, string message, Exception? ex = null)
        => BusinessErrorCore(logger, entity, message, ex);

    public static void Unexpected(this ILogger logger, string entity, Exception? ex = null)
        => UnexpectedCore(logger, entity, ex);
}
