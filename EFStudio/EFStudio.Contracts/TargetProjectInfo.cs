namespace EFStudio.Contracts;

public sealed record TargetProjectInfo(
    string ProjectPath,
    string Framework,
    string TargetPath,
    string ProjectDirectory,
    string AssemblyName
);
