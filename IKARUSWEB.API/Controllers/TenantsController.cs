using IKARUSWEB.Application.Common.Interfaces;
using IKARUSWEB.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IKARUSWEB.API.Controllers
{
    [ApiController]
    [Route("api/tenants")]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly IRepository<Tenant> _repo;
        public TenantsController(IRepository<Tenant> repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.ListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Tenant model)
        {
            await _repo.AddAsync(model);
            return CreatedAtAction(nameof(GetAll), new { id = model.Id }, model);
        }
    }
}
