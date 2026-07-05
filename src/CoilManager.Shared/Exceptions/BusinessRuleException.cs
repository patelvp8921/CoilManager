namespace CoilManager.Shared.Exceptions;

public sealed class BusinessRuleException(string message) : Exception(message);
