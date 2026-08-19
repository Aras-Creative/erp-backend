namespace ArasERP.BuildingBlocks.Application;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyList<string> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : base("Validation failed")
    {
        Errors = [error];
    }

    public IReadOnlyList<string> Errors { get; }
}
