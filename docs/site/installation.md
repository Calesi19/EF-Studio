---
layout: default
title: Installation
description: Install and configure EFStudio in an ASP.NET Core app.
permalink: /installation/
---

<section class="page-intro panel">
  <span class="eyebrow">Installation</span>
  <h1>Install the package and register the middleware.</h1>
  <p>
    EFStudio is designed to fit into an existing ASP.NET Core application with
    minimal setup. It reuses the app’s EF Core configuration rather than asking
    you to define a second database connection.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>1. Add the package</h2>

```bash
dotnet add package EFStudio
```

  <p>
    The package name is <code>EFStudio</code>. The current extension method
    namespace in the repo is <code>EFStudio.Core.Extensions</code>.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>2. Register EFStudio with your <code>DbContext</code></h2>

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddEFStudio<AppDbContext>();
```

  <div class="callout">
    EFStudio expects your application to already be configured with a working EF
    Core provider.
  </div>
</section>

<section class="section panel" markdown="1">
  <h2>3. Expose the studio in development</h2>

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseEFStudio();
}
```

  <p>
    The middleware is intentionally intended for development environments only.
    Keep that guard in place.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>Requirements</h2>
  <ul>
    <li>.NET 6.0 or higher</li>
    <li>Entity Framework Core 6.0+</li>
    <li>An EF Core provider for your database</li>
    <li><code>Npgsql.EntityFrameworkCore.PostgreSQL</code> for PostgreSQL</li>
    <li><code>Microsoft.EntityFrameworkCore.Sqlite</code> for SQLite</li>
  </ul>
</section>
