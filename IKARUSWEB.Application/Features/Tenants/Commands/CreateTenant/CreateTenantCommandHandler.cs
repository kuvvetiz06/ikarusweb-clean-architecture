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

namespace IKARUSWEB.Application.Features.Tenants.Commands.CreateTenant
{
    public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateTenantCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken ct)
        {
            var entity = new Tenant(
                request.Name,
                request.Country,
                request.City,
                request.Street,
                request.DefaultCurrency,
                request.TimeZone,
                request.DefaultCulture
            );

            await _db.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);

            var dto = _mapper.Map<TenantDto>(entity);
            return Result<TenantDto>.Success(dto, "Tenant created.");
        }
    }
}
