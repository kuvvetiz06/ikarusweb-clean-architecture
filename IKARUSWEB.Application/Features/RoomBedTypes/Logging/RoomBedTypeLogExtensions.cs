using Microsoft.Extensions.Logging;
using IKARUSWEB.Application.Common.Logging;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Logging;

public static class RoomBedTypeLogExtensions
{
    private const string Entity = "RoomBedType";

    public static void RoomBedTypeAdded(this ILogger logger, Guid id)
        => logger.Created(Entity, id);

    public static void RoomBedTypeNotAdded(this ILogger logger, string reason, Exception? ex = null)
        => logger.CreateFailed(Entity, reason, ex);

    public static void RoomBedTypeRetrieved(this ILogger logger, Guid id)
        => logger.Retrieved(Entity, id);

    public static void RoomBedTypeNotRetrieved(this ILogger logger, Guid id)
        => logger.RetrievedFailed(Entity, id);
    public static void RoomBedTypeUpdated(this ILogger logger, Guid id)
        => logger.Updated(Entity, id);

    public static void RoomBedTypeNotUpdated(this ILogger logger, string reason, Exception? ex = null)
        => logger.UpdateFailed(Entity, reason, ex);

    public static void RoomBedTypeDeleted(this ILogger logger, Guid id)
        => logger.Deleted(Entity, id);

    public static void RoomBedTypeNotDeleted(this ILogger logger, string reason, Exception? ex = null)
        => logger.DeleteFailed(Entity, reason, ex);
}
