namespace CoilManager.Shared.Exceptions;

public sealed class UnauthorizedException(string message) : Exception(message);
