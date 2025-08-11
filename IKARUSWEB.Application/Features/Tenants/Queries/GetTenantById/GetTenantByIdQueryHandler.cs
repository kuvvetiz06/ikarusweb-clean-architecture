using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Mapping;
using IKARUSWEB.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Queries.GetTenantById
{
    public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetTenantByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken ct)
        {
            var entity = await _db.FindAsync<Tenant>(request.Id, ct);
            if (entity is null) return Result<TenantDto>.Failure("Tenant not found.");

            var dto = _mapper.Map<TenantDto>(entity);
            return Result<TenantDto>.Success(dto);
        }
    }
}
