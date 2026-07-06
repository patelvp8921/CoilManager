namespace CoilManager.Application.Services;

public static class RawCoilNumberGenerator
{
    public static string Generate(int year, int sequence)
    {
        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be greater than zero.");
        }

        return $"RC-{year}-{sequence:0000000}";
    }
}
