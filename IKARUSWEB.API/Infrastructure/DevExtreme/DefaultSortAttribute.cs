using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IKARUSWEB.API.Infrastructure.DevExtreme
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DefaultSortAttribute : Attribute, IAsyncActionFilter
    {
        private readonly (string selector, bool desc)[] _defs;
        public DefaultSortAttribute(params object[] parts)
            => _defs = parts.Chunk(2).Select(p => ((string)p[0], (bool)p[1])).ToArray();

        public Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            if (ctx.ActionArguments.Values.FirstOrDefault(v => v is DataSourceLoadOptions) is DataSourceLoadOptions load)
            {
                if (load.Sort == null || load.Sort.Length == 0)
                    load.Sort = _defs.Select(d => new SortingInfo { Selector = d.selector, Desc = d.desc }).ToArray();

                if (load.PrimaryKey == null || load.PrimaryKey.Length == 0)
                    load.PrimaryKey = new[] { "Id" };
            }
            return next();
        }
    }
}
