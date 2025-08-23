using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Contracts.Currencies;
using IKARUSWEB.Domain.Entities;
using MediatR;


namespace IKARUSWEB.Application.Features.Currencies.Commands.CreateCurrency
{
    public sealed class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
    {
        private readonly ICurrencyRepository _repo;
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateCurrencyCommandHandler(ICurrencyRepository repo, IAppDbContext db, IMapper mapper)
        {
            _repo = repo; _db = db; _mapper = mapper;
        }

        public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken ct)
        {
            var entity = new Currency(request.Name, request.Code, request.CurrencyMultiplier, request.Rate);
            await _repo.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct); // UoW değil; EF context save

            return Result<CurrencyDto>.Success(_mapper.Map<CurrencyDto>(entity));
        }
    }
}
