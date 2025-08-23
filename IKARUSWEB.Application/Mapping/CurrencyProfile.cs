using AutoMapper;
using IKARUSWEB.Application.Contracts.Currencies;
using IKARUSWEB.Domain.Entities;


namespace IKARUSWEB.Application.Mapping
{
    public sealed class CurrencyProfile : Profile
    {
        public CurrencyProfile()
        {
            CreateMap<Currency, CurrencyDto>();
        }
    }
}
