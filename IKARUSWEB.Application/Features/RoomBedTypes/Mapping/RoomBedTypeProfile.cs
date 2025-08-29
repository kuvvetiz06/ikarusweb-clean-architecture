using AutoMapper;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using IKARUSWEB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Mapping
{
    public sealed class RoomBedTypeProfile : Profile
    {
        public RoomBedTypeProfile()
        {
            CreateMap<RoomBedType, RoomBedTypeDto>();
        }
    }
}
