namespace EFStudio.Core.Services;

public sealed class EFStudioRequestException : Exception
{
    public int StatusCode { get; }

    public EFStudioRequestException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
