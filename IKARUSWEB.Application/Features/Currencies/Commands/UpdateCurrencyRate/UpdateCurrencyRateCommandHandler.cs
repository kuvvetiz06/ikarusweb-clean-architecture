using AutoMapper;
using IKARUSWEB.Application.Abstractions;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Mapping;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.Currencies.Commands.UpdateCurrencyRate
{
    public sealed class UpdateCurrencyRateCommandHandler : IRequestHandler<UpdateCurrencyRateCommand, Result<CurrencyDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICurrencyRepository _repo;

        public UpdateCurrencyRateCommandHandler(ICurrencyRepository repo, IAppDbContext db, IMapper mapper)
        {
            _repo = repo; _db = db; _mapper = mapper;
        }

        public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyRateCommand request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(request.Id, ct);
            if (entity is null)
                return Result<CurrencyDto>.Failure("Currency not found.");

            entity.SetRate(request.Rate);
            await _db.SaveChangesAsync(ct);

            var dto = _mapper.Map<CurrencyDto>(entity);
            return Result<CurrencyDto>.Success(dto);
        }
    }
}
