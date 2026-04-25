namespace EFStudio.Core.Services;

internal sealed class EFStudioRequestException : Exception
{
    public int StatusCode { get; }

    public EFStudioRequestException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
