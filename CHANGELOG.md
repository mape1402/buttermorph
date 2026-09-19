# Changelog

All notable changes to ButterMorph will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project follows semantic versioning.

---

## [v1.1.3] - 2026-09-19

### Fixed
- Fixed required field-level metadata validation so applied values and values still open in the Field Metadata modal are synchronized before schema save.
- Added regression coverage for the payload schema metadata sync flow and refreshed schema builder asset cache-busting.

---

## [v1.1.2] - 2026-09-19

### Fixed
- Fixed Custom Fields Allowed Values styling in dark mode.
- Fixed required field metadata validation when saved values are wrapped or nested.

---

## [v1.1.1] - 2026-09-18

### Changed
- Changed designer theming to configure separate `Light` and `Dark` palettes once during service registration.
- Updated live theme synchronization so hosts switch modes with `ButterMorphHost.setThemeMode(...)` without resending palette colors.

---

## [v1.1.0] - 2026-09-18

### Added
- Added host-configurable designer themes for `ButterMorph.Web.Razor`.
- Added built-in light and dark designer modes.
- Added live theme synchronization for embedded iframes and popups through `ButterMorphHost.setThemeMode(...)` and the `ButterMorphThemeChanged` message.
- Added theme documentation and integration tests for configured colors and dark mode rendering.

---

## [v1.0.2] - 2026-09-09

### Changed
- Multi-targeted NuGet projects for `net9.0` and `net10.0`.
- Replaced separate build, changelog, release, and publish workflows with the Mule-style unified Build and Release workflow.
- Updated README CI badge to point at the new workflow.

---

## [v1.0.1] - 2026-07-13

### Fixed
- Improved Schema Designer validation feedback across payload schemas, custom types, and custom fields.
- Replaced inline validation banners with a centered message box that shows a generic error and collapsible details.
- Blocked silent saves when custom type, custom field, schema field, or metadata validation fails.
- Kept custom type payload fields closed in payload schemas so validations and nested structure remain owned by the custom type definition.
- Validated required field-level metadata consistently with schema-level metadata.

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
