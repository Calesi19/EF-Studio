public class ToolOptionsTests
{
    [Fact]
    public void Parse_NoArgs_ReturnsNullPaths_ShowHelpFalse()
    {
        var options = ToolOptions.Parse([]);
        Assert.Null(options.ProjectPath);
        Assert.Null(options.StartupProjectPath);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void Parse_WithProjectFlag_SetsProjectPath()
    {
        var options = ToolOptions.Parse(["--project", "MyApp/MyApp.csproj"]);
        Assert.Equal("MyApp/MyApp.csproj", options.ProjectPath);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void Parse_WithStartupProjectFlag_SetsStartupProjectPath()
    {
        var options = ToolOptions.Parse(["--startup-project", "MyApi/MyApi.csproj"]);
        Assert.Equal("MyApi/MyApi.csproj", options.StartupProjectPath);
        Assert.Null(options.ProjectPath);
    }

    [Fact]
    public void Parse_WithBothProjectAndStartupProject()
    {
        var options = ToolOptions.Parse([
            "--project", "Core/Core.csproj",
            "--startup-project", "Api/Api.csproj",
        ]);
        Assert.Equal("Core/Core.csproj", options.ProjectPath);
        Assert.Equal("Api/Api.csproj", options.StartupProjectPath);
    }

    [Fact]
    public void Parse_WithHelpFlag_SetsShowHelp()
    {
        var options = ToolOptions.Parse(["--help"]);
        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void Parse_WithShortHelpFlag_SetsShowHelp()
    {
        var options = ToolOptions.Parse(["-h"]);
        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void Parse_ProjectFlagWithoutValue_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ToolOptions.Parse(["--project"]));
    }

    [Fact]
    public void Parse_StartupProjectFlagWithoutValue_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ToolOptions.Parse(["--startup-project"]));
    }

    [Fact]
    public void WriteHelp_WritesUsageAndOptions()
    {
        using var writer = new StringWriter();
        ToolOptions.WriteHelp(writer);
        var output = writer.ToString();

        Assert.Contains("Usage:", output);
        Assert.Contains("--project", output);
        Assert.Contains("--startup-project", output);
        Assert.Contains("--help", output);
    }
}
