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
    and a workflow that stays close to your project instead of requiring app
    integration.
  </p>
</section>

<section class="info-grid">
  <article class="info-card">
    <h3>Auto-discovery</h3>
    <p>
      EFStudio finds your target project, builds it, and exposes available
      <code>DbContext</code> types without a separate schema definition step.
    </p>
  </article>
  <article class="info-card">
    <h3>Zero app wiring</h3>
    <p>
      Install the tool once and run it when needed. No package registration,
      service setup, or middleware changes are required in your application.
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
    <h3>Local-only hosting</h3>
    <p>
      EFStudio hosts the studio on localhost, opens your browser automatically,
      and keeps the inspection surface local to your machine.
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
        <td>Run the tool and inspect records directly against the project's EF Core setup.</td>
      </tr>
      <tr>
        <td>Validating mappings</td>
        <td>Confirm tables, keys, and relationships line up with the EF Core model.</td>
      </tr>
      <tr>
        <td>Debugging local environments</td>
        <td>Verify data state quickly during API or UI development without an external admin client.</td>
      </tr>
      <tr>
        <td>Team onboarding</td>
        <td>Give developers a visual way to understand the data model in a new project with one CLI command.</td>
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
