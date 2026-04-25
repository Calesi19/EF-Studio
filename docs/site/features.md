---
layout: default
title: Features
description: EFStudio feature overview.
permalink: /features/
---

<section class="page-intro panel">
  <span class="eyebrow">Features</span>
  <h1>Small surface area, useful defaults.</h1>
  <p>
    EFStudio focuses on the parts of database inspection that matter most during
    application development: fast access to schema structure, record browsing,
    and an interface that feels native to the app instead of bolted on.
  </p>
</section>

<section class="info-grid">
  <article class="info-card">
    <h3>Auto-discovery</h3>
    <p>
      EFStudio maps your EF Core model and exposes table information without a
      separate schema definition step.
    </p>
  </article>
  <article class="info-card">
    <h3>Embedded workflow</h3>
    <p>
      The studio lives inside the ASP.NET Core app and is available through a
      predictable route in development.
    </p>
  </article>
  <article class="info-card">
    <h3>Read-first usage</h3>
    <p>
      The current shape is intentionally conservative: focused on inspection and
      safe local exploration.
    </p>
  </article>
  <article class="info-card">
    <h3>Low setup cost</h3>
    <p>
      Package install, service registration, middleware. That is the entire
      integration surface for a typical app.
    </p>
  </article>
</section>

<section class="section panel">
  <div class="section-heading">
    <h2>What it helps with</h2>
    <p>
      EFStudio is most useful when you need to inspect real application data
      while debugging, validating migrations, or understanding how a model is
      being materialized at runtime.
    </p>
  </div>
  <table>
    <thead>
      <tr>
        <th>Use case</th>
        <th>How EFStudio helps</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Checking seed data</td>
        <td>Open the studio and inspect records without leaving the app context.</td>
      </tr>
      <tr>
        <td>Validating mappings</td>
        <td>Confirm tables, keys, and relationships line up with the EF Core model.</td>
      </tr>
      <tr>
        <td>Debugging local environments</td>
        <td>Verify data state quickly during API or UI development.</td>
      </tr>
      <tr>
        <td>Team onboarding</td>
        <td>Give developers a visual way to understand the data model in a new project.</td>
      </tr>
    </tbody>
  </table>
</section>

<section class="section panel">
  <h2>Current direction</h2>
  <p>
    Planned improvements in the repository roadmap include richer editing
    support, export options, and broader database provider coverage. The current
    experience stays intentionally simple so the core workflow remains clear.
  </p>
</section>
