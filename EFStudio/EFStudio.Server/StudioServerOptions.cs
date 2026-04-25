namespace EFStudio.Server;

public sealed record StudioServerOptions(string Url)
{
    public Uri BaseUri => new(Url);
    public Uri StudioUri => new(BaseUri, "/efstudio");
}
