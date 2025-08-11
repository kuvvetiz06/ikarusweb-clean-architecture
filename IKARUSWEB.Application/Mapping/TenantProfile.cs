using AutoMapper;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Mapping
{
    public sealed class TenantProfile : Profile
    {
        public TenantProfile()
        {
            CreateMap<Tenant, TenantDto>();
        }
    }

    public sealed record TenantDto(
        Guid Id,
        string Name,
        string Street,
        string City,
        string Country,
        string DefaultCurrency,
        string TimeZone,
        string DefaultCulture
    );
}
