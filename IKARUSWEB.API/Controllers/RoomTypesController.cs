using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IKARUSWEB.Application.Common.Interfaces;
using IKARUSWEB.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/roomtypes")]
    [Authorize]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRepository<RoomType> _repo;
        public RoomTypesController(IRepository<RoomType> repo) => _repo = repo;

        [HttpGet]
        public async Task<LoadResult> Get(DataSourceLoadOptions loadOptions)
            => await (_repo as dynamic).LoadDataAsync(loadOptions);
    }
}
