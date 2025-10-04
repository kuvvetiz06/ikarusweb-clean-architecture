using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Abstractions.Localization
{
    public static class MessageCodes
    {
        public static class Common
        {
            public const string RecordCreated = "Common.RecordCreated";
            public const string RecordUpdated = "Common.RecordUpdated";
            public const string RecordDeleted = "Common.RecordDeleted";
            public const string RecordNotFound = "Common.RecordNotFound";


            public const string UnExpectedError = "Common.UnExpectedError";
            public const string Validation = "Common.Validation";
            public const string NotFound = "Common.NotFound";
            public const string BadRequest = "Common.BadRequest";
            public const string Conflict = "Common.Conflict";
            public const string Forbidden = "Common.Forbidden";
            public const string Unauthorized = "Common.Unauthorized";
        }

        public static class RoomBedType
        {
            public const string NameExists = "RoomBedType.Name.Exists";
        }
    }
}
