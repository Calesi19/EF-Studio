---
layout: default
title: Usage
description: Usage examples for the EFStudio dotnet global tool.
permalink: /usage/
---

<section class="page-intro panel">
  <span class="eyebrow">Usage</span>
  <h1>Run EFStudio when you need to inspect a project.</h1>
  <p>
    EFStudio fits into the normal local development loop without changing your
    app startup. Run the tool, let it discover your context, and inspect schema
    and records in a browser tab.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>Basic command</h2>

```bash
dotnet efstudio
```
</section>

<section class="section info-grid">
  <article class="info-card">
    <h3>Discover contexts</h3>
    <p>
      Run the tool in a project directory and EFStudio will discover the
      available <code>DbContext</code> types it can activate.
    </p>
  </article>
  <article class="info-card">
    <h3>Open the studio</h3>
    <p>
      EFStudio starts a local server and opens the browser automatically unless
      you pass <code>--no-browser</code>.
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
      Browse paged records locally without opening a separate database client.
    </p>
  </article>
</section>

<section class="section panel" markdown="1">
  <h2>Command examples</h2>

```bash
dotnet efstudio --project ./MyProject.csproj
dotnet efstudio --startup-project ./MyApi.csproj
dotnet efstudio --context AppDbContext
dotnet efstudio --port 5123
dotnet efstudio --no-browser
```
</section>

<section class="section panel" markdown="1">
  <h2>Context activation</h2>
  <p>
    EFStudio prefers <code>IDesignTimeDbContextFactory&lt;TContext&gt;</code>
    when creating a context. If no design-time factory exists, it will try to
    create the startup project's service provider through a conventional startup
    builder such as <code>CreateHostBuilder</code>. If neither path works,
    EFStudio returns a clear error so you can fix the project's design-time
    setup.
  </p>
</section>

<section class="section panel" markdown="1">
  <h2>Typical workflow</h2>
  <ol>
    <li>Open a terminal in the project directory you want to inspect.</li>
    <li>Run <code>dotnet efstudio</code>.</li>
    <li>Let EFStudio build the project and discover the available contexts.</li>
    <li>Inspect schema and records in the browser.</li>
    <li>Stop the tool when you are done.</li>
  </ol>
</section>
