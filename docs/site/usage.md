---
layout: default
title: Usage
description: Usage examples for EFStudio.
permalink: /usage/
---

<section class="page-intro panel">
  <span class="eyebrow">Usage</span>
  <h1>Keep database inspection inside the application workflow.</h1>
  <p>
    After setup, EFStudio becomes part of the same local loop as your API. Start
    the app, open the studio route, and inspect the data model without switching
    to an external client.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>Full example</h2>

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
</section>

<section class="section info-grid">
  <article class="info-card">
    <h3>Open the studio</h3>
    <p>
      Browse to <code>/efstudio</code> after starting the app. Example:
      <code>http://localhost:5000/efstudio</code>.
    </p>
  </article>
  <article class="info-card">
    <h3>Inspect schema</h3>
    <p>
      Use the UI to move through discovered tables and understand how your
      entities are mapped.
    </p>
  </article>
  <article class="info-card">
    <h3>Review records</h3>
    <p>
      Browse and filter the current data without setting up an external admin
      client.
    </p>
  </article>
  <article class="info-card">
    <h3>Stay in development mode</h3>
    <p>
      The intended use case is local and internal development, not production
      exposure.
    </p>
  </article>
</section>

<section class="section panel" markdown="1">
  <h2>Typical workflow</h2>
  <ol>
    <li>Start the ASP.NET Core app with your normal local configuration.</li>
    <li>Open <code>/efstudio</code> in the browser.</li>
    <li>Inspect schema and records while developing or debugging.</li>
    <li>Close the tool when you are done. No separate admin stack is required.</li>
  </ol>
</section>
