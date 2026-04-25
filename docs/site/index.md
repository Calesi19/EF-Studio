---
layout: default
title: EFStudio
description: A Prisma Studio-style visual database editor for .NET and ASP.NET Core apps.
---

# EFStudio

<p class="tagline">A Prisma Studio-style visual database editor for .NET and ASP.NET Core.</p>

EFStudio embeds a visual table browser directly into your ASP.NET Core app. Point it at your existing `DbContext`, start the app, and inspect your data at `/efstudio` — no separate database client needed.

It is designed for development only. The middleware is intentionally guarded so it never runs in production.

<img
  class="banner"
  src="{{ '/assets/images/banner.webp' | relative_url }}"
  alt="EFStudio interface — a table browser showing EF Core entities"
/>

## Install

<div class="code-copy-block">
  <pre><code>dotnet add package EFStudio</code></pre>
  <button class="copy-btn" aria-label="Copy to clipboard">Copy</button>
</div>

## Wire it up

Add the service and middleware in `Program.cs`:

```csharp
using EFStudio.Core.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddEFStudio<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseEFStudio();
}

app.Run();
```

<p class="note">EFStudio is served at <code>/efstudio</code> by default — for example, <code>http://localhost:5000/efstudio</code>.</p>
