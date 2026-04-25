---
layout: default
title: Installation
description: Install EFStudio as a dotnet global tool and point it at your EF Core project.
permalink: /installation/
---

<section class="page-intro panel">
  <span class="eyebrow">Installation</span>
  <h1>Install the tool and run it against your project.</h1>
  <p>
    EFStudio is delivered as a dotnet global tool. It reuses your project's EF
    Core configuration and design-time setup rather than asking you to register
    middleware or duplicate database settings.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>1. Install the global tool</h2>

```bash
dotnet tool install --global EFStudio.Tool
```

  <p>
    Once installed, the command is <code>dotnet efstudio</code>.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>2. Run EFStudio from your project directory</h2>

```bash
dotnet efstudio
```

  <div class="callout">
    EFStudio expects a buildable .NET project with a working EF Core provider
    and a way to create the target <code>DbContext</code>.
  </div>
</section>

<section class="section panel" markdown="1">
  <h2>3. Target a specific project when needed</h2>

```bash
dotnet efstudio --project ./MyDataProject.csproj
dotnet efstudio --startup-project ./MyApi.csproj
dotnet efstudio --context AppDbContext
```

  <p>
    Use <code>--project</code> when the current directory is not the project you
    want to inspect, <code>--startup-project</code> when the app startup lives in
    a different project, and <code>--context</code> to preselect a specific
    <code>DbContext</code>.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>4. Browser behavior</h2>

```bash
dotnet efstudio --port 5123
dotnet efstudio --no-browser
```

  <p>
    EFStudio binds to a localhost port, opens the browser by default, and hosts
    the studio at <code>/efstudio</code>. Use <code>--no-browser</code> if you
    want to copy the URL manually.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>Requirements</h2>
  <ul>
    <li>.NET SDK 10.0 or higher</li>
    <li>Entity Framework Core 6.0+</li>
    <li>A buildable .NET project that can create your target <code>DbContext</code></li>
    <li>An EF Core provider for your database</li>
    <li><code>Npgsql.EntityFrameworkCore.PostgreSQL</code> for PostgreSQL</li>
    <li><code>Microsoft.EntityFrameworkCore.Sqlite</code> for SQLite</li>
  </ul>
</section>
