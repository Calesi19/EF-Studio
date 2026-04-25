---
layout: default
title: EFStudio
description: Minimal visual database studio for EF Core apps.
---

<section class="hero">
  <div class="panel">
    <span class="eyebrow">Minimal docs. Fast setup. Clear workflow.</span>
    <h1>Inspect your EF Core data without leaving the app.</h1>
    <p>
      EFStudio is a Prisma Studio-style visual database editor for ASP.NET Core
      apps. Plug it into your existing project, let it discover your
      <code>DbContext</code>, and inspect tables and records directly in the
      browser.
    </p>
    <div class="hero-actions">
      <a class="button button-primary" href="{{ '/installation/' | relative_url }}">Install EFStudio</a>
      <a class="button button-secondary" href="{{ '/usage/' | relative_url }}">See usage examples</a>
    </div>
    <div class="stat-grid">
      <div class="stat">
        <strong>Single-line setup</strong>
        Uses your existing EF Core configuration and middleware pipeline.
      </div>
      <div class="stat">
        <strong>Context-aware</strong>
        Reads EF metadata so relations and mapped tables stay aligned with the app.
      </div>
      <div class="stat">
        <strong>Development-only</strong>
        Intended for local and internal development workflows, not production access.
      </div>
    </div>
  </div>

  <aside class="panel hero-visual">
    <div>
      <span class="eyebrow">Product preview</span>
      <p>
        Familiar table browsing, lightweight setup, and no extra database client
        to maintain.
      </p>
    </div>
    <img
      src="{{ '/assets/images/banner.webp' | relative_url }}"
      alt="EFStudio interface preview"
    />
  </aside>
</section>

<section class="section" markdown="1">
  <div class="section-heading">
    <h2>Why EFStudio</h2>
    <p>
      Database inspection tools are useful, but switching out of your app slows
      down development. EFStudio keeps schema browsing and record inspection in
      the same project context as your API.
    </p>
  </div>
  <div class="feature-grid">
    <article class="feature-card">
      <h3>Zero separate connection setup</h3>
      <p>
        If your app already connects to the database, EFStudio can use that
        setup. No duplicate credentials or external tooling required.
      </p>
    </article>
    <article class="feature-card">
      <h3>Shaped by EF Core metadata</h3>
      <p>
        The studio understands your mapped entities, keys, and relationships
        through the same model your application already runs on.
      </p>
    </article>
    <article class="feature-card">
      <h3>Purpose-built UI</h3>
      <p>
        The interface follows the same clean visual language as the app: simple
        surfaces, strong spacing, and concise controls.
      </p>
    </article>
    <article class="feature-card">
      <h3>Focused scope</h3>
      <p>
        EFStudio is intentionally narrow. It is a development tool for quick
        inspection, not a heavyweight database administration console.
      </p>
    </article>
  </div>
</section>

<section class="section" markdown="1">
  <div class="panel">
    <div class="section-heading">
      <h2>Quick start</h2>
      <p>Install the package, register your <code>DbContext</code>, and expose the middleware in development.</p>
    </div>

```bash
dotnet add package EFStudio
```

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

    <div class="callout">
      EFStudio is served at <code>/efstudio</code> by default. Example:
      <code>http://localhost:5000/efstudio</code>.
    </div>
  </div>
</section>

<section class="section">
  <div class="cta-row">
    <a class="button button-primary" href="{{ '/features/' | relative_url }}">Explore features</a>
    <a class="button button-secondary" href="https://github.com/carloslespin/EF-Studio">View repository</a>
  </div>
</section>
