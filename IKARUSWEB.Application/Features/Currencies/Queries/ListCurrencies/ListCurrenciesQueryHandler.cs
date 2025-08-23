using AutoMapper;
using AutoMapper.QueryableExtensions;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Contracts.Currencies;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IKARUSWEB.Application.Features.Currencies.Queries.ListCurrencies
{
    public sealed class ListCurrenciesQueryHandler : IRequestHandler<ListCurrenciesQuery, IReadOnlyList<CurrencyDto>>
    {
        private readonly ICurrencyRepository _repo;
        private readonly IMapper _mapper;

        public ListCurrenciesQueryHandler(ICurrencyRepository repo, IMapper mapper)
        {
           _repo = repo; _mapper = mapper;
        }

        public async Task<IReadOnlyList<CurrencyDto>> Handle(ListCurrenciesQuery request, CancellationToken ct)
        {
            var q = _repo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var term = request.Q.Trim();
                q = q.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }

            return await q
                .OrderBy(x => x.Code)
                .ProjectTo<CurrencyDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }
    }
}
