using AutoMapper;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Mappings;

public sealed class RawCoilMappingProfile : Profile
{
    public RawCoilMappingProfile()
    {
        CreateMap<RawCoil, RawCoilDto>()
            .ForCtorParam(nameof(RawCoilDto.RowVersion), options => options.MapFrom(rawCoil => Convert.ToBase64String(rawCoil.RowVersion)))
            .ForCtorParam(nameof(RawCoilDto.ModifiedBy), options => options.MapFrom(rawCoil => rawCoil.UpdatedBy))
            .ForCtorParam(nameof(RawCoilDto.DocumentPlaceholders), options => options.MapFrom(_ => Array.Empty<string>()));
    }
}
