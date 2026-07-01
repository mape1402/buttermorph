# Changelog

All notable changes to ButterMorph will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project follows semantic versioning.

---

## [v1.0.0] - 2026-06-30

### Added
- Initial modular ButterMorph package set:
  - `ButterMorph` for core runtime, typed transformation model, expressions, native functions, validation, semantics, DSL, modeling builders, and dependency injection.
  - `ButterMorph.Json` for JSON structure graph reader/writer adapters.
  - `ButterMorph.Json.Schema` for JSON Schema import/export compatibility.
  - `ButterMorph.SchemaDesign` for custom type, custom field, payload schema, metadata definition, and schema rehydration services.
  - `ButterMorph.Design` for headless mapping designer sessions, schema exploration, capability exploration, DSL import/export, and diagnostics.
  - `ButterMorph.Web.Razor` for reusable Razor Pages designers.
- Reusable Mapping Designer:
  - source toolbox with collapsible source schemas;
  - function toolbox with native/custom function discovery;
  - visual target mapping canvas;
  - array projection editing;
  - DSL mini IDE with syntax highlighting, autocomplete, function tooltips, snippets, inline diagnostics, and error list panel;
  - host preload/save flow for embedded integrations.
- Schema design tooling:
  - custom data type designer;
  - versioned custom field designer;
  - payload schema designer;
  - schema-level and field-level metadata capture;
  - custom type/custom field catalog injection controlled by host applications;
  - schema rehydration from clean ButterMorph definitions.
- Runtime and modeling APIs:
  - public fluent builders through `ButterMorphModel`;
  - native function catalog and descriptors;
  - semantic analysis for mapping documents;
  - DSL parser/exporter roundtrip support;
  - JSON graph execution path through `IButterMorphEngine`.
- Host integration support:
  - Razor route mapping through `MapButterMorphDesigner`;
  - host callback interfaces for mappings, custom types, custom fields, and schemas;
  - iframe/modal host helper flow;
  - clean save payloads separated from operational diagnostics.
- Samples:
  - `ButterMorph.Playground` as a simple host-flow demo;
  - `ButterMorph.StudioPlayground` as a structured host simulation with CRUD, catalog injection, mapping design, and execution.
- Repository/package infrastructure:
  - centralized build properties;
  - centralized package versions;
  - NuGet package metadata, README, license, changelog, and package icon;
  - GitHub Actions for build/test/pack, changelog checks, release creation, NuGet publishing, and Dependabot.

### Changed
- Consolidated core projects into a smaller NuGet-friendly package structure.
- Moved schema rehydration responsibility into ButterMorph instead of sample hosts.
- Kept playgrounds as host examples only: persistence, listing, launch, injection, and display.

### Fixed
- Mapping designer saved-state rehydration from DSL.
- Schema designer editing for saved custom types, custom fields, metadata values, and custom type references.
- Studio playground persistence and refresh flow for host-owned data.
- NuGet packaging warnings for README/icon metadata.
