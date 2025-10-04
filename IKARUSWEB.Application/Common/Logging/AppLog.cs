using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Common.Logging
{
    public static class AppLog
    {
        // 1000-1999: CRUD
        public const int Created = 1001;
        public const int CreateFailed = 1002;
        public const int Retrieved = 1003;
        public const int RetrievedFailed = 1004;
        public const int Updated = 1005;
        public const int UpdateFailed = 1006;
        public const int Deleted = 1007;
        public const int DeleteFailed = 1008;

        // 2000-2099: Validation/Business
        public const int ValidationFailed = 2001;
        public const int BusinessError = 2002;

        // 3000-3099: Unexpected/System
        public const int Unexpected = 3001;
    }
}
