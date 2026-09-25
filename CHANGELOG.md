# Changelog

All notable changes to ButterMorph will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project follows semantic versioning.

---

## [v2.0.0] - 2026-09-25

### Added
- Added first-class `ValidationDocument` support with parser/exporter roundtrips, modeling helpers, design sessions, runtime execution, and host save/load contracts.
- Added the reusable Validation Designer at `/buttermorph/validations/designer` with separate visual and DSL views.
- Added explicit payload validation against schemas, including required fields, scalar type checks, string length, numeric restrictions, arrays, enum/allowed values, and temporal restrictions.
- Added validation assertions with field references, logical groups, conditional `when(...)` branches, nested `when(...)` branches, and `foreach` item validation with the default `item` alias.
- Added temporal primitive handling for `Date`, `DateTime`, `Time`, and `TimeSpan`, plus temporal parsing and validation functions.
- Added validation-oriented function discovery alongside transformation functions in designer tooling.
- Added source metadata support in the mapping designer so hosts can provide display name, description, and tags while keeping technical source IDs out of the main panel.
- Added visual conditional mapping composition for `when(condition, thenExpression, elseExpression)`, including nested conditionals and conditional fields inside array projections.

### Changed
- Separated mapping and validation workflows: `MappingDocument` remains for transformation, while `ValidationDocument` owns explicit validation rules.
- Validation is now an explicit runtime operation; transforms do not automatically validate payloads unless the host asks for validation.
- Changed mapping and validation source IDs to require DSL-safe identifiers: aliases must start with a letter or underscore and may contain only letters, digits, and underscores.
- Refined mapping designer controls for conditional mappings, clearer clear/remove actions, compact value slots, and less native-looking select controls.

### Fixed
- Fixed field-reference handling in function arguments so expressions can use source fields as function parameters instead of only direct literal values.
- Fixed mapping designer save feedback so visual users receive diagnostics when a mapping cannot be saved.

### Compatibility
- Existing mapping documents remain loadable through the same DSL model. Documents using conditional mappings continue to load as `when(...)` expressions.
- Stored mappings that used source aliases with hyphens should be migrated to identifier-safe aliases before editing or executing in v2.0.0.

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
