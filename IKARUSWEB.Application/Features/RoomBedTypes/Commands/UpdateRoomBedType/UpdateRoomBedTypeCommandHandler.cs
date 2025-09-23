using AutoMapper;
using IKARUSWEB.Application.Abstractions.Repositories.RoomBedTypeRepositories;
using IKARUSWEB.Application.Common.Results;
using IKARUSWEB.Application.Features.RoomBedTypes.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Features.RoomBedTypes.Commands.UpdateRoomBedType
{
    public sealed class UpdateRoomBedTypeCommandHandler
     : IRequestHandler<UpdateRoomBedTypeCommand, Result<RoomBedTypeDto>>
    {
        private readonly IRoomBedTypeReadRepository _read;
        private readonly IRoomBedTypeWriteRepository _write;
        private readonly IMapper _mapper;

        public UpdateRoomBedTypeCommandHandler(
            IRoomBedTypeReadRepository read,
            IRoomBedTypeWriteRepository write,
            IMapper mapper)
        {
            _read = read;
            _write = write;
            _mapper = mapper;
        }

        public async Task<Result<RoomBedTypeDto>> Handle(UpdateRoomBedTypeCommand request, CancellationToken ct)
        {
            // 1) Kayıt mevcut mu? + tenant kontrolü
            var entity = await _read.GetByIdAsync(request.Id, ct);
            if (entity is null || entity.TenantId != request.TenantId)
                return Result<RoomBedTypeDto>.Failure("Not found");

            // 2) İsim benzersizliği (tenant scope'unda)
            var exists = await _read.ExistsByNameAsync(request.TenantId, request.Name, excludeId: request.Id, ct);
            if (exists)
                return Result<RoomBedTypeDto>.Failure("Name already exists.");

            // 3) Full update (Name + Code + Description)
            entity.UpdateDetails(request.Name, request.Code, request.Description);

            await _write.UpdateAsync(entity, ct);

            var dto = _mapper.Map<RoomBedTypeDto>(entity);
            return Result<RoomBedTypeDto>.Success(dto);
        }
    }
}
