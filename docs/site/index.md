---
layout: default
title: EFStudio
description: A visual database studio for EF Core projects.
---

# EFStudio

<p class="tagline">A visual database studio for EF Core projects.</p>

EFStudio runs as a `.NET global tool`. Point it at a project that contains your EF Core setup, let it discover your `DbContext`, and inspect your data in a local browser UI without wiring anything into `Program.cs`.

It is designed for local development only. EFStudio starts a local server, opens the studio in your browser, and keeps the inspection workflow outside your application startup path.

<img
  class="banner"
  src="{{ '/assets/images/demo.webp' | relative_url }}"
  alt="EFStudio interface — a table browser showing EF Core entities"
/>

## Install

<div class="code-copy-block">
  <pre><code>dotnet tool install --global EFStudio</code></pre>
  <button class="copy-btn" aria-label="Copy to clipboard">Copy</button>
</div>

## Run it

From the project directory that contains your EF Core app:

```bash
dotnet efstudio
```

<p class="note">EFStudio hosts a local studio at <code>/efstudio</code> on a localhost URL such as <code>http://127.0.0.1:5123/efstudio</code>.</p>

## Common options

```bash
dotnet efstudio --project ./SomeProject.csproj
dotnet efstudio --startup-project ./SomeApi.csproj
dotnet efstudio --context AppDbContext
dotnet efstudio --port 5123
dotnet efstudio --no-browser
```
