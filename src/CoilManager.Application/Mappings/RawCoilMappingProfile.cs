using AutoMapper;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Mappings;

public sealed class RawCoilMappingProfile : Profile
{
    public RawCoilMappingProfile()
    {
        CreateMap<RawCoil, RawCoilDto>()
            .ForCtorParam(nameof(RawCoilDto.SupplierName), options => options.MapFrom(rawCoil => rawCoil.Supplier == null ? string.Empty : rawCoil.Supplier.Name))
            .ForCtorParam(nameof(RawCoilDto.ManufacturerName), options => options.MapFrom(rawCoil => rawCoil.Manufacturer == null ? string.Empty : rawCoil.Manufacturer.Name))
            .ForCtorParam(nameof(RawCoilDto.Grade), options => options.MapFrom(rawCoil => rawCoil.Grade == null ? string.Empty : rawCoil.Grade.Code))
            .ForCtorParam(nameof(RawCoilDto.RowVersion), options => options.MapFrom(rawCoil => Convert.ToBase64String(rawCoil.RowVersion)))
            .ForCtorParam(nameof(RawCoilDto.ModifiedBy), options => options.MapFrom(rawCoil => rawCoil.UpdatedBy))
            .ForCtorParam(nameof(RawCoilDto.DocumentPlaceholders), options => options.MapFrom(_ => Array.Empty<string>()));
    }
}
