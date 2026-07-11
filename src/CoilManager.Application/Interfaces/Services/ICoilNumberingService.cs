namespace CoilManager.Application.Interfaces.Services;

public interface ICoilNumberingService
{
    string GenerateFirstGenerationSlitCoilNumber(string motherCoilNumber, int slitSequence);
    string GenerateChildSlitCoilNumber(string parentCoilNumber, int slitSequence);
}
