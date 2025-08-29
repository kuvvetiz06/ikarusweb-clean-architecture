using AutoMapper;
using IKARUSWEB.Application.Abstractions.Repositories;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedTypeName
{
    public sealed class UpdateRoomBedTypeNameCommandHandler
      : IRequestHandler<UpdateRoomBedTypeNameCommand, Result<RoomBedTypeDto>>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IRoomBedTypeWriteRepository _write;

        private readonly IMapper _mapper;

        public UpdateRoomBedTypeNameCommandHandler(
            IRoomBedTypeReadRepository read, 
            IRoomBedTypeWriteRepository write, IMapper mapper)
        { _read = read; _write = write;  _mapper = mapper; }

        public async Task<Result<RoomBedTypeDto>> Handle(UpdateRoomBedTypeNameCommand request, CancellationToken ct)
        {
            var entity = await _read.GetByIdAsync(request.Id, ct);
            if (entity is null) return Result<RoomBedTypeDto>.Failure("Not found");

            // Basit çakışma kontrolü
            var list = await _read.ListAsync(request.Name, ct);
            if (list.Any(x => x.Id != request.Id &&
                              x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                return Result<RoomBedTypeDto>.Failure("Name already exists.");

            entity.Rename(request.Name);
            await _write.UpdateAsync(entity, ct);

            var dto = _mapper.Map<RoomBedTypeDto>(entity);
            return Result<RoomBedTypeDto>.Success(dto);
        }
    }
}
