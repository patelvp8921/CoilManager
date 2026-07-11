using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Mappings;

public static class SlitCoilDtoMapper
{
    public static SlitCoilDto MapToDto(SlitCoil coil)
    {
        RawCoil motherCoil = coil.MotherCoil ?? throw new InvalidOperationException("Slit coil mother coil was not loaded.");
        SlittingJob job = coil.SlittingJob ?? throw new InvalidOperationException("Slit coil slitting job was not loaded.");

        return new SlitCoilDto(
            coil.Id,
            coil.CoilNumber,
            coil.MotherCoilId,
            motherCoil.RawCoilNumber,
            coil.SlittingJobId,
            job.SlittingJobNo,
            coil.Grade?.Code,
            coil.Thickness,
            coil.Category,
            coil.CoreLossPerKg,
            coil.Width,
            coil.Weight,
            coil.Status,
            coil.WarehouseLocation,
            coil.BarcodeValue,
            coil.QrCodeValue,
            coil.LabelVersion,
            coil.CreatedAtUtc);
    }

    public static GeneratedSlitCoilDto MapToGeneratedDto(SlitCoil coil)
    {
        RawCoil motherCoil = coil.MotherCoil ?? throw new InvalidOperationException("Slit coil mother coil was not loaded.");
        SlittingJob job = coil.SlittingJob ?? throw new InvalidOperationException("Slit coil slitting job was not loaded.");

        return new GeneratedSlitCoilDto(
            coil.Id,
            coil.CoilNumber,
            motherCoil.RawCoilNumber,
            motherCoil.RawCoilNumber,
            job.SlittingJobNo,
            coil.Width,
            coil.Weight,
            coil.Grade?.Code,
            coil.Thickness,
            coil.Category,
            coil.CoreLossPerKg,
            coil.Status,
            coil.BarcodeValue,
            coil.QrCodeValue,
            coil.LabelVersion);
    }
}
