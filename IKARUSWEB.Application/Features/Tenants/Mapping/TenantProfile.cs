using AutoMapper;
using IKARUSWEB.Application.Features.Tenants.Dtos;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Tenants.Mapping
{
    public sealed class TenantProfile : Profile
    {
        public TenantProfile()
        {
            CreateMap<Tenant, TenantDto>()
             .ForMember(d => d.DefaultCurrencyCode,
                 opt => opt.MapFrom(s => s.DefaultCurrencyCode));
        }
    }

   
}
