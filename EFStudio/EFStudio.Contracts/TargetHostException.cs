namespace EFStudio.Contracts;

public sealed class TargetHostException : Exception
{
    public int StatusCode { get; }

    public TargetHostException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public TargetHostException(int statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
