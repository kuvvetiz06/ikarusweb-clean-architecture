using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.Tenants.Dtos;

using MediatR;


namespace IKARUSWEB.Application.Features.Tenants.Queries.GetTenantById
{
    public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
    {
        private readonly ITenantRepository _repo;
        private readonly IMapper _mapper;

        public GetTenantByIdQueryHandler(ITenantRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(request.Id, ct);
            if (entity is null) return Result<TenantDto>.Failure("Tenant not found.");
            

            return Result<TenantDto>.Success(_mapper.Map<TenantDto>(entity));
        }
    }
}
